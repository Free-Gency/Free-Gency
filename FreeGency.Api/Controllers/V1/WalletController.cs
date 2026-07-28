using FreeGency.Api.Extensions;
using FreeGency.Application.Common.Helpers;
using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.WalletFeature.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;

namespace FreeGency.Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class WalletController(IWalletService walletService,IOptions<StripeSetting> options) : ControllerBase
    {
        private readonly StripeSetting _options = options.Value;

        [HttpGet("me")]
        public async Task<IActionResult> GetUserWallet()
        {
            var result = await walletService.GetUserWallet();
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
        [HttpPost("topup")]
        public async Task<IActionResult> TopUp(
                                                [FromBody] TopUpRequestDto dto)
        {
            var result = await walletService.CreateTopUpIntentAsync(dto);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem();
        }
        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            Result result;
            try
            {
                var stripeEvent = ConstructStripeEvent(json, Request.Headers["Stripe-Signature"]);
                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        result= await walletService.HandleSucceeded(
                            (PaymentIntent)stripeEvent.Data.Object);
                        break;

                    case "payment_intent.payment_failed":
                      result=  await walletService.HandleFailed(
                            (PaymentIntent)stripeEvent.Data.Object);
                        break;

                    case "payment_intent.canceled":
                      result=  await walletService.HandleCanceled(
                            (PaymentIntent)stripeEvent.Data.Object);
                        break;
                    default:
                        return Ok();
                }

                return result.IsSuccess?Ok():result.ToProblem();
            }
            catch (StripeException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private Event ConstructStripeEvent(string json, string? signature)
        {
            StripeException? lastError = null;
            foreach (var secret in EnumerateWebhookSecrets())
            {
                try
                {
                    return EventUtility.ConstructEvent(json, signature, secret);
                }
                catch (StripeException ex)
                {
                    lastError = ex;
                }
            }

            throw lastError ?? new StripeException("No Stripe webhook secrets configured.");
        }

        private IEnumerable<string> EnumerateWebhookSecrets()
        {
            if (!string.IsNullOrWhiteSpace(_options.WhSecret))
                yield return _options.WhSecret;
            if (!string.IsNullOrWhiteSpace(_options.CliWhSecret)
                && !string.Equals(_options.CliWhSecret, _options.WhSecret, StringComparison.Ordinal))
                yield return _options.CliWhSecret;
        }
    }
}
