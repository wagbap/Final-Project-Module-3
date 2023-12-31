﻿// <auto-generated />
using System;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DataAccessLayer.Migrations
{
    [DbContext(typeof(ClinicaDbContext))]
    [Migration("20231008153201_step1")]
    partial class step1
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("DataAccessLayer.Model.AppointmentModel", b =>
                {
                    b.Property<int>("AppointId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AppointId"));

                    b.Property<DateTime>("AppointmentDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("DoctorMsg")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("DoctorUserId")
                        .HasColumnType("int");

                    b.Property<bool>("IsCompleted")
                        .HasColumnType("bit");

                    b.Property<string>("PDFFile")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PatientMsg")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PatientUserId")
                        .HasColumnType("int");

                    b.Property<float?>("Price")
                        .HasColumnType("real");

                    b.Property<DateTime>("UpdateTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("info")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("AppointId");

                    b.HasIndex("DoctorUserId");

                    b.HasIndex("PatientUserId");

                    b.ToTable("Appointments");
                });

            modelBuilder.Entity("DataAccessLayer.Model.Disease", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Diseases");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "Câncer"
                        },
                        new
                        {
                            Id = 2,
                            Name = "Tumor"
                        },
                        new
                        {
                            Id = 3,
                            Name = "AVC"
                        },
                        new
                        {
                            Id = 4,
                            Name = "Diabetes"
                        },
                        new
                        {
                            Id = 5,
                            Name = "Anemia"
                        },
                        new
                        {
                            Id = 6,
                            Name = "Hipertensão"
                        },
                        new
                        {
                            Id = 7,
                            Name = "Psicose"
                        });
                });

            modelBuilder.Entity("DataAccessLayer.Model.FileUser", b =>
                {
                    b.Property<int>("ImgId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ImgId"));

                    b.Property<string>("ImageUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("ImgId");

                    b.HasIndex("UserId");

                    b.ToTable("ImgUser");
                });

            modelBuilder.Entity("DataAccessLayer.Model.LogModel", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int?>("Id"));

                    b.Property<string>("Exception")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Level")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Obs")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Properties")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("Timestamp")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Logs");
                });

            modelBuilder.Entity("DataAccessLayer.Model.MessageModel", b =>
                {
                    b.Property<int>("MessageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MessageId"));

                    b.Property<int>("AppointId")
                        .HasColumnType("int");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("TimeSend")
                        .HasColumnType("datetime2");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("MessageId");

                    b.HasIndex("UserId");

                    b.ToTable("Message");
                });

            modelBuilder.Entity("DataAccessLayer.Model.RegionDiseaseStatistic", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("DeathStatus")
                        .HasColumnType("bit");

                    b.Property<int>("DiseaseId")
                        .HasColumnType("int");

                    b.Property<string>("Region")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DiseaseId");

                    b.ToTable("RegionDiseaseStatistics");
                });

            modelBuilder.Entity("DataAccessLayer.Model.UserModel", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserId"));

                    b.Property<DateTime?>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateATT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<int>("UserType")
                        .HasColumnType("int");

                    b.HasKey("UserId");

                    b.ToTable("Users");

                    b.HasDiscriminator<string>("Discriminator").HasValue("UserModel");

                    b.UseTphMappingStrategy();

                    b.HasData(
                        new
                        {
                            UserId = 1,
                            CreationDate = new DateTime(2023, 10, 8, 15, 32, 1, 244, DateTimeKind.Utc).AddTicks(8569),
                            DateATT = new DateTime(2023, 10, 8, 15, 32, 1, 244, DateTimeKind.Utc).AddTicks(8573),
                            Email = "superadmin@example.com",
                            FullName = "Super Admin",
                            IsDeleted = false,
                            Password = "$2b$10$z.Y8cglQp872m8IDXWOD5uVqp5V/0wOeZjebClfByNPQfy19g5qW.",
                            Status = 1,
                            UserType = 3
                        });
                });

            modelBuilder.Entity("DataAccessLayer.Model.DoctorModel", b =>
                {
                    b.HasBaseType("DataAccessLayer.Model.UserModel");

                    b.Property<string>("AdInfo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Especialization")
                        .HasColumnType("nvarchar(max)");

                    b.Property<float?>("Fees")
                        .HasColumnType("real");

                    b.Property<string>("Region")
                        .HasColumnType("nvarchar(max)");

                    b.ToTable("Users", t =>
                        {
                            t.Property("Address")
                                .HasColumnName("DoctorModel_Address");

                            t.Property("City")
                                .HasColumnName("DoctorModel_City");

                            t.Property("Region")
                                .HasColumnName("DoctorModel_Region");
                        });

                    b.HasDiscriminator().HasValue("DoctorModel");
                });

            modelBuilder.Entity("DataAccessLayer.Model.PatientModel", b =>
                {
                    b.HasBaseType("DataAccessLayer.Model.UserModel");

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Region")
                        .HasColumnType("nvarchar(max)");

                    b.HasDiscriminator().HasValue("PatientModel");
                });

            modelBuilder.Entity("DataAccessLayer.Model.AppointmentModel", b =>
                {
                    b.HasOne("DataAccessLayer.Model.DoctorModel", "Doctor")
                        .WithMany()
                        .HasForeignKey("DoctorUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DataAccessLayer.Model.PatientModel", "Patient")
                        .WithMany()
                        .HasForeignKey("PatientUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Doctor");

                    b.Navigation("Patient");
                });

            modelBuilder.Entity("DataAccessLayer.Model.FileUser", b =>
                {
                    b.HasOne("DataAccessLayer.Model.UserModel", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("DataAccessLayer.Model.MessageModel", b =>
                {
                    b.HasOne("DataAccessLayer.Model.UserModel", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("DataAccessLayer.Model.RegionDiseaseStatistic", b =>
                {
                    b.HasOne("DataAccessLayer.Model.Disease", "Disease")
                        .WithMany()
                        .HasForeignKey("DiseaseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Disease");
                });
#pragma warning restore 612, 618
        }
    }
}
