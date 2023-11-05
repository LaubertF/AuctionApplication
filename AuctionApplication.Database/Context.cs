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
    
    
    public string DbPath { get; }
    public Context()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "auction.db");
        Console.WriteLine($"Data Source={DbPath}");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
    
}