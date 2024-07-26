using System.ComponentModel.DataAnnotations;

namespace TranskriptTest.Models
{
    public class Video
    {
        [Key]
        public int Id { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public List<Subtitle>? Subtitles { get; set; }
    }
}
