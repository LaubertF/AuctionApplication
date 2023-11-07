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
    private readonly DbContext _context;

    public UserController(DbContext context, UserService userService)
    {
        _context = context;
        _userService = userService;
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
                Category = auction.Category,
                StartingPrice = auction.StartingPrice,
                ProductImages = auction.ProductImages,
                Owner = auction.Owner,
                Winner = auction.Winner
            })
            .Where(a => a.Winner != null && a.Winner.Auth0Id == user.Auth0Id).ToListAsync();
        return Ok(auctions);
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