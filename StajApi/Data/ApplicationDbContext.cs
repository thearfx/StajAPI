// StajApi/Data/ApplicationDbContext.cs

using Microsoft.EntityFrameworkCore;
using StajApi.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace StajApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<WeatherEntry> WeatherEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // İhtiyaç olursa burada veritabanı tablolarınız için özel ayarlar yapabilirsiniz
            base.OnModelCreating(modelBuilder);
        }
    }
}