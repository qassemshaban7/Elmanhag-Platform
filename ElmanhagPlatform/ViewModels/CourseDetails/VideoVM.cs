using ElmanhagPlatform.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElmanhagPlatform.ViewModels.CourseDetails
{
    public class VideoVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string VideoPath { get; set; }
        public int WatchCount { get; set; }
        public int TotalMinutesWatched { get; set; }
    }
}