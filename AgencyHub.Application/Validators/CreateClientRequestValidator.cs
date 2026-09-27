using AgencyHub.Application.DTOs.Clients;

using FluentValidation;

namespace AgencyHub.Application.Validators;

public class CreateClientRequestValidator
    : AbstractValidator<CreateClientRequest>
{
    public CreateClientRequestValidator()
    {
        RuleFor(x =>
            x.CompanyName)

            .NotEmpty()
            .WithMessage(
                "Company name is required.")

            .MaximumLength(200);

        RuleFor(x =>
            x.ContactPerson)

            .NotEmpty()
            .WithMessage(
                "Contact person is required.")

            .MaximumLength(150);

        RuleFor(x =>
            x.Email)

            .NotEmpty()
            .WithMessage(
                "Email is required.")

            .EmailAddress()
            .WithMessage(
                "Enter a valid email address.")

            .MaximumLength(200);

        RuleFor(x =>
            x.Phone)

            .MaximumLength(30);

        RuleFor(x =>
            x.Address)

            .MaximumLength(500);

        RuleFor(x =>
            x.Industry)

            .MaximumLength(100);

        RuleFor(x =>
            x.Status)

            .NotEmpty()

            .Must(status =>
                status == "Active" ||
                status == "Inactive")

            .WithMessage(
                "Status must be Active or Inactive.");

        RuleFor(x =>
            x.Notes)

            .MaximumLength(2000);
    }
}