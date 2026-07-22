using FluentValidation;
using FreeGency.Application.Features.Account.Dtos;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Account.Validator
{
    public class UpdateClientAccountValidation:AbstractValidator<UpdateClientAccountDto>
    {
        private readonly string[] _allowedExtensions =
      {
            ".jpg",".jpeg",".png",".webp"
      };
        public UpdateClientAccountValidation()
        {
            RuleFor(x => x.FirstName)
     .NotEmpty()
     .WithMessage("First name is required.")
     .MaximumLength(50)
     .WithMessage("First name must not exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required.")
                .MaximumLength(50)
                .WithMessage("Last name must not exceed 50 characters.");

            RuleFor(x => x.Country)
                .MaximumLength(100)
                .WithMessage("Country must not exceed 100 characters.");

            RuleFor(x => x.Bio)
                .MaximumLength(1000)
                .WithMessage("Bio must not exceed 1000 characters.");

            RuleFor(x => x.ProfileImage)
                .Must(BeAValidImage)
                .When(x => x.ProfileImage is not null)
                .WithMessage("Only JPG, JPEG, PNG, and WEBP files are allowed.");

            RuleFor(x => x.ProfileImage)
                .Must(file => file == null || file.Length <= 5 * 1024 * 1024)
                .WithMessage("Image size must not exceed 5 MB.");
        }
        private bool BeAValidImage(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();

            return _allowedExtensions.Contains(extension);
        }
    }
}
