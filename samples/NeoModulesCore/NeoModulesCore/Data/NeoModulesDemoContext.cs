using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NeoModulesCore.Models
{
    public class NeoModulesDemoContext : DbContext
    {
        public NeoModulesDemoContext (DbContextOptions<NeoModulesDemoContext> options)
            : base(options)
        {
        }

        public DbSet<NeoModulesCore.Models.Wallet> Wallet { get; set; }
    }
}
