using System.Security.Claims;
using AuctionApplication.Database;
using AuctionApplication.Shared;
using Microsoft.EntityFrameworkCore;

namespace AuctionApplication.Server.Business;

public class UserService
{
    //repository
    private readonly EfRepository<User> _userRepository;
    private readonly DbContext _context;

    public UserService(EfRepository<User> userRepository, DbContext context)
    {
        _userRepository = userRepository;
        _context = context;
    }


    public async Task<User> GetUserByAuth0Id(ClaimsPrincipal user)
    {
        var auth0Id = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (auth0Id == null) throw new Exception();
        var userFromDb = await _context.Set<User>().FirstOrDefaultAsync(u => u.Auth0Id == auth0Id.Value);
        if (userFromDb != null) return userFromDb;
        var newUser = new User
        {
            Auth0Id = auth0Id.Value,
            Name = user.Claims.FirstOrDefault(c => c.Type == "custom_email")?.Value
        };
        await _userRepository.AddAsync(newUser);
        return newUser;
    }
}