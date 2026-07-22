using FoundIt.Application.Common.Models;
using Microsoft.AspNetCore.Http;

namespace FreeGency.Application.Common.Errors;

public static class SpecialtyErrors
{
    public static readonly Error NotFound =
        new(
            "Specialty.NotFound",
            "Specialty not found.",
            StatusCodes.Status404NotFound
        );

    public static readonly Error AlreadyExists =
        new(
            "Specialty.AlreadyExists",
            "Specialty already exists.",
            StatusCodes.Status409Conflict
        );

    public static readonly Error InvalidCategory =
        new(
            "Specialty.InvalidCategory",
            "Category not found.",
            StatusCodes.Status404NotFound
        );
}