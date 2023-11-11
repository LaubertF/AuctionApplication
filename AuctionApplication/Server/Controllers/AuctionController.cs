using System.Net;
using System.Text.Json;
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
    private readonly AuctionService _auctionService;
    private readonly EfRepository<Auction> _auctionRepository;
    private readonly IHubContext<BidHub> _hubContext;

    public AuctionController(DbContext context, EfRepository<Auction> auctionRepository, UserService userService,
        IHubContext<BidHub> hubContext, AuctionService auctionService)
    {
        _context = context;
        _auctionRepository = auctionRepository;
        _userService = userService;
        _hubContext = hubContext;
        _auctionService = auctionService;
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
        Auction auction;
        try
        {
            auction = await _context.Set<Auction>()
                .Include(a => a.ProductImages)
                .Include(a => a.Owner)
                .FirstAsync(a => a.Id == id);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return NotFound($"Auction detail with ID {id} does not exist.");
        }

        try
        {
            auction.StartingPrice = await _auctionService.GetMinBidValueForAuctionAsync(id);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return NotFound($"Could not get minimal starting bid value");
        }

        _auctionService.CheckAuctionForCompletion(auction);

        return Ok(auction);
    }

    [HttpPost]
    [Route("/Auctions/{id:int}/Bid")]
    public async Task<IActionResult> BidOnAuction(int id, [FromBody] decimal value)
    {
        var auction = await _context.Set<Auction>().FirstOrDefaultAsync(a => a.Id == id);
        if (auction == null) return NotFound();
        var user = await _userService.GetUserByAuth0Id(User);

        if (auction.Owner == user)
        {
            return BadRequest($"You cannot bid on your own auction");
        }

        var minBidValue = await _auctionService.GetMinBidValueForAuctionAsync(id);
        if (value <= minBidValue)
        {
            return BadRequest($"The value must be greater that actual minimal bid value ({minBidValue} €)");
        }

        var newBid = new Bid
        {
            Auction = auction,
            Bidder = user,
            Value = value,
            Time = DateTime.Now,
        };
        await _context.Set<Bid>().AddAsync(newBid);
        await _context.SaveChangesAsync();

        var bidData = new BidDto
        {
            AuctionId = auction.Id,
            BidderName = user.Name,
            Value = value,
            Time = DateTime.Now,
        };
        await _hubContext.Clients.All.SendAsync("SendBid", bidData);
        return Ok(newBid);
    }

    [HttpPost]
    [Route("/Auctions/{id:int}/Buyout")]
    public async Task<IActionResult> BuyoutOnAuction(int id, [FromBody] decimal value)
    {
        var auction = await _context.Set<Auction>().FirstOrDefaultAsync(a => a.Id == id);
        if (auction == null) return NotFound();
        if (auction.IsClosed == true)
        {
            return BadRequest("Auction already ended");
        }

        try
        {
            var winner = await _userService.GetUserByAuth0Id(User);
            
            if (auction.Owner == winner)
            {
                return BadRequest($"You cannot buy your own auction");
            }

            auction.Winner = winner;
            auction.IsClosed = true;

            Payment payment = new Payment
            {
                Auction = auction,
                User = winner,
                Value = value,
                DateCreated = DateTime.Now
            };

            await _context.Set<Payment>().AddAsync(payment);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return BadRequest($"Could not save payment");
        }

        return NoContent();
    }

    [HttpGet]
    [Route("/Payments/{id:int}")]
    public async Task<ObjectResult> GetPaymentById(int id)
    {
        Auction auction;
        Payment payment;
        try
        {
            auction = await _context.Set<Auction>()
                .FirstAsync(a => a.Id == id);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return NotFound($"Auction detail with ID {id} does not exist.");
        }

        if (auction.IsClosed == false)
        {
            return BadRequest($"The auction was not closed yet.");
        }

        var winner = await _userService.GetUserByAuth0Id(User);
        if (auction.Winner != null && auction.Winner.Id != winner.Id)
        {
            return BadRequest($"You did not win this auction!");
        }

        try
        {
            payment = await _context.Set<Payment>()
                .Include(a => a.Auction)
                .Include(a => a.User)
                .Where(a => a.Auction.Id == id)
                .Where(a => a.User.Id == winner.Id)
                .FirstAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return NotFound($"Payment for auction with ID {id} does not exist");
        }

        if (payment.State == PaymentState.Registered)
        {
            return BadRequest($"Payment was already registered!");
        }
        
        if (payment.State == PaymentState.Paid)
        {
            return BadRequest($"The auction was already paid for!");
        }
        
        return Ok(new PaymentDto
        {
            Id = payment.Id,
            Value = payment.Value,
            NameOfProduct = payment.Auction.NameOfProduct
        });
    }

    [HttpPost]
    [Route("/Payments/{id:int}")]
    public async Task<ObjectResult> PostPaymentById(int id, [FromBody] decimal value)
    {
        Payment payment;
        try
        {
            payment = await _context.Set<Payment>()
                .FirstAsync(a => a.Id == id);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return NotFound($"Payment detail with ID {id} does not exist.");
        }

        try
        {
            payment.State = PaymentState.Registered;
            payment.DateRegisterd = DateTime.Now;
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return NotFound($"Payment was not successfull");
        }

        return Ok(payment);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete]
    [Route("/Auctions/{id:int}")]
    public async Task<IActionResult> DeleteAuction(int id)
    {
        var requester = await _userService.GetUserByAuth0Id(User);
        var auction = await _context.Set<Auction>().FirstOrDefaultAsync(a => a.Id == id);
        if (auction == null) return NotFound();
        if (auction.Owner.Id != requester.Id) return StatusCode((int)HttpStatusCode.Forbidden);
        _context.Set<Auction>().Remove(auction);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    [Route("/Auctions/{id:int}")]
    public async Task<IActionResult> UpdateAuction(int id, [FromBody] Auction auction)
    {
        var requester = await _userService.GetUserByAuth0Id(User);
        var auctionToUpdate = await _context.Set<Auction>().FirstOrDefaultAsync(a => a.Id == id);
        if (auctionToUpdate == null) return NotFound();
        if (auctionToUpdate.Owner.Id != requester.Id)
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
    
    [HttpGet]
    [Route("/Auctions/Category/")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Set<CustomAuctionCategory>().ToListAsync();
        return Ok(categories);
    }
    
    [HttpPost]
    [Route("/Auctions/Category/")]
    public async Task<IActionResult> CreateCategory([FromBody] CustomAuctionCategory category)
    {
        await _context.Set<CustomAuctionCategory>().AddAsync(category);
        await _context.SaveChangesAsync();
        return Ok();
    }
    
    
    [HttpDelete]
    [Route("/Auctions/Category/{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Set<CustomAuctionCategory>().FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return NotFound();
        _context.Set<CustomAuctionCategory>().Remove(category);
        await _context.SaveChangesAsync();
        return Ok();
    }
}

public record AuctionDto(string NameOfProduct, string Description, decimal StartingPrice, DateTime EndInclusive);