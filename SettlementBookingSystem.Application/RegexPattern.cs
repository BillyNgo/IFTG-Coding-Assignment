using System;
using System.Text.RegularExpressions;

namespace SettlementBookingSystem.Application
{
    public static class RegexPatterns
    {
        /// <summary>
        /// Regular expression pattern to validate time span
        /// </summary>
        public static readonly Regex TimeRegex = new(@"[0-9]{1,2}:[0-9][0-9]");
    }
}
