using AuctionApplication.Shared;
using Microsoft.EntityFrameworkCore;

namespace AuctionApplication.Database;

public class Context : DbContext
{
    #pragma warning disable CS8618 // Required by Entity Framework
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Bid> Bids { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<AuctionCategory> AuctionCategory { get; set; }
    
    
    public string DbPath { get; }
    public Context()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "auction.db");

    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auction>().HasMany(a => a.ProductImages).WithOne().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Auction>().HasOne(a => a.Owner).WithMany().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Auction>().HasOne(a => a.Category).WithMany().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Auction>().HasOne(a => a.Winner).WithMany().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Bid>().HasOne(a => a.Auction).WithMany().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Bid>().HasOne(a => a.Bidder).WithMany().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Payment>().HasOne(a => a.Auction).WithOne().HasForeignKey<Auction>(a => a.PaymentId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Payment>().HasOne(a => a.User).WithMany().OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<AuctionCategory>().HasData(
            new AuctionCategory { Id = 1, Name = "Other" },
            new AuctionCategory { Id = 2, Name = "Electronics" },
            new AuctionCategory { Id = 3, Name = "Fashion" },
            new AuctionCategory { Id = 4, Name = "Home" },
            new AuctionCategory { Id = 5, Name = "Sports" },
            new AuctionCategory { Id = 6, Name = "Vehicles" }
        );
    }
}
