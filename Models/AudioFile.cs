using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TranskriptTest.Models
{
    public class AudioFile
    {
        [Key]
        public int Id { get; set; }
        public List<Subtitle>? Subtitle { get; set; }
        public string? FilePath { get; set; }
        public double FileSize { get; set; }
    }
}
