using FreeGency.Api.Extensions;
using FreeGency.Application.Features.WalletFeature.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FreeGency.Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class WalletController(IWalletService walletService) : ControllerBase
    {
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
    }
}
