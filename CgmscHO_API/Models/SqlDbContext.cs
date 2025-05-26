using CgmscHO_API.AttendenceDTO;
using CgmscHO_API.FacilityDTO;
using CgmscHO_API.HodDTO;
using CgmscHO_API.WarehouseDTO;
using Microsoft.EntityFrameworkCore;

namespace CgmscHO_API.Models
{
    public class SqlDbContext: DbContext
    {
        public SqlDbContext(DbContextOptions<SqlDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<GetLocationDTO> GetLocationDbSet { get; set; }
        public DbSet<EmdDetailDTO> EmdDetailDbSet { get; set; }
        public DbSet<AttendenceRecordDTO> AttendenceRecordDbSet { get; set; }
        public DbSet<GetDesignationDTO> GetDesignationDbSet { get; set; }
        




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AttendenceRecordDTO>().HasNoKey();

        }
    }
}
