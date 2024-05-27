using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImagesUploadViewer.Models;
using Microsoft.EntityFrameworkCore;

namespace ImagesUploadViewer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Picture> Pictures { get; set; }
    }
}
