using System.ComponentModel.DataAnnotations.Schema;

namespace ElmanhagPlatform.Models
{
    public class Result
    {
        public int Id { get; set; }
        public string Answer { get; set; }

        [ForeignKey("QuestionId")]
        public int QuestionId { get; set; }
        public Question Question { get; set; }

        [ForeignKey("ExamAnswerId")]
        public int ExamAnswerId { get; set; }
        public ExamAnswer ExamAnswer { get; set; }
    }
}
