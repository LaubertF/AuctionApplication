using AuctionApplication.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionApplication.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly DbContext _context;

    public UserController(DbContext context)
    {
        _context = context;
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
}