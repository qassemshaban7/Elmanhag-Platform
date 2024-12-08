using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElmanhagPlatform.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public int Status { get; set; }

        [Required(ErrorMessage = "رقم الهاتف التي قمت بالتحويل منه مطلوب")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "رقم الهاتف يجب أن يكون 11 رقما")]
        public string PhoneNumber { get; set; }
        public string Image { get; set; }

        [ForeignKey("StudentId")]
        public string StudentId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
