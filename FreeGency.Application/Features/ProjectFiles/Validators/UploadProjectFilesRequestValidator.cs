using FreeGency.Application.Common.Validators;
using FreeGency.Application.Features.ProjectFiles.DTOs;

namespace FreeGency.Application.Features.ProjectFiles.Validators;

public sealed class UploadProjectFilesRequestValidator : AbstractValidator<UploadProjectFilesRequestDto>
{
    public UploadProjectFilesRequestValidator()
    {
        RuleFor(x => x.Files)
            .NotEmpty()
            .WithMessage("Please select at least one file.");

        RuleFor(x => x.Files)
            .Must(files => files.Length <= 20)
            .WithMessage("You can upload at most 20 files at once.");

        RuleFor(x => x.FileKind)
            .IsInEnum();

        UploadFileValidator.ApplyManyRules(this, x => x.Files, UploadFileKind.Any, maxSizeInMb: 50);
    }
}
