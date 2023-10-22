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
    
    public async Task<User> GetUserByAuth0Id(string auth0Id)
    {
        
        var user =  await _context.Set<User>().FirstOrDefaultAsync(u => u.Auth0Id == auth0Id);
        if (user != null) return user;
        user = new User
        {
            Auth0Id = auth0Id
        };
        await _userRepository.AddAsync(user);
        return user;
    }

    public async Task<User> GetUserByAuth0Id(ClaimsPrincipal user)
    {
        var auth0Id = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (auth0Id != null) return await GetUserByAuth0Id(auth0Id.Value);
        throw new Exception("User not found");
    }
    
}