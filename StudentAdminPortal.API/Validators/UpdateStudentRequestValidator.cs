using FluentValidation;
using StudentAdminPortal.API.DomainModels;
using StudentAdminPortal.API.Repositories;
using System;
using System.Linq;

namespace StudentAdminPortal.API.Validators
{
    public class UpdateStudentRequestValidator: AbstractValidator<UpdateStudentRequest>
    {
        public UpdateStudentRequestValidator(IStudentRepository studentRepository)
        {
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();

            RuleFor(x => x.DateOfBirth).NotEmpty();
            //RuleFor(x => x.DateOfBirth)
            //    .NotEmpty()
            //    .Must(dob =>
            //    {
            //        DateTime parsedDob;
            //        // Try to parse the string to DateTime
            //        if (DateTime.TryParse(dob, out parsedDob))
            //        {
            //            return parsedDob < DateTime.Now;
            //        }
            //        // If parsing fails, validation fails
            //        return false;
            //    })
            //    .WithMessage("Date of Birth must be a valid date and cannot be in the future");
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Mobile).GreaterThan(99999).LessThan(10000000000);
            RuleFor(x => x.GenderId).NotEmpty().Must(id =>
            {
                var gender = studentRepository.GetGendersAsync().Result.ToList()
                .FirstOrDefault(x => x.Id == id);

                if (gender != null)
                {
                    return true;
                }

                return false;
            }).WithMessage("Please select a valid Gender");
            //RuleFor(x => x.PhysicalAddress).NotEmpty();
            //RuleFor(x => x.PostalAddress).NotEmpty();
        }
    }
}
