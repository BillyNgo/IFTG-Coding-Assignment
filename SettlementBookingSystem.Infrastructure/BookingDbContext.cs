using Microsoft.EntityFrameworkCore;
using SettlementBookingSystem.Domain.Entities.BookingAggregate;

namespace SettlementBookingSystem.Infrastructure
{
    public class BookingDbContext : DbContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options)
            : base(options)
        {
        }

        public DbSet<Booking> Bookings { get; set; }

    }
}