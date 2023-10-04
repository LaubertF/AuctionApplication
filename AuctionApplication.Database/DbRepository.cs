using Ardalis.Specification.EntityFrameworkCore;
using AuctionApplication.Shared;
using Microsoft.EntityFrameworkCore;

namespace AuctionApplication.Database;

public class EfRepository<T> : RepositoryBase<T> where T : class
{
    public EfRepository(DbContext dbContext) : base(dbContext)
    {
    }
}