using AuctionApplication.Shared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace AuctionApplication.Server.Hubs;

public class BidHub : Hub
{
    private readonly DbContext _context;

    public BidHub(DbContext context)
    {
        _context = context;
    }


    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task SendBid(Bid bid)
    {
        await Clients.All.SendAsync("ReceiveBid", bid);
    }

    public async Task Bid(string user, decimal value)
    {
        Console.WriteLine("Invoked");
        var bid = new Bid
        {
            Bidder = new User(),
            Value = value,
            Time = DateTime.UtcNow
        };
        await Clients.All.SendAsync("Bid", bid);
    }


    public async Task GetBids(int auctionId)
    {
        Console.WriteLine("GetBids");
        var bids = await _context.Set<Bid>()
            .Include(b => b.Bidder)
            .Include(b => b.Auction)
            .Where(b => b.Auction.Id == auctionId)
            .OrderByDescending(b => b.Time)
            .ToListAsync();
        var bidList = new List<BidDto>();
        foreach (var bid in bids)
        {
            bidList.Add(new BidDto
            {
                AuctionId = bid.Auction.Id,
                BidderName  = bid.Bidder.Name,
                Value = bid.Value,
                Time = bid.Time,
            });
        }

        await Clients.Caller.SendAsync("ReceiveBids", bidList);
    }
    
    
    
}