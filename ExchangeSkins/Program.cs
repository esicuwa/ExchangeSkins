using Newtonsoft.Json.Linq;


namespace ExchangeSkins
{
    internal class Program
    {
        private static readonly string ConfigFilePath = Path.Combine(Tools.GetExecutableDir(), "config.json");

        static void Main(string[] args)
        {
            Logger.Initialize(consoleOutput: true, minLogLevel: LogLevel.TRACE);
            var logger = Logger.Instance;

            logger.Section("APPLICATION START");
            logger.Info("Loading configuration...");

            string jsonContent = File.ReadAllText(ConfigFilePath);
            JObject config = JObject.Parse(jsonContent, new JsonLoadSettings
            {
                CommentHandling = CommentHandling.Ignore
            });

            var Rarity = config["Rarity"]!.ToString();

            if (Blacklist.Rares[Rarity] is null)
            {
                Console.WriteLine("Error: invalid Rarity format in config.json");
                logger.Error("Error: invalid Rarity format in config.json");
                return;
            }

            var folderPath = config["MaFile_Path"]!.ToString();
            string[] maFiles = Directory.GetFiles(folderPath, "*.mafile");
            logger.Debug($"Found {maFiles.Length} .mafile(s) in '{folderPath}'");

            List<string> DataAccounts = File.ReadAllLines(config["Accounts_Path"].ToString()).ToList();

            foreach (var dataAccount in DataAccounts)
            {

                string[] accountParts = dataAccount.ToString().Split(':');
                if (accountParts.Length < 2)
                {
                    logger.Error("Invalid account format. Expected 'login:password'");
                    Console.WriteLine("Error: invalid Account format");
                    continue;
                }

                string login = accountParts[0];
                string password = accountParts[1];

                logger.Info("Creating client and connecting...");
                try
                {
                    var bot = new Client(login, password, maFiles, Rarity);
                    bot.Connect();
                    bot.Wait();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    Console.WriteLine(ex.ToString());
                }
                Console.WriteLine("----------- END ACCOUNT -----------");
                logger.Warning("----------- END ACCOUNT -----------");
                Console.WriteLine("KD 30 SECONDS");

                Thread.Sleep(30000);
            }
            Console.WriteLine("Application finished.");
            logger.Info("Application finished.");
            Console.ReadLine();
        }
    }
}