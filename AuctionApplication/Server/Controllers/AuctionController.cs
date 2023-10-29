using System.Net;
using AuctionApplication.Database;
using AuctionApplication.Server.Business;
using AuctionApplication.Server.Hubs;
using AuctionApplication.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AuctionApplication.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class AuctionController : ControllerBase
{
    private readonly DbContext _context;
    private readonly UserService _userService;
    private readonly EfRepository<Auction> _auctionRepository;
    private readonly IHubContext<ChatHub> _hubContext;

    public AuctionController(DbContext context, EfRepository<Auction> auctionRepository, UserService userService, IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _auctionRepository = auctionRepository;
        _userService = userService;
        _hubContext = hubContext;
    }

    [HttpPost]
    [Route("/test")]
    public IActionResult Test()
    {
        _hubContext.Clients.All.SendAsync("SendBid", new Bid());
        _hubContext.Clients.All.SendAsync("ReceiveMessage", "user", "message");
        return Ok();
    }
    
    [HttpGet]
    [Route("/Auctions")]
    public async Task<IList<Auction>> Get()
    {
        return await _auctionRepository.ListAsync();
    }

    [HttpPost]
    [Route("/Auctions/Create")]
    public async Task<IActionResult> CreateAction([FromBody] Auction formData)
    {
        try
        {
            var user = await _userService.GetUserByAuth0Id(User);
            formData.Owner = user;
            var auction = await _auctionRepository.AddAsync(formData);
            return Ok(auction.Id);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    [Route("/Auctions/{id:int}")]
    public async Task<ObjectResult> GetAuctionById(int id)
    {
        try
        {
            Auction auction = await _context.Set<Auction>()
                .Include(a => a.ProductImages)
                .FirstAsync(a => a.Id == id);
            return Ok(auction);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return NotFound($"Auction detail with ID {id} does not exist.");
        }
    }

    [HttpPost]
    [Route("/Auctions/{id:int}/Bid")]
    public async Task<IActionResult> BidOnAuction(int id, [FromBody] decimal value)
    {
        var auction = await _context.Set<Auction>().FirstOrDefaultAsync(a => a.Id == id);
        if (auction == null) return NotFound();
        var user = await _userService.GetUserByAuth0Id(User);
        var newBid = new Bid
        {
            Auction = auction,
            Bidder = user,
            Value = value
        };
        await _context.Set<Bid>().AddAsync(newBid);
        await _context.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("SendBid", newBid);
        return Ok(newBid);
    }

    [HttpDelete]
    [Route("/Auctions/{id:int}")]
    public async Task<IActionResult> DeleteAuction(int id)
    {
        var requester = await _userService.GetUserByAuth0Id(User);
        var auction = await _context.Set<Auction>().FirstOrDefaultAsync(a => a.Id == id);
        if (auction == null) return NotFound();
        if (auction.Owner.Id != requester.Id || !requester.IsAdmin) return StatusCode((int)HttpStatusCode.Forbidden);
        _context.Set<Auction>().Remove(auction);
        await _context.SaveChangesAsync();
        return Ok();
    }
    
    [HttpPut]
    [Route("/Auctions/{id:int}")]
    public async Task<IActionResult> UpdateAuction(int id, [FromBody] Auction auction)
    {
        var requester = await _userService.GetUserByAuth0Id(User);
        var auctionToUpdate = await _context.Set<Auction>().FirstOrDefaultAsync(a => a.Id == id);
        if (auctionToUpdate == null) return NotFound();
        if (auctionToUpdate.Owner.Id != requester.Id || !requester.IsAdmin) return StatusCode((int)HttpStatusCode.Forbidden);
        auctionToUpdate.NameOfProduct = auction.NameOfProduct;
        auctionToUpdate.Description = auction.Description;
        auctionToUpdate.StartingPrice = auction.StartingPrice;
        auctionToUpdate.EndInclusive = auction.EndInclusive;
        await _context.SaveChangesAsync();
        return Ok();
    }
}

public record AuctionDto(string NameOfProduct, string Description, decimal StartingPrice, DateTime EndInclusive);
