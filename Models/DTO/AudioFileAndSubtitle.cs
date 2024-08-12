namespace TranskriptTest.Models.DTO
{
    public class AudioFileAndSubtitle
    {
        public int Id { get; set; }
        public string FilePath { get; set; }
        public List<SubtitleDTO> Subtitles { get; set; } = new List<SubtitleDTO>();
    }
}
