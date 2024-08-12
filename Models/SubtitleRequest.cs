using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TranskriptTest.Models
{
    public class SubtitleRequest
    {
        [Key]
        public int Id { get; set; }
        public int? TextTrackId { get; set; }
        public string? TextTrackUri { get; set; }
        public string? TextTrackContent { get; set; }
        public Subtitle? Subtitle { get; set; }
        [ForeignKey("SubtitleId")]
        public int? SubtitleId { get; set; }
        public bool IsActive { get; set; }
    }
}
