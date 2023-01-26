﻿// <auto-generated />
using System;
using AIQXCoreService.Implementation.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AIQXCoreService.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20220404154546_UseCaseCurrentStep")]
    partial class UseCaseCurrentStep
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("AIQXCoreService.Domain.Models.AttachmentEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2");

                    b.Property<string>("Metadata")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RefId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("UseCaseId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UseCaseId");

                    b.ToTable("core__attachments");
                });

            modelBuilder.Entity("AIQXCoreService.Domain.Models.PlantEntity", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("Position")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("core__plants");
                });

            modelBuilder.Entity("AIQXCoreService.Domain.Models.UseCaseEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Building")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("CurrentStep")
                        .HasColumnType("int");

                    b.Property<string>("Image")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Line")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PlantId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Position")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("PlantId");

                    b.ToTable("core__use_cases");
                });

            modelBuilder.Entity("AIQXCoreService.Domain.Models.UseCaseStepEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("CompletedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Duration")
                        .HasColumnType("int");

                    b.Property<string>("Form")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<Guid>("UseCaseId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UseCaseId");

                    b.ToTable("core__use_case_steps");
                });

            modelBuilder.Entity("AIQXCoreService.Domain.Models.AttachmentEntity", b =>
                {
                    b.HasOne("AIQXCoreService.Domain.Models.UseCaseEntity", "UseCase")
                        .WithMany("Attachments")
                        .HasForeignKey("UseCaseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UseCase");
                });

            modelBuilder.Entity("AIQXCoreService.Domain.Models.UseCaseEntity", b =>
                {
                    b.HasOne("AIQXCoreService.Domain.Models.PlantEntity", "Plant")
                        .WithMany("UseCases")
                        .HasForeignKey("PlantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Plant");
                });

            modelBuilder.Entity("AIQXCoreService.Domain.Models.UseCaseStepEntity", b =>
                {
                    b.HasOne("AIQXCoreService.Domain.Models.UseCaseEntity", "UseCase")
                        .WithMany("Steps")
                        .HasForeignKey("UseCaseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UseCase");
                });

            modelBuilder.Entity("AIQXCoreService.Domain.Models.PlantEntity", b =>
                {
                    b.Navigation("UseCases");
                });

            modelBuilder.Entity("AIQXCoreService.Domain.Models.UseCaseEntity", b =>
                {
                    b.Navigation("Attachments");

                    b.Navigation("Steps");
                });
#pragma warning restore 612, 618
        }
    }
}
