namespace ElmanhagPlatform.ViewModels.Video
{
    public class AddVideoVM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile VideoPath { get; set; }
        public string LectureId { get; set; }
        public int Duration { get; set; }

    }
}
