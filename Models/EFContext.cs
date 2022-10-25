using Microsoft.EntityFrameworkCore;
using FuelPriceReadAPI.Models;

namespace EFCore.Models
{ 
    public class EFContext : DbContext
    { 
        private const string connectionString = "Server=(localdb)\\mssqllocaldb;Database=EFCore; Trusted_Connection=True";
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {   
            optionsBuilder.UseSqlServer(connectionString);
        }

        public DbSet<FuelPrice> FuelPrices { get; set; }
    }
}