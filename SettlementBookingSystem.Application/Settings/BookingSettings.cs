
namespace SettlementBookingSystem.Application.Settings
{
    public sealed class BookingSettings
    {
        public static string Section => nameof(BookingSettings);

        public WorkingHour WorkingHour { get; set; }
        public int BookingDurationInHour { get; set; }
        public int MaxOfOverlapBookings { get; set; }
    }

    public class WorkingHour
    {
        public string From { get; set; }
        public string To { get; set; }
    }
}
