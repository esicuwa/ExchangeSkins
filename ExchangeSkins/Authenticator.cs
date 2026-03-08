using SteamAuth;
using SteamKit2.Authentication;


namespace ExchangeSkins
{
    public class UserAuthenticator : IAuthenticator
    {
        private readonly Logger logger = Logger.Instance;

        public UserAuthenticator(string SharedSecret_)
        {
            _SharedSecret = SharedSecret_;
            logger.Debug("UserAuthenticator created");
        }

        public string _SharedSecret { get; set; }


        /// <inheritdoc />
        public Task<string> GetDeviceCodeAsync(bool previousCodeWasIncorrect)
        {
            if (previousCodeWasIncorrect)
            {
                logger.Warning("Previous Steam Guard code was incorrect, generating new one");
            }

            var account = new SteamGuardAccount { SharedSecret = _SharedSecret };
            var loginCode = account.GenerateSteamGuardCode();

            logger.Info("Steam Guard code generated successfully");
            return Task.FromResult(loginCode);
        }

        /// <inheritdoc />
        public async Task<string> GetEmailCodeAsync(string email, bool previousCodeWasIncorrect)
        {
            logger.Debug($"Email code requested for: {email}");
            return await Task.FromResult("false");
        }

        /// <inheritdoc />
        public Task<bool> AcceptDeviceConfirmationAsync()
        {
            logger.Debug("Device confirmation requested — returning false");
            return Task.FromResult(false);
        }
    }

}