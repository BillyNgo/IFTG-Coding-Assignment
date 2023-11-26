using System;

namespace SettlementBookingSystem.Application.Bookings.Dtos
{
    public class BookingDto
    {
        public BookingDto(Guid id)
        {
            BookingId = id;
        }
        public Guid BookingId { get; private set; }
    }
}
