﻿using AuctionApplication.Server.Hubs;
using AuctionApplication.Shared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Radzen;

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

        var bids = await _context.Set<Bid>()
            .Where(b => b.Auction.Id == auctionId)
            .ToListAsync();

        return bids.Any() ? bids.Max(b => b.Value) : auction.StartingPrice;
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
        if (auction.EndInclusive >= DateTime.Now) return false;
        if (auction.IsClosed) return true;
        
        auction.IsClosed = true;
        var topBid = _context.Set<Bid>().Where(b => b.Auction.Id == auction.Id).Include(b => b.Bidder).ToList()
            .MaxBy(b => b.Value);
        if (topBid == null)
        {
            _context.SaveChanges();
            return true;
        }

        auction.Winner = topBid.Bidder;

        var payment = new Payment
        {
            Auction = auction,
            User = auction.Winner,
            Value = topBid.Value,
            State = PaymentState.New
        };

        _context.Set<Payment>().Add(payment);
        _context.SaveChanges();
        return true;
    }


    public async void SendNotification(IHubContext<BidHub> context, string connectionId, string summary, string message)
    {

        var notificationMessage = new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = summary,
            Detail = message,
            Duration = 15000
        };
        await context.Clients.All.SendAsync("ReceiveAuctionNotification", connectionId, notificationMessage);
    }
}