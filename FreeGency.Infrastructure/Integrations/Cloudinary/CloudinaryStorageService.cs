using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FreeGency.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using CloudinaryClient = CloudinaryDotNet.Cloudinary;

namespace FreeGency.Infrastructure.Integrations.Cloudinary;

public class CloudinaryStorageService(CloudinaryClient cloudinary) : IStorageService
{
    public async Task<UploadedAsset> UploadAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
    {
        if (!StorageFileTypes.IsAllowed(file.FileName))
            throw new InvalidOperationException($"Unsupported file type: {Path.GetExtension(file.FileName)}");

        var url = StorageFileTypes.IsImage(file.FileName)
            ? await UploadImageAsync(file, folder, cancellationToken)
            : await UploadRawAsync(file, folder, cancellationToken);

        return new UploadedAsset(url, file.FileName);
    }

    public async Task<IReadOnlyList<UploadedAsset>> UploadManyAsync(
        IEnumerable<IFormFile> files,
        string folder,
        CancellationToken cancellationToken = default)
    {
        var uploaded = new List<UploadedAsset>();

        foreach (var file in files)
            uploaded.Add(await UploadAsync(file, folder, cancellationToken));

        return uploaded;
    }

    private async Task<string> UploadImageAsync(IFormFile file, string folder, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();

        var result = await cloudinary.UploadAsync(new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder,
            PublicId = BuildPublicId(file),
            Overwrite = false
        }, cancellationToken);

        return GetUrl(result);
    }

    private async Task<string> UploadRawAsync(IFormFile file, string folder, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();

        var result = await cloudinary.UploadAsync(new RawUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder,
            PublicId = BuildPublicId(file),
            Overwrite = false
        }, "raw", cancellationToken);

        return GetUrl(result);
    }

    private static string BuildPublicId(IFormFile file)
    {
        var fieldName = string.IsNullOrWhiteSpace(file.Name) ? "file" : file.Name;
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var originalName = Path.GetFileNameWithoutExtension(file.FileName);

        return $"{fieldName}-{timestamp}-{originalName}";
    }

    private static string GetUrl(UploadResult result)
    {
        if (result.Error is not null)
            throw new InvalidOperationException(result.Error.Message);

        return result.SecureUrl.ToString();
    }
}
