using Newtonsoft.Json;
using SteamKit2;
using SteamKit2.Authentication;
using SteamKit2.GC;
using SteamKit2.Internal;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;

namespace ExchangeSkins
{
    public class Client
    {
        private readonly SteamClient _client;
        private readonly SteamUser _user;
        private readonly SteamGameCoordinator _gameCoordinator;
        private readonly SteamFriends _friends;
        private readonly CallbackManager _callbackManager;
        private readonly string _userName;
        private readonly string _password;
        private readonly System.Timers.Timer _helloTimer;
        private readonly Logger _logger = Logger.Instance;
        private SteamAuth.SteamGuardAccount MafData;

        private bool _workDone;
        private string _accessToken;
        private string _refreshToken;
        private ulong _steamId64;
        private InventoryManager _inventoryManager;
        private TaskCompletionSource<bool> _exchangeConfirmation;
        private List<InventoryItem> _items;

        private const int AppId = 730;
        private const int DefaultTimeoutMs = 15000;
        private readonly string _craftType = "Common";

        public Client(string userName, string password, string[] files, string craftType = "Common")
        {
            _userName = userName;
            _password = password;
            _client = new SteamClient();
            _craftType = craftType;
            _logger.Info($"Initializing client for account: {userName}");

            _user = _client.GetHandler<SteamUser>();
            _gameCoordinator = _client.GetHandler<SteamGameCoordinator>();
            _friends = _client.GetHandler<SteamFriends>();

            _callbackManager = new CallbackManager(_client);

            _callbackManager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            _callbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
            _callbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            _callbackManager.Subscribe<SteamGameCoordinator.MessageCallback>(OnGameCoordinatorMessage);



            
            string foundFile = null;
            foreach (var file in files)
            {
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
                if (string.Equals(fileNameWithoutExt, userName, StringComparison.OrdinalIgnoreCase))
                {
                    foundFile = file;
                    break;
                }
            }
            if (foundFile == null)
            {
                Console.WriteLine($"Error: .mafile for account {userName} not found.");
                return;
            }
            MafData = JsonConvert.DeserializeObject<SteamAuth.SteamGuardAccount>(File.ReadAllText(foundFile));
            

            _helloTimer = new System.Timers.Timer(2000)
            {
                AutoReset = true
            };
            _helloTimer.Elapsed += OnHelloTimerElapsed;

            _logger.Debug("Client initialized");
        }

        public void Connect()
        {
            _logger.Info("Connecting to Steam...");
            Console.WriteLine("Connecting to Steam...");
            _client.Connect();
        }

        public void Wait()
        {
            _logger.Debug("Entering callback wait loop...");
            while (!_workDone)
            {
                _callbackManager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
            _logger.Debug("Exited callback wait loop.");
        }

        private void OnGameCoordinatorMessage(SteamGameCoordinator.MessageCallback callback)
        {
            _logger.Trace($"[IN] MessageID: {callback.EMsg}");
            Console.WriteLine($"[IN] MessageID: {callback.EMsg}");

            var messageMap = new Dictionary<uint, Action<IPacketGCMsg>>
            {
                { (uint)SteamKit2.GC.CSGO.Internal.EGCBaseClientMsg.k_EMsgGCClientWelcome, OnClientWelcome },
                { (uint)SteamKit2.GC.CSGO.Internal.EGCItemMsg.k_EMsgGCCraftResponse, OnCraftResponse }
            };

            if (messageMap.TryGetValue(callback.EMsg, out var handler))
            {
                handler(callback.Message);
            }
            else
            {
                _logger.Warning($"[UNHANDLED] MessageID: {callback.EMsg}");
                Console.WriteLine($"[UNHANDLED] MessageID: {callback.EMsg}");
            }
        }

        private async void OnConnected(SteamClient.ConnectedCallback callback)
        {
            try
            {
                
                _logger.Info("Connected to Steam. Logging in...");
                Console.WriteLine("Connected to Steam. Logging in...");

                var authSession = await _client.Authentication.BeginAuthSessionViaCredentialsAsync(
                    new AuthSessionDetails
                    {
                        Username = _userName,
                        Password = _password,
                        IsPersistentSession = true,
                        Authenticator = new UserAuthenticator(MafData.SharedSecret)

                        //Authenticator = new UserConsoleAuthenticator()
                    }
                );

                var pollResponse = await authSession.PollingWaitForResultAsync();

                _accessToken = pollResponse.AccessToken;
                _refreshToken = pollResponse.RefreshToken;

                _logger.Info("Authentication tokens received");

                Thread.Sleep(3000);
                _user.LogOn(new SteamUser.LogOnDetails
                {
                    Username = _userName,
                    AccessToken = _refreshToken,
                    ShouldRememberPassword = true
                });

                _logger.Debug($"LogOn sent for user: {_userName}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Connection/auth error for {_userName}", ex);

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Type: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                Console.ResetColor();

                await Task.Delay(10000);
                _workDone = true;

                if (_client.IsConnected)
                {
                    _client.Disconnect();
                }
            }
        }

        private async void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                _logger.Error($"Login failed: {callback.Result}");
                Console.WriteLine($"Login failed: {callback.Result}");
                _workDone = true;
                return;
            }

