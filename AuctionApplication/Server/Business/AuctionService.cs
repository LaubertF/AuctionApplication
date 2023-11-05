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

    public async Task CheckAuctionsForCompletion()
    {
        var auctions = await _context.Set<Auction>().Where(x => x.Winner == null).ToListAsync();
        foreach (var auction in auctions)
        {
            CheckAuctionForCompletion(auction);
        }
    }

    /**
     * Returns true if the auction is completed, false otherwise.
     */
    public bool CheckAuctionForCompletion(Auction auction)
    {
        if (auction.EndInclusive >= DateTime.UtcNow) return false;
        if (auction.Winner != null) return true;
        var topBid = _context.Set<Bid>().Where(b => b.Auction.Id == auction.Id).Include(b => b.Bidder).ToList().MaxBy(b => b.Value);
        if (topBid == null)
        {
            return true;
        }

        auction.Winner = topBid.Bidder;

        var payment = new Payment
        {
            Auction = auction,
            User = auction.Winner,
            Amount = topBid.Value,
            State = PaymentState.New
        };

        _context.Set<Payment>().Add(payment);
        _context.SaveChanges();
        return true;
    }
}