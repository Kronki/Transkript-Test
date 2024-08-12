﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TranskriptTest.Data;

#nullable disable

namespace TranskriptTest.Migrations
{
    [DbContext(typeof(VideoDbContext))]
    partial class VideoDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("TranskriptTest.Models.AudioFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("FilePath")
                        .HasColumnType("longtext");

                    b.Property<double>("FileSize")
                        .HasColumnType("double");

                    b.HasKey("Id");

                    b.ToTable("AudioFiles");
                });

            modelBuilder.Entity("TranskriptTest.Models.Subtitle", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("AudioFileId")
                        .HasColumnType("int");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Language")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Path")
                        .HasColumnType("longtext");

                    b.Property<int?>("VideoId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AudioFileId");

                    b.HasIndex("VideoId");

                    b.ToTable("Subtitles");
                });

            modelBuilder.Entity("TranskriptTest.Models.SubtitleRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("IsActive")
                        .HasColumnType("tinyint(1)");

                    b.Property<int?>("SubtitleId")
                        .HasColumnType("int");

                    b.Property<string>("TextTrackContent")
                        .HasColumnType("longtext");

                    b.Property<int?>("TextTrackId")
                        .HasColumnType("int");

                    b.Property<string>("TextTrackUri")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("SubtitleId");

                    b.ToTable("SubtitleRequests");
                });

            modelBuilder.Entity("TranskriptTest.Models.Video", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Videos");
                });

            modelBuilder.Entity("TranskriptTest.Models.Subtitle", b =>
                {
                    b.HasOne("TranskriptTest.Models.AudioFile", "AudioFile")
                        .WithMany("Subtitle")
                        .HasForeignKey("AudioFileId");

                    b.HasOne("TranskriptTest.Models.Video", "Video")
                        .WithMany("Subtitles")
                        .HasForeignKey("VideoId");

                    b.Navigation("AudioFile");

                    b.Navigation("Video");
                });

            modelBuilder.Entity("TranskriptTest.Models.SubtitleRequest", b =>
                {
                    b.HasOne("TranskriptTest.Models.Subtitle", "Subtitle")
                        .WithMany("SubtitleRequests")
                        .HasForeignKey("SubtitleId");

                    b.Navigation("Subtitle");
                });

            modelBuilder.Entity("TranskriptTest.Models.AudioFile", b =>
                {
                    b.Navigation("Subtitle");
                });

            modelBuilder.Entity("TranskriptTest.Models.Subtitle", b =>
                {
                    b.Navigation("SubtitleRequests");
                });

            modelBuilder.Entity("TranskriptTest.Models.Video", b =>
                {
                    b.Navigation("Subtitles");
                });
#pragma warning restore 612, 618
        }
    }
}
