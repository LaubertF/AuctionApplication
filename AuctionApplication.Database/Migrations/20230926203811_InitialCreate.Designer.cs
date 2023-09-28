﻿// <auto-generated />
using System;
using AuctionApplication.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AuctionApplication.Database.Migrations
{
    [DbContext(typeof(Context))]
    [Migration("20230926203811_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.11");

            modelBuilder.Entity("AuctionApplication.Shared.Auction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("EndInclusive")
                        .HasColumnType("TEXT");

                    b.Property<string>("NameOfProduct")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PhotoUrl")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<decimal>("StartingPrice")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Auctions");
                });
#pragma warning restore 612, 618
        }
    }
}