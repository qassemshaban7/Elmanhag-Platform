namespace ElmanhagPlatform.ViewModels.ExamVM
{
    public class AddExamVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public int LectureId { get; set; }

        public List<QuestionVM> Questions { get; set; } = new List<QuestionVM>();
    }
}