            _steamId64 = ParseSteamIdFromToken(_accessToken);

            _logger.Info($"Logged in successfully. SteamID64: {_steamId64}");
            Console.WriteLine("Logged in! Setting Online status...");

            _friends.SetPersonaState(EPersonaState.Online);

            await Task.Delay(5000);

            _inventoryManager = new InventoryManager(_steamId64.ToString(), _accessToken);
            LoadInventoryAsync();

            var playGame = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);
            playGame.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
            {
                game_id = AppId
            });
            _client.Send(playGame);

            _logger.Info("Launched CS2.");
            Console.WriteLine("Launched CS2.");
            _helloTimer.Start();
        }

        private void OnHelloTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!_client.IsConnected)
            {
                _logger.Warning("SendClientHello skipped — client not connected");
                return;
            }

            _logger.Trace("Sending ClientHello");
            Console.WriteLine("Sending ClientHello");

            var clientHello = new ClientGCMsgProtobuf<SteamKit2.GC.CSGO.Internal.CMsgClientHello>(4006);
            clientHello.Body.version = 2000700;
            clientHello.Body.client_session_need = 0;
            clientHello.Body.client_launcher = 0;
            clientHello.Body.steam_launcher = 0;
            _gameCoordinator.Send(clientHello, AppId);
        }

        private async void OnClientWelcome(IPacketGCMsg packetMsg)
        {
            _helloTimer.Stop();

            _logger.Info($"GC Welcome received. Items to craft: {_items?.Count ?? 0}");
            int totalBatches = (_items?.Count ?? 0) / 10;
            _logger.Info($"Craft batches: {totalBatches}");

            if (totalBatches == 0)
            {
                _logger.Info("No items to craft");
                Console.WriteLine("No items to craft");
                _client.Disconnect();
                return;
            }

            for (int i = 0; i < totalBatches; i++)
            {
                try
                {
                    _exchangeConfirmation?.TrySetCanceled();
                    _exchangeConfirmation = new TaskCompletionSource<bool>();

                    var assetIds = new List<ulong>();
                    for (int j = 0; j < 10; j++)
                    {
                        if (ulong.TryParse(_items[0].AssetId, out ulong asset))
                        {
                            assetIds.Add(asset);
                            _items.RemoveAt(0);
                        }
                    }

                    _logger.Debug($"Craft batch {i + 1}: {assetIds.Count} items [{string.Join(", ", assetIds)}]");
                    Thread.Sleep(1500);



                    SendCraftRequest(assetIds, Convert.ToInt32(Blacklist.BluePrint[_craftType]));

                    bool success = await WaitForConfirmationAsync(
                        _exchangeConfirmation,
                        DefaultTimeoutMs,
                        "Exchange"
                    );

                    _logger.Info($"Exchange result: {success}");

                    if (!success)
                    {
                        _logger.Warning("Exchange timeout or failed");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Exchange timeout.");
                        Console.ResetColor();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    _logger.Error(ex.ToString());
                }
            }

            

            _workDone = true;
            _client.Disconnect();
        }



        private void SendCraftRequest(List<ulong> itemIds, int blueprint = 0) // blueprint редкость в цифре от ширпа - 0 до максимально значения, промышленное - 1 и тд
        {
            var craftMsg = new ClientGCMsg<MsgGCCraft>();
            craftMsg.Body.Blueprint = (short)blueprint;
            craftMsg.Body.ItemCount = (ushort)itemIds.Count;
            craftMsg.Body.ItemIds.AddRange(itemIds);

            _gameCoordinator.Send(craftMsg, AppId);
            _logger.Info($"[Craft] Sent request: {itemIds.Count} items, blueprint={blueprint}");
            Console.WriteLine($"[Craft] Sent request: {itemIds.Count} items, blueprint={blueprint}");
        }

        private void OnCraftResponse(IPacketGCMsg packetMsg)
        {
            var rawMsg = packetMsg as PacketClientGCMsg;
            if (rawMsg == null)
            {
                _logger.Error("[Craft] Failed to get raw message from craft response");
                Console.WriteLine("[Craft] Failed to get raw message");
                _exchangeConfirmation?.TrySetResult(false);
                return;
            }

            byte[] payload = rawMsg.GetData();
            _logger.Debug($"[Craft] Response payload size: {payload.Length} bytes");

            const int GcHeaderSize = 18;

            if (payload.Length < GcHeaderSize + 8)
            {
                _logger.Warning($"[Craft] Packet too short: {payload.Length} bytes");
                Console.WriteLine($"[Craft] Packet too short: {payload.Length} bytes");
                _exchangeConfirmation?.TrySetResult(false);
                return;
            }

            ushort headerVersion = BitConverter.ToUInt16(payload, 0);
            ulong jobIdTarget = BitConverter.ToUInt64(payload, 2);
            ulong jobIdSource = BitConverter.ToUInt64(payload, 10);

            _logger.Trace($"[Craft] Header: version={headerVersion}, jobTarget={jobIdTarget}, jobSource={jobIdSource}");

            int offset = GcHeaderSize;
            short responseBlueprint = BitConverter.ToInt16(payload, offset); offset += 2;
            uint unknownField = BitConverter.ToUInt32(payload, offset); offset += 4;
            short idCount = BitConverter.ToInt16(payload, offset); offset += 2;

            _logger.Debug($"[Craft] Blueprint={responseBlueprint}, Unknown={unknownField}, IdCount={idCount}");

            if (idCount == -1)
            {
                _logger.Warning("[Craft] Craft failed (IdCount = -1)");
                Console.WriteLine("[Craft] Craft failed (IdCount = -1)");
                _exchangeConfirmation?.TrySetResult(false);
                return;
            }

            var resultIds = new List<ulong>();
            for (int i = 0; i < idCount && offset + 8 <= payload.Length; i++, offset += 8)
            {
                resultIds.Add(BitConverter.ToUInt64(payload, offset));
            }

            _logger.Info($"[Craft] Successful! Received {resultIds.Count} item(s): [{string.Join(", ", resultIds)}]");
            Console.WriteLine($"[Craft] Successful! Received: {resultIds.Count} item(s)");
            foreach (var id in resultIds)
            {
                Console.WriteLine($"  -> Item ID: {id}");
            }

            _exchangeConfirmation?.TrySetResult(true);
        }

        private void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            _logger.Warning("Disconnected from Steam.");
            Console.WriteLine("Disconnected from Steam.");

            _helloTimer?.Stop();
            _workDone = true;
        }

        private async Task<bool> WaitForConfirmationAsync(
            TaskCompletionSource<bool> confirmation,
            int timeoutMs = DefaultTimeoutMs,
            string operationName = "")
        {
            _logger.Debug($"WaitForConfirmationAsync: {operationName}, timeout: {timeoutMs}ms");

            try
            {
                var completedTask = await Task.WhenAny(
                    confirmation.Task,
                    Task.Delay(timeoutMs)
                );

                if (completedTask == confirmation.Task)
                {
                    var result = await confirmation.Task;
                    _logger.Info($"{operationName} confirmed (result: {result})");
                    Console.WriteLine($"{operationName} confirmed successfully");
                    return result;
                }
                else
                {
                    _logger.Warning($"Timeout waiting for {operationName} ({timeoutMs}ms)");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Timeout waiting for {operationName} ({timeoutMs}ms)");
                    Console.ResetColor();
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in {operationName}", ex);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in {operationName}: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        public static ulong ParseSteamIdFromToken(string accessToken)
        {
            var log = Logger.Instance;
            try
            {
                var parts = accessToken.Split('.');
                if (parts.Length < 2) return 0;

                var payloadSegment = parts[1];

                switch (payloadSegment.Length % 4)
                {
                    case 2: payloadSegment += "=="; break;
                    case 3: payloadSegment += "="; break;
                }

                var bytes = Convert.FromBase64String(payloadSegment);
                var json = Encoding.UTF8.GetString(bytes);

                var match = Regex.Match(json, "\"sub\"\\s*:\\s*\"(\\d+)\"");

                if (match.Success && ulong.TryParse(match.Groups[1].Value, out ulong id))
                {
                    log.Debug($"Parsed SteamID64 from token: {id}");
                    return id;
                }

                log.Warning("Failed to parse SteamID64 from access token");
            }
            catch (Exception ex)
            {
                log.Error("Exception while parsing SteamID from token", ex);
            }
            return 0;
        }

        private async void LoadInventoryAsync()
        {
            _logger.SubSection("LOADING INVENTORY");

            try
            {
                var loadedItems = await _inventoryManager.GetInventory(AppId, 2);
                _logger.Info($"Inventory loaded: {loadedItems.Count} total items");

                var weaponItems = Blacklist.ClearNoWeapon(loadedItems);
                _logger.Info($"After weapon filter: {weaponItems.Count} items");

                _items = Blacklist.ClearRarity(weaponItems, _craftType);
                _logger.Info($"After rarity filter ({_craftType}): {_items.Count} items ready for craft");
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load inventory", ex);
                Console.WriteLine($"{ex.Message}");
            }
        }
    }

    

   
}