using FluentAssertions;
using FluentValidation;
using SettlementBookingSystem.Application.Bookings.Commands;
using SettlementBookingSystem.Application.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using SettlementBookingSystem.Application.Settings;
using SettlementBookingSystem.Infrastructure;
using Xunit;

namespace SettlementBookingSystem.Application.UnitTests
{
    public class CreateBookingCommandHandlerTests
    {
        private readonly BookingDbContext _bookingDbContext;
        private readonly IOptionsMonitor<BookingSettings> _optionsMonitor;

        public CreateBookingCommandHandlerTests()
        {
            var bookingDbOptions = new DbContextOptionsBuilder<BookingDbContext>().UseInMemoryDatabase("BookingTest").Options;
            _bookingDbContext = new BookingDbContext(bookingDbOptions);
            var bookingSetting = new BookingSettings()
            {
                WorkingHour = new WorkingHour { From = "09:00", To = "16:00" },
                BookingDurationInHour = 1,
                MaxOfOverlapBookings = 4
            };
            _optionsMonitor = Mock.Of<IOptionsMonitor<BookingSettings>>(_ => _.CurrentValue == bookingSetting);
        }

        [Fact]
        public async Task GivenValidBookingTime_WhenNoConflictingBookings_ThenBookingIsAccepted()
        {
            var command = new CreateBookingCommand
            {
                Name = "test",
                BookingTime = "09:15",
            };

            var handler = new CreateBookingCommandHandler(_bookingDbContext, _optionsMonitor);

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.BookingId.Should().NotBeEmpty();
        }

        [Fact]
        public void GivenOutOfHoursBookingTime_WhenBooking_ThenValidationFails()
        {
            var command = new CreateBookingCommand
            {
                Name = "test",
                BookingTime = "00:00",
            };

            var handler = new CreateBookingCommandHandler(_bookingDbContext, _optionsMonitor);

            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            act.Should().Throw<ValidationException>();
        }

        [Fact]
        public void GivenValidBookingTime_WhenTheBookingIsFull_ThenConflictThrown()
        {
            var command = new CreateBookingCommand
            {
                Name = "test",
                BookingTime = "09:15",
            };

            var handler = new CreateBookingCommandHandler(_bookingDbContext, _optionsMonitor);

            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            act.Should().Throw<ConflictException>();
        }
    }
}
