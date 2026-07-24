namespace FreeGency.Application.Features.Portfolio.Commands
{
    // Queries
    public partial class PortfolioService
    {
        public async Task<ApiResponse<IEnumerable<PortfolioProjectDto>>> GetMineAsync(
            CancellationToken ct = default)
        {
            var repo = _unitOfWork.Repository<IPortfolioRepository, PortfolioProject>();

            var portfolios = await repo.Query()
                .Where(x => x.OwnerUserId == _currentUser.UserId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);

            var dto = _mapper.Map<IEnumerable<PortfolioProjectDto>>(portfolios);

            return ApiResponse.Success(dto);
        }

        public async Task<ApiResponse<IEnumerable<PortfolioProjectDto>>> GetDeveloperPortfolioAsync(
            Guid developerId,
            CancellationToken ct = default)
        {
            var repo = _unitOfWork.Repository<IPortfolioRepository, PortfolioProject>();

            var portfolios = await repo.GetDeveloperPortfolioAsync(developerId, ct);

            var dto = _mapper.Map<IEnumerable<PortfolioProjectDto>>(portfolios);

            return ApiResponse.Success(dto);
        }

        public async Task<ApiResponse<PortfolioProjectDetailsDto>> GetDetailsAsync(
            Guid id,
            CancellationToken ct = default)
        {
            var repo = _unitOfWork.Repository<IPortfolioRepository, PortfolioProject>();

            var portfolio = await repo.GetDetailsAsync(id, ct);

            if (portfolio is null)
                return ApiResponse.Failure<PortfolioProjectDetailsDto>(
                    AppError.NotFound(nameof(PortfolioProject), id));

            return ApiResponse.Success(
                _mapper.Map<PortfolioProjectDetailsDto>(portfolio));
        }

        public async Task<ApiResponse<IEnumerable<PortfolioProjectDto>>> GetTeamPortfolioAsync(
           Guid teamId,
           CancellationToken ct = default)
        {
            var portfolio = await _portfolioRepo
                .GetTeamPortfolioAsync(teamId, ct);

            return ApiResponse.Success(
                _mapper.Map<IEnumerable<PortfolioProjectDto>>(portfolio));
        }

    }
}
