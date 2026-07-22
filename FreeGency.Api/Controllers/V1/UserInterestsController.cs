using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Features.userInterests.Dtos;

namespace FreeGency.Api.Controllers.V1;

[Authorize]
[Route("api/v1/me/interests")]
public class UserInterestsController(IUserInterestService _userInterestService) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetMyInterests(CancellationToken ct)
        => HandleResult(await _userInterestService.GetMyInterestsAsync(ct));

    [HttpPut]
    public async Task<IActionResult> ReplaceMyInterests([FromBody] ReplaceUserInterestsDto dto, CancellationToken ct)
        => HandleResult(await _userInterestService.ReplaceMyInterestsAsync(dto, ct));
}
