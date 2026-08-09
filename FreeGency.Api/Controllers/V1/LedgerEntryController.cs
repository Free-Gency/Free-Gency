using FreeGency.Api.Extensions;
using FreeGency.Application.Features.LedgerEntryFeature.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FreeGency.Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class LedgerEntryController(ILedgerEntryService ledgerEntryService) : ControllerBase
    {
        [HttpGet("me")]
        public async Task<IActionResult> GetLedger([FromQuery] EntryFilter entryFilter)
        {
            var result = await ledgerEntryService.GetLedgerEntryAsync(entryFilter);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
    }
}
