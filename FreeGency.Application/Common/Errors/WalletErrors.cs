using FreeGency.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Errors
{
    public static class WalletErrors
    {
        public static readonly Error NotFound =
       new(
           "Wallet.NotFound",
           "Wallet was not found.",
           StatusCodes.Status404NotFound);

        public static readonly Error AlreadyExists =
            new(
                "Wallet.AlreadyExists",
                "Wallet already exists.",
                StatusCodes.Status409Conflict);

        public static readonly Error InsufficientBalance =
            new(
                "Wallet.InsufficientBalance",
                "Insufficient wallet balance.",
                StatusCodes.Status400BadRequest);

        public static readonly Error InvalidAmount =
            new(
                "Wallet.InvalidAmount",
                "The amount must be greater than zero.",
                StatusCodes.Status400BadRequest);

        public static readonly Error CurrencyMismatch =
            new(
                "Wallet.CurrencyMismatch",
                "Wallet currency does not match the transaction currency.",
                StatusCodes.Status400BadRequest);

        public static readonly Error WalletLocked =
            new(
                "Wallet.WalletLocked",
                "Wallet is locked.",
                StatusCodes.Status403Forbidden);

        public static readonly Error DepositFailed =
            new(
                "Wallet.DepositFailed",
                "Failed to deposit funds into the wallet.",
                StatusCodes.Status400BadRequest);

        public static readonly Error WithdrawalFailed =
            new(
                "Wallet.WithdrawalFailed",
                "Failed to withdraw funds from the wallet.",
                StatusCodes.Status400BadRequest);

        public static readonly Error TransferFailed =
            new(
                "Wallet.TransferFailed",
                "Failed to transfer funds.",
                StatusCodes.Status400BadRequest);
    }
}
