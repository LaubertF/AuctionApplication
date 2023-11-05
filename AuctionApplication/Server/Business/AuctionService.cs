using AuctionApplication.Shared;
using Microsoft.EntityFrameworkCore;

namespace AuctionApplication.Server.Business;

public class AuctionService
{
    private readonly DbContext _context;
    
    public AuctionService(DbContext context)
    {
        _context = context;
    }
    
    public async Task<decimal> GetMinBidValueForAuctionAsync(int auctionId)
    {
        var auction = await _context.Set<Auction>().FirstAsync(a => a.Id == auctionId);
        
        var bids  = await _context.Set<Bid>()
            .Where(b => b.Auction.Id == auctionId)
            .ToListAsync();
        
        return bids.Any() ? bids.Max(b => b.Value) : auction.StartingPrice;
    }
}