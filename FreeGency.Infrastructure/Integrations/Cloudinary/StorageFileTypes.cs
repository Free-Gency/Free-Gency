namespace FreeGency.Infrastructure.Integrations.Cloudinary;

public static class StorageFileTypes
{
    public static readonly HashSet<string> Images =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

    public static readonly HashSet<string> Documents =
        new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" };

    public static bool IsImage(string fileName)
        => Images.Contains(Path.GetExtension(fileName));

    public static bool IsDocument(string fileName)
        => Documents.Contains(Path.GetExtension(fileName));

    public static bool IsAllowed(string fileName)
        => IsImage(fileName) || IsDocument(fileName);
}
