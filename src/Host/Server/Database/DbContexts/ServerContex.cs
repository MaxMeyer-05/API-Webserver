using Microsoft.EntityFrameworkCore;
using Server.Database.Entities;

namespace Server.Database.DbContexts;
public class ServerDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public ServerDbContext(DbContextOptions<ServerDbContext> options) 
        : base(options)
    {
    }
}