﻿// <auto-generated />
using AICBank.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AICBank.Data.Migrations
{
    [DbContext(typeof(AICBankDbContext))]
    [Migration("20240802192250_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("AICBank.Core.Entities.Address", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("BankAccountId")
                        .HasColumnType("int");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Complement")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Neighborhood")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Number")
                        .IsRequired()
                        .HasColumnType("varchar(20)");

                    b.Property<string>("State")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Street")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ZipCode")
                        .IsRequired()
                        .HasColumnType("varchar(20)");

                    b.HasKey("Id");

                    b.ToTable("Addresses", (string)null);
                });

            modelBuilder.Entity("AICBank.Core.Entities.BankAccount", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AddressId")
                        .HasColumnType("int");

                    b.Property<string>("Cnae")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Document")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("EmailContact")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("GalaxHash")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("GalaxId")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("GalaxPayId")
                        .HasColumnType("int");

                    b.Property<string>("Logo")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("NameDisplay")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("varchar(20)");

                    b.Property<int>("ProfessionalId")
                        .HasColumnType("int");

                    b.Property<string>("ResponsibleDocument")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("SoftDescriptor")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<byte>("Type")
                        .HasColumnType("tinyint unsigned");

                    b.Property<string>("TypeCompany")
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("AddressId")
                        .IsUnique();

                    b.ToTable("BankAccounts", (string)null);
                });

            modelBuilder.Entity("AICBank.Core.Entities.Professional", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("BankAccountId")
                        .HasColumnType("int");

                    b.Property<string>("Inscription")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("InternalName")
                        .IsRequired()
                        .HasColumnType("varchar(150)");

                    b.HasKey("Id");

                    b.HasIndex("BankAccountId")
                        .IsUnique();

                    b.ToTable("Professionals", (string)null);
                });

            modelBuilder.Entity("AICBank.Core.Entities.BankAccount", b =>
                {
                    b.HasOne("AICBank.Core.Entities.Address", "Address")
                        .WithOne("BankAccount")
                        .HasForeignKey("AICBank.Core.Entities.BankAccount", "AddressId");

                    b.Navigation("Address");
                });

            modelBuilder.Entity("AICBank.Core.Entities.Professional", b =>
                {
                    b.HasOne("AICBank.Core.Entities.BankAccount", "BankAccount")
                        .WithOne("Professional")
                        .HasForeignKey("AICBank.Core.Entities.Professional", "BankAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BankAccount");
                });

            modelBuilder.Entity("AICBank.Core.Entities.Address", b =>
                {
                    b.Navigation("BankAccount")
                        .IsRequired();
                });

            modelBuilder.Entity("AICBank.Core.Entities.BankAccount", b =>
                {
                    b.Navigation("Professional");
                });
#pragma warning restore 612, 618
        }
    }
}
