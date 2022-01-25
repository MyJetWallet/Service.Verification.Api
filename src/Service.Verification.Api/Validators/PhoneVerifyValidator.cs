using System.Linq;
using FluentValidation;
using Service.Verification.Api.Controllers.Contracts;

namespace Service.Verification.Api.Validators
{
    public class PhoneRequestValidator  : AbstractValidator<SendPhoneSetupRequest>
    {
        public PhoneRequestValidator()
        {
            RuleFor(x => x.PhoneCode).NotNull();

            RuleFor(x => x.PhoneCode).Matches("^[+]{0,1}[0-9]*$");
            RuleFor(x => x.PhoneCode).Length(1, 5);
            RuleFor(x => x.PhoneCode).Must(t => t!= null && t.StartsWith('+')).WithMessage("Phone number should starts with +");
            RuleFor(x => x.PhoneCode).Must(t => t!= null &&!t.Any(char.IsWhiteSpace));
            
            RuleFor(x => x.PhoneBody).NotNull();

            RuleFor(x => x.PhoneBody).Matches("^[0-9]*$");
            RuleFor(x => x.PhoneBody).Length(4, 20);
            RuleFor(x => x.PhoneBody).Must(t => t!= null && !t.Any(char.IsWhiteSpace));
        }
    }
    
    public class PhoneVerifyValidator  : AbstractValidator<VerifyPhoneSetupRequest>
    {
        public PhoneVerifyValidator()
        {
            RuleFor(x => x.PhoneCode).NotNull();

            RuleFor(x => x.PhoneCode).Matches("^[+]{0,1}[0-9]*$");
            RuleFor(x => x.PhoneCode).Length(1, 5);
            RuleFor(x => x.PhoneCode).Must(t => t!= null && t.StartsWith('+')).WithMessage("Phone number should starts with +");
            RuleFor(x => x.PhoneCode).Must(t => t!= null && !t.Any(char.IsWhiteSpace));
            
            RuleFor(x => x.PhoneBody).NotNull();

            RuleFor(x => x.PhoneBody).Matches("^[0-9]*$");
            RuleFor(x => x.PhoneBody).Length(4, 20); 
            RuleFor(x => x.PhoneBody).Must(t => t!= null && !t.Any(char.IsWhiteSpace));
        }
    }
}