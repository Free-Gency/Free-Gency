using FreeGency.Application.Features.WalletFeature.Dtos;
using Stripe;
using System;
using System.Collections.Generic;
using System.Text;

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
            return Result.Success(new TopUpResponseDto
            {
                ClientSecret = paymentIntent.ClientSecret,
                PaymentIntentId = paymentIntent.Id
            });
        }
    }
}
