using Newtonsoft.Json.Linq;
using System.Net;

namespace ExchangeSkins
{
    public class InventoryManager
    {

        private readonly HttpClient _httpClient;
        private readonly CookieContainer _cookieContainer;
        private readonly string _steamId;

        private readonly Logger logger = Logger.Instance;

        public InventoryManager(string steamId, string accessToken)
        {

            _steamId = steamId;

            _cookieContainer = new CookieContainer();


            var targetUri = new Uri("https://steamcommunity.com");
            string sessionId = GenSessionId();
            string loginSecureValue = $"{steamId}%7C%7C{accessToken}";

            _cookieContainer.Add(targetUri, new Cookie("steamLoginSecure", loginSecureValue)
            {
                HttpOnly = true,
                Secure = true,
                Domain = "steamcommunity.com"
            });
            _cookieContainer.Add(targetUri, new Cookie("sessionid", sessionId)
            {
                Domain = "steamcommunity.com"
            });

            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = _cookieContainer,
                UseProxy = false,
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.All
            };

            _httpClient = new HttpClient(handler);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Clear();

            logger.Debug($"InventoryManager created for SteamID: {steamId}");
        }

        public async Task<List<InventoryItem>> GetInventory(uint appId = 730, uint contextId = 2)
        {

            var items = new List<InventoryItem>();

            string url = $"https://steamcommunity.com/inventory/{_steamId}/{appId}/{contextId}?l=english&count=1000&preserve_bbcode=1&raw_asset_properties=1";

            logger.Info($"Fetching inventory: appId={appId}, contextId={contextId}");

            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    AddExactHeaders(request, $"https://steamcommunity.com/profiles/{_steamId}/inventory");

                    var response = await _httpClient.SendAsync(request);


                    if (!response.IsSuccessStatusCode)
                    {
                        var errorBody = await response.Content.ReadAsStringAsync();
                        logger.Error($"[GetInventory] HTTP error {response.StatusCode}: {errorBody[..Math.Min(errorBody.Length, 500)]}");
                        Console.WriteLine($"[GetInventory] ERROR body: {errorBody[..Math.Min(errorBody.Length, 500)]}");
                        return items;
                    }

                    var jsonContent = await response.Content.ReadAsStringAsync();

                    var json = JObject.Parse(jsonContent);

                    var successValue = json["success"];

                    if (json["success"]?.Value<int>() != 1)
                    {
                        logger.Warning("[GetInventory] API returned success != 1");
                        return items;
                    }

                    var assets = json["assets"] as JArray;
                    var descriptions = json["descriptions"] as JArray;


                    if (assets == null || descriptions == null)
                    {
                        logger.Warning("[GetInventory] No items in inventory");
                        Console.WriteLine("[GetInventory] No items in inventory");
                        return items;
                    }


                    var descDict = new Dictionary<string, JToken>();
                    foreach (var desc in descriptions)
                    {
                        string key = $"{desc["classid"]}_{desc["instanceid"]}";
                        descDict[key] = desc;
                    }


                    int matched = 0, unmatched = 0;
                    foreach (var asset in assets)
                    {
                        string classId = asset["classid"]?.ToString();
                        string instanceId = asset["instanceid"]?.ToString();
                        string key = $"{classId}_{instanceId}";

                        if (descDict.TryGetValue(key, out JToken description))
                        {
                            matched++;

                            string rarity = null;
                            var tags = description["tags"] as JArray;
                            if (tags != null && tags.Count > 4)
                            {
                                rarity = tags[4]["internal_name"]?.ToString();
                            }

                            string color = null;
                            var ownerDescs = description["owner_descriptions"] as JArray;
                            if (ownerDescs != null && ownerDescs.Count > 1)
                            {
                                color = ownerDescs[1]["color"]?.ToString();
                            }

                            var item = new InventoryItem
                            {
                                AssetId = asset["assetid"]?.ToString(),
                                MarketName = description["market_name"]?.ToString(),
                                MarketHashName = description["market_hash_name"]?.ToString(),
                                Rarity = rarity,
                                Owner_descriptions_color = color
                            };

                            items.Add(item);
                        }
                        else
                        {
                            unmatched++;
                            logger.Trace($"[GetInventory] No description for key={key}");
                            Console.WriteLine($"[GetInventory] WARN: No description for key={key}");
                        }
                    }

                    logger.Info($"[GetInventory] Parsed {matched} items, {unmatched} unmatched");
                    return items;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"[GetInventory] Unexpected error: {ex.GetType().Name}", ex);
                Console.WriteLine($"[GetInventory] {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"[GetInventory] Stack: {ex.StackTrace}");
                return items;
            }
        }

      

        private void AddExactHeaders(HttpRequestMessage req, string referer)
        {
            req.Headers.TryAddWithoutValidation("Accept", "*/*");
            req.Headers.TryAddWithoutValidation("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            req.Headers.TryAddWithoutValidation("Connection", "keep-alive");
            req.Headers.TryAddWithoutValidation("Referer", referer);
            req.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Google Chrome\";v=\"145\", \"Chromium\";v=\"144\", \"Not A(Brand\";v=\"24\"");
            req.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            req.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
            req.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            req.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            req.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
            req.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/145.0.0.0 Safari/537.36");
            req.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
        }

        private string GenSessionId()
        {
            var r = new Random();
            return string.Concat(Enumerable.Range(0, 24).Select(_ => "0123456789abcdef"[r.Next(16)]));
        }

        

        
    }

    public class InventoryItem
    {
        public string AssetId { get; set; }
        public string MarketName { get; set; }
        public string MarketHashName { get; set; }
        public string Owner_descriptions_color { get; set; } = null;

        public string Rarity { get; set; }
    }
}