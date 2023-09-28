using AuctionApplication.Database;
using AuctionApplication.Shared;

namespace AuctionApplication.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void UnitOfWork()
    {
        var auction = new Auction();
        using var context = new Context();
        context.Set<Auction>().Add(auction);
        context.SaveChanges();
    }
}