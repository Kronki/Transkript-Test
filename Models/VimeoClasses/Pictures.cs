namespace TranskriptTest.Models.VimeoClasses
{
    public class Pictures
    {
        public string Uri { get; set; }
        public bool Active { get; set; }
        public string Type { get; set; }
        public string BaseLink { get; set; }
        public List<PictureSize> Sizes { get; set; }
        public string ResourceKey { get; set; }
        public bool DefaultPicture { get; set; }
    }
}
