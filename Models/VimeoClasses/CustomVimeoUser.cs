namespace TranskriptTest.Models.VimeoClasses
{
    public class CustomVimeoUser
    {
        public string Uri { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public Capabilities Capabilities { get; set; }
        public string Location { get; set; }
        public string Gender { get; set; }
        public string Bio { get; set; }
        public string ShortBio { get; set; }
        public DateTime CreatedTime { get; set; }
        public Pictures Pictures { get; set; }
        public List<Website> Websites { get; set; } // Assuming websites might be a list of objects
        public Metadata Metadata { get; set; }
        public LocationDetails LocationDetails { get; set; }
        public List<object> Skills { get; set; } // Assuming skills might be a list of objects
        public bool AvailableForHire { get; set; }
        public bool CanWorkRemotely { get; set; }
        public Preferences Preferences { get; set; }
        public List<string> ContentFilter { get; set; }
        public UploadQuota UploadQuota { get; set; }
        public string ResourceKey { get; set; }
        public string Account { get; set; }
    }
}
