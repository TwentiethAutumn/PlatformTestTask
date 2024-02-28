using Microsoft.EntityFrameworkCore;
using TestTaskPlatform.Models;

namespace TestTaskPlatform.Context;

public class ApiContext : DbContext
{
    public ApiContext(DbContextOptions<ApiContext> options)
        : base(options)
    { }

    public DbSet<User> Users { get; set; }
    public DbSet<MoveHistory> MoveHistory { get; set; }
    public DbSet<GameStats> GameStats { get; set; }
}