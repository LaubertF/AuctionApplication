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
    
    public async Task<Bid?> PostBid(Bid bid, int auctionId)
    {
        var auction = await _context.Set<Auction>().FirstOrDefaultAsync(a => a.Id == auctionId);
        if (auction == null) return null;
        if (auction.Winner != null) return null;
        bid.Auction = auction;
        if (auction.EndInclusive < DateTime.UtcNow || auction.StartInclusive > DateTime.UtcNow) return null;
        
        if (bid.Value >= auction.BuyoutPrice)
        {
            bid.Value = auction.BuyoutPrice;
            auction.Winner = bid.Bidder;
            await _context.Set<Bid>().AddAsync(bid);
            await _context.SaveChangesAsync();
            return bid;
        }
        
        var currentBids = await _context.Set<Bid>().Where(b => b.Auction.Id == auctionId).ToListAsync();
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