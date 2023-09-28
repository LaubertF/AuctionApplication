using AuctionApplication.Shared;
using Microsoft.AspNetCore.SignalR;

namespace AuctionApplication.Server.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task Bid(string user,decimal value)
    {
        Console.WriteLine("Invoked");
        var bid = new Bid
        {
            BidderName = user,
            Value = value,
            Time = DateTime.UtcNow
        };
        await Clients.All.SendAsync("Bid", bid);
    }
}