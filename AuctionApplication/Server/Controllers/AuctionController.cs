using AuctionApplication.Shared;
using Microsoft.AspNetCore.Mvc;

namespace AuctionApplication.Server.Controllers;

public class AuctionController : ControllerBase
{
    [HttpGet]
    [Route("/Auctions")]
    public IEnumerable<Auction> Get()
    {
        return new List<Auction>();
    }
}