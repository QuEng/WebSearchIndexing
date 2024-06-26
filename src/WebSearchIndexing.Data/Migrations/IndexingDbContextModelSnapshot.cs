﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WebSearchIndexing.Data;

#nullable disable

namespace WebSearchIndexing.Data.Migrations
{
    [DbContext(typeof(IndexingDbContext))]
    partial class IndexingDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("WebSearchIndexing.Domain.Entities.ServiceAccount", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Json")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ProjectId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("QuotaLimitPerDay")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("ServiceAccounts");
                });

            modelBuilder.Entity("WebSearchIndexing.Domain.Entities.Setting", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("boolean");

                    b.Property<int>("RequestsPerDay")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("WebSearchIndexing.Domain.Entities.UrlRequest", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("AddedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Priority")
                        .HasColumnType("integer");

                    b.Property<DateTime>("ProcessedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("ServiceAccountId")
                        .HasColumnType("uuid");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ServiceAccountId");

                    b.ToTable("UrlRequests");
                });

            modelBuilder.Entity("WebSearchIndexing.Domain.Entities.UrlRequest", b =>
                {
                    b.HasOne("WebSearchIndexing.Domain.Entities.ServiceAccount", "ServiceAccount")
                        .WithMany()
                        .HasForeignKey("ServiceAccountId");

                    b.Navigation("ServiceAccount");
                });
#pragma warning restore 612, 618
        }
    }
}
