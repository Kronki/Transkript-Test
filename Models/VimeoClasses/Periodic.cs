namespace TranskriptTest.Models.VimeoClasses
{
    public class Periodic
    {
        public string Period { get; set; }
        public string Unit { get; set; }
        public long? Free { get; set; }
        public long? Max { get; set; }
        public long? Used { get; set; }
        public DateTime? ResetDate { get; set; }
    }
}
