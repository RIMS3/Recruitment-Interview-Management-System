using RecruitmentInterviewManagementSystem.Applications.Features.Cvs.DTO;
using FluentValidation;

namespace RecruitmentInterviewManagementSystem.Applications.Features.Cvs.Validators
{
    public class UpdateCvEditorRequestValidator : AbstractValidator<UpdateCvEditorRequest>
    {
        public UpdateCvEditorRequestValidator()
        {
            // 1. Các trường thông tin cơ bản
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Họ tên không được để trống.")
                .MaximumLength(100).WithMessage("Họ tên không được vượt quá 100 ký tự.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Định dạng email không hợp lệ.")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^(0[3|5|7|8|9])+([0-9]{8})$").WithMessage("Số điện thoại không hợp lệ (Phải là số ĐT Việt Nam 10 số).")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            // 👉 THÊM MỚI: Validate Số năm kinh nghiệm < 100
            RuleFor(x => x.ExperienceYears)
                .LessThan(100).WithMessage("Số năm kinh nghiệm phải nhỏ hơn 100.")
                .GreaterThanOrEqualTo(0).WithMessage("Số năm kinh nghiệm không được là số âm.")
                .When(x => x.ExperienceYears.HasValue);

            // 2. Validate mảng Kinh nghiệm làm việc
            RuleForEach(x => x.Experiences).ChildRules(experiences =>
            {
                experiences.RuleFor(e => e.CompanyName)
                    .NotEmpty().WithMessage("Tên công ty không được để trống.");

                experiences.RuleFor(e => e.StartDate)
                    .NotEmpty().WithMessage("Ngày bắt đầu làm việc không được để trống.");

                experiences.RuleFor(e => e.EndDate)
                    .GreaterThanOrEqualTo(e => e.StartDate)
                    .WithMessage("Ngày kết thúc phải sau ngày bắt đầu.")
                    .When(e => e.EndDate.HasValue && e.StartDate.HasValue);
            });
        }
    }
}
