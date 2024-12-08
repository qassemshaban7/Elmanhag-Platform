using System.ComponentModel.DataAnnotations.Schema;

namespace ElmanhagPlatform.Models
{
    public class VideoWatchLog
    {
        public int Id { get; set; }
        public int WatchCount { get; set; }
        public int TotalMinutesWatched { get; set; }
        public DateTime LastWatchDate { get; set; }

        [ForeignKey("VideoId")]
        public int VideoId { get; set; }
        public Video Video { get; set; }

        [ForeignKey("UserId")]
        public string UserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
