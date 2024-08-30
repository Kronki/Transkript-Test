namespace TranskriptTest.Models.DTO
{
    public class EditTextTrackRequest
    {
        public string VideoId { get; set; }
        //public string TextTrackId { get; set; }
        public string NewContent { get; set; }
        public string AccessToken { get; set; }
        public string Language { get; set; }
    }
}
