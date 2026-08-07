using FreeGency.Application.Common.Hubs;
using FreeGency.Application.Features.NotificationFeature.Dtos;
using FreeGency.Application.Features.WalletFeature.Dtos;
using FreeGency.Application.Features.WalletFeature.Mapping;
using FreeGency.Domain.Specifications;
using Microsoft.AspNetCore.SignalR;
using Stripe;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace FreeGency.Application.Features.WalletFeature.Queries
{
    public partial class WalletService
    {
        public async Task<Result<TopUpResponseDto>> CreateTopUpIntentAsync(TopUpRequestDto dto)
        {
            StripeConfiguration.ApiKey = _options.SecretKey;
            var paymentIntentService = new PaymentIntentService();

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(dto.Amount * 100),
                Currency = "USD",

                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                }

            };
            var paymentIntent =
                              await paymentIntentService.CreateAsync(options);
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty) return Result.Failure<TopUpResponseDto>(UserErrors.UserNotFound);
            var wallet = await walletRepository.GetEntityWithSpec(new WalletSpecification(userId));
            if (wallet == null) return Result.Failure<TopUpResponseDto>(UserErrors.UserNotFound);
            var transaction = new PaymentTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                PaymentProviderRef = paymentIntent.Id,
                Amount = dto.Amount,
                Currency = options.Currency,
                Status = PaymentStatus.Pending
            };
            await paymentTransactionRepository.AddAsync(transaction);
            await unitOfWork.SaveChangesAsync();
            return Result.Success(new TopUpResponseDto
            {
                ClientSecret = paymentIntent.ClientSecret,
                PaymentIntentId = paymentIntent.Id
            });
        }
        public async Task<Result> HandleSucceeded(PaymentIntent @object)
        {
            var transaction = await paymentTransactionRepository.GetEntityWithSpec(new paymentTransactionSpecification(@object.Id));
            if (transaction == null) return Result.Failure(PaymentTransationErrors.NotFound);
            if (transaction.Status == PaymentStatus.Succeeded) return Result.Success();
            var wallet = await walletRepository.GetByIdAsync(transaction.WalletId);
            if (wallet == null) return Result.Failure(WalletErrors.NotFound);
            wallet.Available += transaction.Amount;
            transaction.Status = PaymentStatus.Succeeded;
            var ledger = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                EntryType = EntryType.TopUp,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                PaymentProviderRef = transaction.PaymentProviderRef,
                IdempotencyKey = transaction.PaymentProviderRef!,
                CreatedAt = DateTime.UtcNow
            };
            await ledgerEntryRepository.AddAsync(ledger);
            await unitOfWork.SaveChangesAsync();
            var profileId = await userRepository.GetProfileId(wallet.OwnerUserId!.Value);
            var connections = NotificationHub.GetConnections( profileId);
            if (connections.Count > 0)
                await hub.Clients.Clients(connections).SendAsync("WalletUpdated", wallet.ToDto());
            var notificationRequest = new CreateNotificationRequest
            {
                Title = "Wallet topped up",
                Body = $"{transaction.Amount} {transaction.Currency} has been added to your wallet successfully.",
                UserId=wallet.OwnerUserId!.Value,
                Type = NotificationType.Wallet,
                ClientProfileId = null,
                DeveloperProfileId = null,
                Data = JsonSerializer.Serialize(new
                {
                    WalletId = wallet.Id,
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    
                }),
                ActionUrl= "/settings/payments"
            };
            await notificationService.CreateNotification(notificationRequest);
            return Result.Success();
        }
        public async  Task<Result> HandleCanceled(PaymentIntent @object)
        {
            var transaction = await paymentTransactionRepository.GetEntityWithSpec(new paymentTransactionSpecification(@object.Id));
            if (transaction == null) return Result.Failure(PaymentTransationErrors.NotFound);
            transaction.Status = PaymentStatus.Cancelled;
            await unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> HandleFailed(PaymentIntent @object)
        {
            var transaction = await paymentTransactionRepository.GetEntityWithSpec(new paymentTransactionSpecification(@object.Id));
            if (transaction == null) return Result.Failure(PaymentTransationErrors.NotFound);
            transaction.Status = PaymentStatus.Failed;
            transaction.FailureReason =
                                        @object.LastPaymentError?.Message;
            await unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

      
    }
}
