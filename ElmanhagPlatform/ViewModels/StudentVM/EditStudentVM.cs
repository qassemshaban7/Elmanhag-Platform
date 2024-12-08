using System.ComponentModel.DataAnnotations;

namespace ElmanhagPlatform.ViewModels.StudentVM
{
    public class EditStudentVM
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "الاسم رباعي مطلوب")]
        [MinLength(10, ErrorMessage = "الاسم يجب أن يكون 10 أحرف على الأقل")]
        [MaxLength(40, ErrorMessage = "الاسم يجب ألا يتجاوز 40 حرفًا")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "رقم الهاتف يجب أن يكون 11 رقما")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "البريد الالكتروني مطلوب")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "رقم هاتف ولي الأمر مطلوب")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "رقم الهاتف يجب أن يكون 11 رقما")]
        public string PhoneNumber { get; set; }

        [MinLength(6, ErrorMessage = "الرقم السري يجب أن يكون 6 أرقام على الأقل")]
        [RegularExpression(@"^\d+$", ErrorMessage = "الرقم السري يجب أن يحتوي على أرقام فقط")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Compare("Password", ErrorMessage = "الرقم السري وتأكيد الرقم السري غير متطابقين")]
        [MinLength(6, ErrorMessage = "تأكيد الرقم السري يجب أن يكون 6 أرقام على الأقل")]
        [RegularExpression(@"^\d+$", ErrorMessage = "تأكيد الرقم السري يجب أن يحتوي على أرقام فقط")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "صورة البطاقة او صورة شهادة الميلاد مطلوبة")]
        public IFormFile Image { get; set; }

        [Required(ErrorMessage = "الرقم القومي مطلوب")]
        [RegularExpression(@"^\d{14}$", ErrorMessage = "الرقم القومي يجب أن يتكون من 14 رقما")]
        public long? CardNumber { get; set; }
    }
}
