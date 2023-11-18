using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    
    [HttpGet]
    [Route("/Auctions")]
    public async Task<IList<Auction>> Get()
    {
        await _auctionService.CheckAuctionsForCompletion();
        var auctions = await _context.Set<Auction>()
            .Select(auction => new Auction
            {
                Id = auction.Id,
                NameOfProduct = auction.NameOfProduct,
                Category = auction.Category,
                StartingPrice = auction.StartingPrice,
                ProductImages = auction.ProductImages,
                Owner = auction.Owner,
                IsClosed = auction.IsClosed,
                Winner = auction.Winner
            })
            .Where(a => a.IsClosed == false)
            .ToListAsync();
        
        foreach (var auction in auctions)
        {
            auction.StartingPrice = await _auctionService.GetMinBidValueForAuctionAsync(auction.Id);
        }
        return auctions;
    }
    
    [HttpGet]
    [Route("/Auctions/Category/{name}")]
    public async Task<IList<Auction>> GetByCategory(string name)
    {
        await _auctionService.CheckAuctionsForCompletion();
        var auctions = await _context.Set<Auction>()
            .Select(auction => new Auction
            {
                Id = auction.Id,
                NameOfProduct = auction.NameOfProduct,
                Category = auction.Category,
                StartingPrice = auction.StartingPrice,
                ProductImages = auction.ProductImages,
                Owner = auction.Owner,
                IsClosed = auction.IsClosed,
                Winner = auction.Winner
            })
            .Where(a => a.Category != null && a.Category.Name == name && a.IsClosed == false).ToListAsync();

        foreach (var auction in auctions)
        {
            auction.StartingPrice = await _auctionService.GetMinBidValueForAuctionAsync(auction.Id);
        }
        return auctions;
    }
    
    [HttpGet]
    [Route("/Auctions/Name/{name}")]
    public async Task<IList<Auction>> GetByName(string name)
    {
        await _auctionService.CheckAuctionsForCompletion();
        var auctions = await _context.Set<Auction>()
            .Select(auction => new Auction
            {
                Id = auction.Id,
                NameOfProduct = auction.NameOfProduct,
                Category = auction.Category,
                StartingPrice = auction.StartingPrice,
                ProductImages = auction.ProductImages,
                Owner = auction.Owner,
                IsClosed = auction.IsClosed,
                Winner = auction.Winner
            })
            .Where(a =>  EF.Functions.Like(a.NameOfProduct, $"%{name}%") && a.IsClosed == false).ToListAsync();
        
        foreach (var auction in auctions)
        {
            auction.StartingPrice = await _auctionService.GetMinBidValueForAuctionAsync(auction.Id);
        }
        return auctions;
    }
    
    [HttpGet]
    [Route("/Auctions/All")]
    public async Task<IList<AuctionDto>> GetAll()
    {
        IList<Auction> auctions = new List<Auction>();
        auctions = await _context.Set<Auction>()
            .Include(a => a.Owner)
            .Include(a => a.Winner)
            .Include(a => a.Category)
            .ToListAsync();
        IList<AuctionDto> auctionDtos = auctions.Select(auction => new AuctionDto
        {
            Id = auction.Id,
            Title = auction.NameOfProduct,
            Description = auction.Description,
            Category = auction.Category,
            StartInclusive = auction.StartInclusive,
            EndInclusive = auction.EndInclusive,
            StartingPrice = auction.StartingPrice,
            BuyoutPrice = auction.BuyoutPrice,
            OwnerName = auction.Owner.Name,
            WinnerName = auction.Winner?.Name,
            IsClosed = auction.IsClosed
        }).ToList();
        
        return auctionDtos;
    }
    
    [HttpGet]
    [Route("/Auctions/New")]
    public async Task<IActionResult> GetDefaultAuction()
    {
        try
        {
            Auction auction = new Auction();
            AuctionCategory category;
            try
            {
                category = await _context.Set<AuctionCategory>()
                    .FirstAsync(a => a.Name == "Other");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound($"Auction categories do not exist.");
            }

            auction.Category = category;
            return Ok(auction);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost]
    [Route("/Auctions/Create")]
    public async Task<IActionResult> CreateAuction([FromBody] Auction formData)
    {
        try
        {
            var user = await _userService.GetUserByAuth0Id(User);
            formData.Owner = user;
            var categoryName = formData.Category.Name;
            formData.Category = await _context.Set<AuctionCategory>().FirstOrDefaultAsync(c => c.Name == categoryName);
            var auction = await _auctionRepository.AddAsync(formData);
            
            return Ok(auction.Id);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.InnerException);
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
                .Include(a => a.Category)
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
            return BadRequest($"Payment was not successfull");
        }

        return Ok(payment);
    }
    
    [HttpDelete]
    [Route("/Auctions/{id:int}")]
    [Authorize(Policy = "RequireAdministratorRole")]
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
    
    [HttpPut]
    [Route("/Auctions/{id:int}")]
    [Authorize(Policy = "RequireAdministratorRole")]
    public async Task<IActionResult> UpdateAuction(int id, [FromBody] Auction auction)
    {
        var requester = await _userService.GetUserByAuth0Id(User);
        var auctionToUpdate = await _context.Set<Auction>().FirstOrDefaultAsync(a => a.Id == id);
        if (auctionToUpdate == null) return NotFound();
        if (auctionToUpdate.Owner.Id != requester.Id)
            return StatusCode((int)HttpStatusCode.Forbidden);
        auctionToUpdate.NameOfProduct = auction.NameOfProduct;
        auctionToUpdate.Description = auction.Description;
        auctionToUpdate.Category = auction.Category;
        auctionToUpdate.StartingPrice = auction.StartingPrice;
        auctionToUpdate.BuyoutPrice = auction.BuyoutPrice;
        auctionToUpdate.StartInclusive = auction.StartInclusive;
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
    [Route("/Auctions/Categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Set<AuctionCategory>().ToListAsync();
        return Ok(categories);
    }
    
    [HttpGet]
    [Route("/Auctions/Categories/{name}")]
    [Authorize(Policy = "RequireAdministratorRole")]
    public async Task<IActionResult> GetCategory(string name)
    {
        var category = await _context.Set<AuctionCategory>().FirstOrDefaultAsync(c => c.Name == name);
        if (category == null) return NotFound();
        return Ok(category);
    }
    
    [HttpPost]
    [Route("/Auctions/Categories")]
    [Authorize(Policy = "RequireAdministratorRole")]
    public async Task<IActionResult> CreateCategory([FromBody] AuctionCategory category)
    {
        bool categoryExist = await _context.Set<AuctionCategory>().AnyAsync(c => c.Name == category.Name);
        if (categoryExist)
        {
            return BadRequest($"Category with that name already exists.");  
        }
        await _context.Set<AuctionCategory>().AddAsync(category);
        await _context.SaveChangesAsync();
        return Ok();
    }
    
    
    [HttpDelete]
    [Route("/Auctions/Categories/{name}")]
    [Authorize(Policy = "RequireAdministratorRole")]
    public async Task<IActionResult> DeleteCategory(string name)
    {
        var category = await _context.Set<AuctionCategory>().FirstOrDefaultAsync(c => c.Name == name);
        if (category == null) return NotFound();
        _context.Set<AuctionCategory>().Remove(category);
        await _context.SaveChangesAsync();
        return Ok();
    }
}