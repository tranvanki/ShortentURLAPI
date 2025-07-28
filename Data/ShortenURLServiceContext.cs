using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShortenURLService.Models;

namespace ShortenURLService.Data
{
    public class ShortenURLServiceContext : DbContext
    {
        public ShortenURLServiceContext (DbContextOptions<ShortenURLServiceContext> options)
            : base(options)
        {
        }

        public DbSet<ShortenURLService.Models.URL> URL { get; set; } = default!;
    }
}
