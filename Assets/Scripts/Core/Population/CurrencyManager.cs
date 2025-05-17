using Glade.Core.Trading.Data;

namespace Glade.Core.Population
{
    /// <summary>Central rules for which currency a population uses.</summary>
    public static class CurrencyManager
    {
        private static Resource _defaultCurrency;

        public static void SetDefault(Resource res) => _defaultCurrency = res;
        public static Resource Default => _defaultCurrency;

        public static Resource CurrencyFor(PopulationType type)
        {
            // Extend later (vampires=blood, monkeys=banana, etc.)
            return _defaultCurrency;
        }
    }
}
