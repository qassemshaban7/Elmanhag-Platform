using System.ComponentModel.DataAnnotations;

namespace ElmanhagPlatform.ViewModels.TeacherVM
{
    public class CreateTeacherVM
    {
        [Required(ErrorMessage = "اسم المدرس")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "البريد الالكتروني مطلوب")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "رقم هاتف المدرس")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "رقم الهاتف يجب أن يكون 11 رقما")]
        public string PhoneNumber { get; set; }

        [MinLength(6, ErrorMessage = "الرقم السري يجب أن يكون 6 أرقام على الأقل")]
        [Required(ErrorMessage = "الرقم السري مطلوب")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "الرقم السري وتأكيد الرقم السري غير متطابقين")]
        [MinLength(6, ErrorMessage = "تأكيد الرقم السري يجب أن يكون 6 أرقام على الأقل")]
        [Required(ErrorMessage = "تأكيد الرقم السري مطلوب")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "صورة شخصية للمدرس")]
        public IFormFile Image { get; set; }

        [Required(ErrorMessage = "اسم المادة مطلوب")]
        public string MaterialName { get; set; }

        [Required(ErrorMessage = "وصف المدرس مطلوب")]
        public string descreption { get; set; }

        public bool Preparatory { get; set; }
        public bool Secondry { get; set; }
    }
}
