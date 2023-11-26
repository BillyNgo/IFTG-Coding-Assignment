using System;

namespace SettlementBookingSystem.Domain.Entities.BookingAggregate
{
    public class Booking : IAggregateRoot
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string BookingTime { get; private set; }
		

        public Booking(string name, string bookingTime)
        {
            Name = !string.IsNullOrEmpty(name) ? name : throw new ArgumentNullException(nameof(name));
            BookingTime = !string.IsNullOrEmpty(bookingTime) ? bookingTime : throw new ArgumentNullException(nameof(bookingTime));
            Id = Guid.NewGuid();
        }
    }
}
