using Microsoft.EntityFrameworkCore;
using MatMob.Data;
using MatMob.Models.Entities;

namespace AuditServiceTests
{
    public class TestApplicationDbContext : ApplicationDbContext
    {
        public TestApplicationDbContext() : base(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDatabase")
            .Options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}