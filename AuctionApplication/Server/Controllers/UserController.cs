using System.Security.Claims;
using AuctionApplication.Server.Business;
using AuctionApplication.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionApplication.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly AuctionService _auctionService;
    private readonly DbContext _context;

    public UserController(DbContext context, UserService userService, AuctionService auctionService)
    {
        _context = context;
        _userService = userService;
        _auctionService = auctionService;
    }
    
    
    [HttpGet]
    [Route("/Users")]
    public async Task<IList<User>> Get()
    {
        return await _context.Set<User>().ToListAsync();
    }
    
    [HttpGet]
    [Route("/Users/{id:int}")]
    public async Task<OkObjectResult> GetUserById(int id)
    {
        return Ok(await _context.Set<User>().FirstOrDefaultAsync(a => a.Id == id));
    }
    
    [HttpPut]
    [Route("/Users/{id:int}")]
    public async Task<IActionResult> UpdateUserById(int id, [FromBody] User formData)
    {
        var user = await _context.Set<User>().FirstOrDefaultAsync(a => a.Id == id);
        if (user == null)
        {
            return NotFound();
        }
        user.Name = formData.Name;
        await _context.SaveChangesAsync();
        return Ok(user);
    }
    
    [HttpPut]
    [Route("/User/Current")]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] FullName fullName)
    {
        var currentUser = await _userService.GetUserByAuth0Id(User);
        var user = await _context.Set<User>().FirstOrDefaultAsync(a => a.Auth0Id == currentUser.Auth0Id);
        if (user == null)
        {
            return NotFound();
        }
        user.FirstName = fullName.firstName;
        user.LastName = fullName.lastName;
        await _context.SaveChangesAsync();
        return Ok(user);
    }
    
    [HttpDelete]
    [Route("/Users/{id:int}")]
    public async Task<IActionResult> DeleteUserById(int id)
    {
        var user = await _context.Set<User>().FirstOrDefaultAsync(a => a.Id == id);
        if (user == null)
        {
            return NotFound();
        }
        _context.Set<User>().Remove(user);
        await _context.SaveChangesAsync();
        return Ok();
    }
    
    [HttpGet]
    [Route("/User/Wins")]
    public async Task<IActionResult> GetWins()
    {
        IList<WinsDto> winsDtos = new List<WinsDto>();
        var currentUser = await _userService.GetUserByAuth0Id(User);
        var user = await _context.Set<User>().FirstOrDefaultAsync(a => a.Auth0Id == currentUser.Auth0Id);
        if (user == null)
        {
            return NotFound();
        }
        var auctions = await _context.Set<Auction>()
            .Select(auction => new Auction
            {
                Id = auction.Id,
                NameOfProduct = auction.NameOfProduct,
                Winner = auction.Winner,
                PaymentId = auction.PaymentId
            })
            .Where(a => a.Winner != null && a.Winner.Auth0Id == user.Auth0Id).ToListAsync();

        foreach (var auction in auctions)
        {
            WinsDto win = new WinsDto();
            var onePayment = await _context.Set<Payment>()
                .Select(payment => new Payment()
                {
                    Id = payment.Id,
                    Value = payment.Value,
                    State = payment.State
                })
                .Where(a => a.Id == auction.PaymentId)
                .FirstAsync();
            
            // Create obj. winsDto
            win.AuctionId = auction.Id;
            win.NameOfProduct = auction.NameOfProduct;
            win.Value = onePayment.Value;
            win.State = onePayment.State;
            
            // Add obj. to the list
            winsDtos.Add(win);
        }
        return Ok(winsDtos);
    }
    
    [HttpGet]
    [Route("/User")]
    public async Task<OkObjectResult> GetCurrentUserInfo()
    {
        var currentUser = await _userService.GetUserByAuth0Id(User);
        var user = await _context.Set<User>().FirstOrDefaultAsync(a => a.Auth0Id == currentUser.Auth0Id);
        return Ok(user);
    }
    
    [HttpGet]
    [Route("/Users/{id:int}/WonAuctions")]
    public async Task<IActionResult> GetWonAuctions(int id)
    {
        var user = await _context.Set<User>().FirstOrDefaultAsync(a => a.Id == id);
        if (user == null)
        {
            return NotFound();
        }
        var auctions = await _context.Set<Auction>().Where(a => a.Winner == user).ToListAsync();
        return Ok(auctions);
    }

    [HttpPost]
    [Route("/Users/Name")]
    public IActionResult SetUsername([FromBody] string username)
    {
        try
        {
            var user = _context.Set<User>().FirstOrDefault(u =>
                u.Auth0Id == User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
            if (user != null) user.Name = username;
            _context.SaveChanges();
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return NotFound();
        }

    }

    
}