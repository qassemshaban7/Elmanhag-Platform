using System.ComponentModel.DataAnnotations.Schema;

namespace ElmanhagPlatform.Models
{
    public class ExamAnswer
    {
        public int Id { get; set; }

        [ForeignKey("ExamId")]
        public int ExamId { get; set; }
        public Exam Exam { get; set; }

        [ForeignKey("UserId")]
        public string UserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public ICollection<Result> Results { get; set; }
    }
}
