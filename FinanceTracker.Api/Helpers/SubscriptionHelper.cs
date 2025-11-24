using System;

namespace FinanceTracker.Api.Helpers
{
    using Models;

    public static class SubscriptionHelper
    {
        /// <summary>
        /// Validate frequency and amount for a subscription.
        /// </summary>
        public static (bool IsValid, string? Error) Validate(decimal amount, Frequency frequency)
        {
            if (amount <= 0)
                return (false, "Amount must be greater than 0.");

            if (!Enum.IsDefined(typeof(Frequency), frequency))
                return (false, "Frequency is invalid.");

            return (true, null);
        }

        /// <summary>
        /// Calculate the next payment date (UTC) given a frequency and current date.
        /// If nextPayment is null, assumes from now.
        /// </summary>
        public static DateTime CalculateNext(DateTime fromUtc, Frequency frequency)
        {
            return frequency switch
            {
                Frequency.Daily => fromUtc.AddDays(1),
                Frequency.Weekly => fromUtc.AddDays(7),
                Frequency.BiWeekly => fromUtc.AddDays(14),
                Frequency.Monthly => fromUtc.AddMonths(1),
                Frequency.Quarterly => fromUtc.AddMonths(3),
                Frequency.SemiAnnually => fromUtc.AddMonths(6),
                Frequency.Annually => fromUtc.AddYears(1),
                _ => throw new ArgumentOutOfRangeException(nameof(frequency), "Unsupported frequency")
            };
        }
    }
}
