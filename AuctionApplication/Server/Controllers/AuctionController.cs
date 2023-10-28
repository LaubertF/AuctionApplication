using AuctionApplication.Database;
using AuctionApplication.Server.Business;
using AuctionApplication.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionApplication.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class AuctionController : ControllerBase
{
    private readonly DbContext _context;
    private readonly UserService _userService;
    private readonly EfRepository<Auction> _auctionRepository;

    public AuctionController(DbContext context, EfRepository<Auction> auctionRepository, UserService userService)
    {
        _context = context;
        _auctionRepository = auctionRepository;
        _userService = userService;
    }

    [HttpGet]
    [Route("/Auctions")]
    public async Task<IList<Auction>> Get()
    {
        return await _auctionRepository.ListAsync();
    }

    [HttpPost]
    [Route("/Auction")]
    public async Task Post([FromBody] AuctionDto auction)
    {
        var newAuction = new Auction
        {
            Owner = new User
            {
                Nickname = "Test"
            }
        }; //TODO: Map
        await _auctionRepository.AddAsync(newAuction);
    }

    [HttpPost]
    [Route("/create2")]
    public IActionResult CreateAction([FromBody] Auction formData)
    {
        return Ok(formData);
        // return BadRequest() 
    }

    [HttpGet]
    [Route("/test")]
    public IActionResult Test()
    {
        return Ok();
    }

    [HttpPost]
    [Route("/create")]
    public async Task<IActionResult> CreateAction2([FromBody] Auction formData)
    {
        var user = await _userService.GetUserByAuth0Id(User);
        formData.Owner = user;
        await _auctionRepository.AddAsync(formData);
        return Ok(formData);
    }
}

public record AuctionDto(string NameOfProduct, string Description, decimal StartingPrice, DateTime EndInclusive);