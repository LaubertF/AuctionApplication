using System.Net;
using System.Security.Claims;
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
    private readonly IHubContext<BidHub> _hubContext;
    private readonly AuctionService _auctionService;
    private readonly BidService _bidService;

    public AuctionController(DbContext context, EfRepository<Auction> auctionRepository, UserService userService,
        IHubContext<BidHub> hubContext, AuctionService auctionService, BidService bidService)
    {
        _context = context;
        _auctionRepository = auctionRepository;
        _userService = userService;
        _hubContext = hubContext;
        _auctionService = auctionService;
        _bidService = bidService;
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
        await _auctionService.CheckAuctionsForCompletion();
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
            _auctionService.CheckAuctionForCompletion(auction);
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
        if (_auctionService.CheckAuctionForCompletion(auction))
        {
            return BadRequest();
        }
        var userP = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var user = _context.Set<User>().FirstOrDefault(u => u.Auth0Id == userP);
        if (user == null) return StatusCode((int)HttpStatusCode.Forbidden);
        var bid = await _bidService.PostBid(value, auction, user);
        if (bid == null) return BadRequest();
        await _hubContext.Clients.All.SendAsync("SendBid", bid);
        return Ok(bid);
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
        if (auctionToUpdate.Owner.Id != requester.Id || !requester.IsAdmin)
            return StatusCode((int)HttpStatusCode.Forbidden);
        auctionToUpdate.NameOfProduct = auction.NameOfProduct;
        auctionToUpdate.Description = auction.Description;
        auctionToUpdate.StartingPrice = auction.StartingPrice;
        auctionToUpdate.EndInclusive = auction.EndInclusive;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    [Route("/Pay/{id:int}")]
    public async Task<IActionResult> Pay(int id)
    {
        var auction = await _context.Set<Auction>().FirstOrDefaultAsync(a => a.Id == id);
        if (auction == null) return NotFound();
        if (!_auctionService.CheckAuctionForCompletion(auction)) return BadRequest();
        var payment = await _context.Set<Payment>().FirstOrDefaultAsync(p => p.Auction == auction);
        if (payment == null) return NotFound();
        payment.State = PaymentState.Paid;
        await _context.SaveChangesAsync();
        return Ok();
    }
}

public record AuctionDto(string NameOfProduct, string Description, decimal StartingPrice, DateTime EndInclusive);