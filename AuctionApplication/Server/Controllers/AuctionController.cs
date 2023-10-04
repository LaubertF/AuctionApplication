using AuctionApplication.Database;
using AuctionApplication.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionApplication.Server.Controllers;

public class AuctionController : ControllerBase
{
    
    private readonly DbContext _context;
    private readonly EfRepository<Auction> _auctionRepository;
    public AuctionController(DbContext context, EfRepository<Auction> auctionRepository)
    {
        _context = context;
        _auctionRepository = auctionRepository;
    }

    [HttpGet]
    [Route("/Auctions")]
    public async Task<IList<Auction>> Get()
    {
        return await _auctionRepository.ListAsync();
    }

    [HttpPost]
    [Route("/Auction")]
    public async Task Post([FromBody] AuctionFrontEndModel auction)
    {
        var newAuction = new Auction();//TODO: Map
        newAuction.Owner = new User();
        newAuction.Owner.Nickname = "Test";
        await _auctionRepository.AddAsync(newAuction);
    }
}

public record AuctionFrontEndModel(string NameOfProduct,string Description,decimal StartingPrice, DateTime EndInclusive);