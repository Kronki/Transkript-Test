using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TranskriptTest.Models
{
    public class Transcript
    {
        [Key]
        public int Id { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public string Text { get; set; }
        public int? VideoId { get; set; }
    }
}
