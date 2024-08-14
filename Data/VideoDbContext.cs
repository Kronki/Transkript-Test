using Microsoft.EntityFrameworkCore;
using TranskriptTest.Models;

namespace TranskriptTest.Data
{
    public class VideoDbContext : DbContext
    {
        public VideoDbContext(DbContextOptions<VideoDbContext> options) : base(options)
        {

        }

        public DbSet<Video> Videos { get; set; }
        public DbSet<Subtitle> Subtitles { get; set; }
        public DbSet<SubtitleRequest> SubtitleRequests { get; set; }
        public DbSet<AudioFile> AudioFiles { get; set; }
        public DbSet<Transcript> Transcripts { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
