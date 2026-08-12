using System.Linq.Expressions;
using FluentValidation;
using FreeGency.Infrastructure.Integrations.Cloudinary;
using Microsoft.AspNetCore.Http;

namespace FreeGency.Application.Common.Validators;

public enum UploadFileKind
{
    Any,
    Image,
    Document
}

public static class UploadFileValidator
{
    public static void ApplyRules<T>(
        AbstractValidator<T> validator,
        Expression<Func<T, IFormFile?>> selector,
        UploadFileKind kind = UploadFileKind.Any,
        int maxSizeInMb = 10)
    {
        validator.RuleFor(selector)
            .Must(file => file is null || IsAllowed(file, kind))
            .WithMessage(GetMessage(kind));

        validator.RuleFor(selector)
            .Must(file => file is null || file.Length <= maxSizeInMb * 1024 * 1024)
            .WithMessage($"File size must not exceed {maxSizeInMb} MB.");
    }

    public static void ApplyManyRules<T>(
        AbstractValidator<T> validator,
        Expression<Func<T, IFormFile[]?>> selector,
        UploadFileKind kind = UploadFileKind.Any,
        int maxSizeInMb = 10)
    {
        validator.RuleFor(selector)
            .Must(files => files is null || files.All(file => IsAllowed(file, kind)))
            .WithMessage(GetMessage(kind));

        validator.RuleFor(selector)
            .Must(files => files is null || files.Sum(file => file.Length) <= maxSizeInMb * 1024 * 1024)
            .WithMessage($"Total file size must not exceed {maxSizeInMb} MB.");
    }

    private static bool IsAllowed(IFormFile file, UploadFileKind kind) => kind switch
    {
        UploadFileKind.Image => StorageFileTypes.IsImage(file.FileName),
        UploadFileKind.Document => StorageFileTypes.IsDocument(file.FileName),
        _ => StorageFileTypes.IsAllowed(file.FileName)
    };

    private static string GetMessage(UploadFileKind kind) => kind switch
    {
        UploadFileKind.Image => "Only JPG, JPEG, PNG, and WEBP files are allowed.",
        UploadFileKind.Document => "Only PDF, DOC, DOCX, XLS, XLSX, PPT, PPTX, CSV, and TXT files are allowed.",
        _ => "Unsupported file type. Allowed: images (JPG, PNG, WEBP) and documents (PDF, Office, CSV, TXT).",
    };
}
