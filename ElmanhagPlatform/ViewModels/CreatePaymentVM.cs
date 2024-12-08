using System.ComponentModel.DataAnnotations;

namespace ElmanhagPlatform.ViewModels
{
    public class CreatePaymentVM
    {
        [Required(ErrorMessage = "رقم الهاتف التي قمت بالتحويل منه مطلوب")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "رقم الهاتف يجب أن يكون 11 رقما")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "المبلغ المدفوع مطلوب")]
        public int Value { get; set; }

        [Required(ErrorMessage = "صورة الوصل مطلوبة")]
        public IFormFile Image { get; set; }
    }
}
