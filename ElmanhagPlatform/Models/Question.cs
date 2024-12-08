using System.ComponentModel.DataAnnotations.Schema;

namespace ElmanhagPlatform.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string FirstAnswer { get; set; }
        public string SecondAnswer { get; set; }
        public string ThirdAnswer { get; set; }
        public string ForthAnswer { get; set; }
        public string CorrectAnswer { get; set; }

        [ForeignKey("ExamId")]
        public int ExamId { get; set; }
        public Exam? Exam { get; set; }
    }
}
