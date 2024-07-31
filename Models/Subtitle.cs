using System.ComponentModel.DataAnnotations.Schema;

namespace TranskriptTest.Models
{
    public class Subtitle
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string Content { get; set; }
        public Video? Video { get; set; }
        [ForeignKey("VideoId")]
        public int VideoId { get; set; }
        public List<SubtitleRequest>? SubtitleRequests { get; set; }
    }
}
