namespace ElmanhagPlatform.ViewModels.Video
{
    public class EditVideoVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile? VideoPath { get; set; }
        public int LectureId { get; set; }
        public int? Duration { get; set; }

    }
}
