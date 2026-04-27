using konditeremApiv2.Models;
using Microsoft.EntityFrameworkCore;

namespace konditeremApiv2.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Gym> Gyms { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<GymHasProduct> GymHasProducts { get; set; }
    public DbSet<Purchase> Purchases { get; set; }
}