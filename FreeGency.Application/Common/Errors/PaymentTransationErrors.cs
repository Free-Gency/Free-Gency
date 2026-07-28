using FreeGency.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace FreeGency.Application.Common.Errors
{
    public static class PaymentTransationErrors
    {
        public static readonly Error NotFound =
        new(
            "PaymentTransaction.NotFound",
            "Payment transaction was not found.",
            StatusCodes.Status404NotFound);

        public static readonly Error AlreadyCompleted =
            new(
                "PaymentTransaction.AlreadyCompleted",
                "Payment transaction has already been completed.",
                StatusCodes.Status409Conflict);

        public static readonly Error AlreadyRefunded =
            new(
                "PaymentTransaction.AlreadyRefunded",
                "Payment transaction has already been refunded.",
                StatusCodes.Status409Conflict);

        public static readonly Error InvalidStatus =
            new(
                "PaymentTransaction.InvalidStatus",
                "Payment transaction status is invalid.",
                StatusCodes.Status400BadRequest);

        public static readonly Error PaymentIntentNotFound =
            new(
                "PaymentTransaction.PaymentIntentNotFound",
                "Payment intent was not found.",
                StatusCodes.Status404NotFound);

        public static readonly Error ChargeNotFound =
            new(
                "PaymentTransaction.ChargeNotFound",
                "Charge was not found.",
                StatusCodes.Status404NotFound);

        public static readonly Error RefundFailed =
            new(
                "PaymentTransaction.RefundFailed",
                "Failed to process the refund.",
                StatusCodes.Status400BadRequest);

        public static readonly Error WebhookVerificationFailed =
            new(
                "PaymentTransaction.WebhookVerificationFailed",
                "Stripe webhook signature verification failed.",
                StatusCodes.Status401Unauthorized);

        public static readonly Error CurrencyMismatch =
            new(
                "PaymentTransaction.CurrencyMismatch",
                "Payment currency does not match the expected currency.",
                StatusCodes.Status400BadRequest);

        public static readonly Error AmountMismatch =
            new(
                "PaymentTransaction.AmountMismatch",
                "Payment amount does not match the expected amount.",
                StatusCodes.Status400BadRequest);
    }
}
