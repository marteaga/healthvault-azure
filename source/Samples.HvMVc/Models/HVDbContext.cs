using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Samples.HvMvc.Models
{
    public class HVDbContext : DbContext
    {
        public HVDbContext()
            : base("DefaultConnection")
        {
        }


        /// <summary>
        /// Journal Entries in the system
        /// </summary>
        public DbSet<JournalEntry> JournalEntries { get; set; }
    }
}