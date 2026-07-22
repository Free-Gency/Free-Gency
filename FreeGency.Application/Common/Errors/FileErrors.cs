using FreeGency.Application.Common.Models;

namespace FreeGency.Application.Common.Errors;

public static class FileErrors
{
    public static readonly Error UploadFailed =
        new("File.UploadFailed", "Failed to upload image. Please try again.", StatusCodes.Status500InternalServerError);
}
