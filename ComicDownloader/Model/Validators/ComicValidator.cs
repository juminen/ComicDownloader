using FluentValidation;
using System;

namespace ComicDownloader.Model.Validators
{
    class ComicValidator : AbstractValidator<Comic>
    {
        public ComicValidator()
        {
            RuleFor(c => c.Name)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty().WithMessage("{PropertyName} can not be empty.")
                .MinimumLength(NameLength).WithMessage(NameLengthErrorMessage);

            RuleFor(c => c.SavingLocation)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty().WithMessage("{PropertyName} can not be empty.")
                .Must(SavinLocationExists).WithMessage("{PropertyName} does not exist.");

            RuleFor(c => c.StartUrl)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty().WithMessage("{PropertyName} can not be empty.")
                .Must(UriIsValid).WithMessage("{PropertyName} is not valid.");
        }

        protected int NameLength = 3;
        protected string NameLengthErrorMessage { get { return $"Name must be longer than {NameLength.ToString()}."; } }

        protected bool SavinLocationExists(string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                return false;
            }
            return true;
        }

        protected bool UriIsValid(string uri)
        {
            bool result = Uri.TryCreate(uri, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return result;
        }
    }
}
