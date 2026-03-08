using Newtonsoft.Json.Linq;

namespace ExchangeSkins
{
    public class Blacklist
    {
        private static readonly Logger logger = Logger.Instance;

        public static JObject Rares = new JObject
{
    { "Common", "Rarity_Common_Weapon" },
    { "Uncommon", "Rarity_Uncommon_Weapon" },
    { "Rare", "Rarity_Rare_Weapon" },
    { "Mythical", "Rarity_Mythical_Weapon" }
};
        public static JObject BluePrint = new JObject
{
    { "Common", 0 },
    { "Uncommon", 1 },
    { "Rare", 2 },
    { "Mythical", 3 }
};


        public static List<InventoryItem> ClearNoWeapon(List<InventoryItem> items)
        {
            List<InventoryItem> Finalize_items = new List<InventoryItem>();

            logger.Debug($"ClearNoWeapon: filtering {items.Count} items");

            foreach (InventoryItem item in items)
            {



                if (CheckExchange(item))
                {
                    Finalize_items.Add(item);
                }

            }

            logger.Info($"ClearNoWeapon: {items.Count} -> {Finalize_items.Count} items after weapon filter");
            return Finalize_items;
        }

        public static List<InventoryItem> ClearRarity(List<InventoryItem> items, string _rarity = "Common")
        {
            List<InventoryItem> Finalize_items = new List<InventoryItem>();

            logger.Debug($"ClearRarity: filtering {items.Count} items by rarity '{_rarity}'");

            foreach (InventoryItem item in items)
            {

                if (item.Rarity == Rares[_rarity]?.ToString())
                {
                    Finalize_items.Add(item);
                }
            }

            logger.Info($"ClearRarity: {items.Count} -> {Finalize_items.Count} items with rarity '{_rarity}'");
            return Finalize_items;
        }

        private static bool CheckExchange(InventoryItem item)
        {

            if (string.IsNullOrEmpty(item.MarketHashName) || string.IsNullOrEmpty(item.Rarity))
            {
                logger.Trace($"CheckExchange: skipping item (empty name or rarity) AssetId={item.AssetId}");
                return false;
            }


            var excludedWeaponTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Sticker", "Charm", "Capsule",
                    "Agent", "Graffiti", "Case", "Patch", "Music Kit",
                    "Graffiti Box", "StatTrak", "Unknown",
                    "Pin", "Package", "Knife", "Souvenir", "Gloves",
                    "Bayonet", "Karambit", "Medal", "Coin", "Badge"
                };
            var excludedPrefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Sealed Graffiti", "Music Kit", "Charm Detachment Pack"
                };
            try
            {
                if (item.Owner_descriptions_color == "e4ae39")
                {
                    logger.Trace($"CheckExchange: excluded (trade locked) '{item.MarketHashName}'");
                    return false;
                }
                else if (excludedPrefixes.Any(prefix => item.MarketHashName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                {
                    logger.Trace($"CheckExchange: excluded (prefix match) '{item.MarketHashName}'");
                    return false;
                }
                else if (excludedWeaponTypes.Any(excluded => item.MarketHashName.Contains(excluded)))
                {
                    logger.Trace($"CheckExchange: excluded (type match) '{item.MarketHashName}'");
                    return false;
                }
                else if (item.Rarity.Contains("Weapon", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    logger.Trace($"CheckExchange: excluded (not weapon rarity) '{item.MarketHashName}' rarity={item.Rarity}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"CheckExchange: error processing '{item.MarketHashName}'", ex);
                Console.WriteLine(ex.Message);
                return false;
            }



        }




    }
}