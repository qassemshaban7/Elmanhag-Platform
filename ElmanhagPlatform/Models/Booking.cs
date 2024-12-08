using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElmanhagPlatform.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime PaymentDate { get; set; }
        [NotMapped]
        public double Price { get; set; }

        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        [ValidateNever]
        public Course Course { get; set; }
        
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
    }
}
