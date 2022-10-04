using System;
using Microsoft.EntityFrameworkCore;
using API.Entities;
using System.Linq;


namespace API.Data
{

    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<AppUser> Users { get; set; }
    }
}