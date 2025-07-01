using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UserAuthApp.Models;

namespace UserAuthApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<TaskItem> TaskItems { get; set; }

        public DbSet<User> Users { get; set; }
    }
}
