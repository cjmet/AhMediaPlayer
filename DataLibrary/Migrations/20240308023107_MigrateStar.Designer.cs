﻿// <auto-generated />
using System;
using DataLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DataLibrary.Migrations
{
    [DbContext(typeof(PlaylistContext))]
    [Migration("20240308023107_MigrateStar")]
    partial class MigrateStar
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.2");

            modelBuilder.Entity("DataLibrary.Playlist", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Playlists");
                });

            modelBuilder.Entity("DataLibrary.Song", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Album")
                        .HasColumnType("TEXT");

                    b.Property<string>("AlphaTitle")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Artist")
                        .HasColumnType("TEXT");

                    b.Property<string>("Band")
                        .HasColumnType("TEXT");

                    b.Property<long>("FileSize")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Genre")
                        .HasColumnType("TEXT");

                    b.Property<string>("Length")
                        .HasColumnType("TEXT");

                    b.Property<string>("PathName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("Star")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int?>("Track")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("Year")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Songs");
                });

            modelBuilder.Entity("PlaylistSong", b =>
                {
                    b.Property<int>("PlaylistsId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SongsId")
                        .HasColumnType("INTEGER");

                    b.HasKey("PlaylistsId", "SongsId");

                    b.HasIndex("SongsId");

                    b.ToTable("PlaylistSong");
                });

            modelBuilder.Entity("PlaylistSong", b =>
                {
                    b.HasOne("DataLibrary.Playlist", null)
                        .WithMany()
                        .HasForeignKey("PlaylistsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DataLibrary.Song", null)
                        .WithMany()
                        .HasForeignKey("SongsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}