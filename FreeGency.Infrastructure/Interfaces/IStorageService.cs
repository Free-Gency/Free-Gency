using Microsoft.AspNetCore.Http;

namespace FreeGency.Infrastructure.Interfaces;

public record UploadedAsset(string Url, string FileName);

public interface IStorageService
{
    Task<UploadedAsset> UploadAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UploadedAsset>> UploadManyAsync(IEnumerable<IFormFile> files, string folder, CancellationToken cancellationToken = default);
}
