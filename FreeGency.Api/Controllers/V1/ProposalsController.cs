using FreeGency.Application.Features.Proposals.Dtos;


namespace FreeGency.Api.Controllers.V1;

[Route("api/v1/proposals")]
[ApiController]
[Authorize]
public class ProposalsController(IProposalService proposalService) : BaseApiController
{

    [HttpGet]
    public async Task<IActionResult> Browse([FromQuery] FilterProposalDto filter, CancellationToken ct)
        => HandleResult(await proposalService.BrowseAsync(filter, ct));



    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => HandleResult(await proposalService.GetByIdAsync(id, ct));



    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProposalDto dto, CancellationToken ct)
        => HandleResult(await proposalService.CreateAsync(dto, ct));

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateProposalDto dto, CancellationToken ct)
        => HandleResult(await proposalService.UpdateAsync(dto, ct));


    [HttpPost("{id:guid}/withdraw")]
    public async Task<IActionResult> Withdraw(Guid id, CancellationToken ct)
        => HandleResult(await proposalService.WithdrawAsync(id, ct));


    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id, CancellationToken ct)
        => HandleResult(await proposalService.AcceptAsync(id, ct));


    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, CancellationToken ct)
        => HandleResult(await proposalService.RejectAsync(id, ct));


    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => HandleResult(await proposalService.DeleteAsync(id, ct));
}