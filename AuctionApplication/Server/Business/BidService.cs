using AuctionApplication.Shared;
using Microsoft.EntityFrameworkCore;

namespace AuctionApplication.Server.Business;

public class BidService
{
    private readonly DbContext _context;

    public BidService(DbContext context)
    {
        _context = context;
    }

    public async Task<Bid?> PostBid(decimal value, Auction auction, User user)
    {
        var bid = new Bid
        {
            Value = value,
            Bidder = user
        };
        if (auction.Winner != null) return null;
        bid.Auction = auction;
        if (auction.EndInclusive < DateTime.UtcNow || auction.StartInclusive > DateTime.UtcNow) return null;

        if (auction.BuyoutPrice != null && bid.Value >= auction.BuyoutPrice)
        {
            bid.Value = (decimal)auction.BuyoutPrice;
            auction.Winner = bid.Bidder;
            await _context.Set<Bid>().AddAsync(bid);
            await _context.SaveChangesAsync();
            return bid;
        }

        var currentBids = await _context.Set<Bid>().Where(b => b.Auction == auction).ToListAsync();
        if (currentBids.Count > 0)
        {
            var highestBid = currentBids.Max(b => b.Value);
            if (bid.Value <= highestBid) return null;
        }

        await _context.Set<Bid>().AddAsync(bid);
        await _context.SaveChangesAsync();
        return bid;
    }
}