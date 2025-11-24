namespace FinanceTracker.Api.BalanceManagement.Helpers
{
    public static class BalanceHelper
    {
        public static decimal ApplyCredit(decimal currentBalance, decimal amount)
        {
            return currentBalance + amount;
        }

        public static decimal ApplyDebit(decimal currentBalance, decimal amount)
        {
            if (amount > currentBalance)
                throw new InvalidOperationException("Insufficient balance.");

            return currentBalance - amount;
        }
    }
}
