using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Options;
using SettlementBookingSystem.Application.Bookings.Dtos;
using SettlementBookingSystem.Application.Exceptions;
using SettlementBookingSystem.Application.Settings;
using SettlementBookingSystem.Domain.Entities.BookingAggregate;
using SettlementBookingSystem.Infrastructure;

namespace SettlementBookingSystem.Application.Bookings.Commands
{
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BookingDto>
    {
        private readonly BookingSettings _bookingSettings;
        private readonly BookingDbContext _bookingDbContext;

        public CreateBookingCommandHandler(BookingDbContext dbContext, IOptionsMonitor<BookingSettings> settings)
        {
            _bookingDbContext = dbContext;
            _bookingSettings = settings.CurrentValue;
        }

        public async Task<BookingDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            if (IsReserved(request.BookingTime)) throw new ConflictException("Booking are reserved.");
            if (IsExceedMaxOfOverlapBookings(request.BookingTime)) throw new ConflictException("Maximum overlap booking exceeded.");
            if (IsOutOfWorkingTime(request.BookingTime))
                throw new ValidationException(new List<ValidationFailure>
                {
                    new(nameof(CreateBookingCommand.BookingTime), "Booking is out of working time.")
                });

            var booking = new Booking(request.Name, request.BookingTime);

            _bookingDbContext.Bookings.Add(booking);
            await _bookingDbContext.SaveChangesAsync(cancellationToken);

            return new BookingDto(booking.Id);
        }

        private bool IsOutOfWorkingTime(string bookingTime)
        {
            var fromWorkingHour = TimeSpan.Parse(_bookingSettings.WorkingHour.From);
            var lastWorkingHour = TimeSpan.Parse(_bookingSettings.WorkingHour.To).Subtract(TimeSpan.FromHours(_bookingSettings.BookingDurationInHour));
            var bookingHour = TimeSpan.Parse(bookingTime);
            return !(bookingHour >= fromWorkingHour && bookingHour <= lastWorkingHour);
        }

        private bool IsReserved(string bookingTime)
        {
            var bookingHour = TimeSpan.Parse(bookingTime);
            return _bookingDbContext.Bookings.Select(b => TimeSpan.Parse(b.BookingTime)).Any(bt => bookingHour == bt);
        }

        private bool IsExceedMaxOfOverlapBookings(string bookingTime)
        {
            var bookingHour = TimeSpan.Parse(bookingTime);
            var fromHour = new TimeSpan(bookingHour.Hours, 0, 0);
            var toHour = new TimeSpan(bookingHour.Hours, 59, 0);

            var numberOfOverlapBookings = _bookingDbContext.Bookings.Select(b => TimeSpan.Parse(b.BookingTime))
                .Count(bt => fromHour <= bt && toHour >= bt);

            return numberOfOverlapBookings >= _bookingSettings.MaxOfOverlapBookings;
        }
    }
}