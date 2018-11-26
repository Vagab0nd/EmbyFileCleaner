namespace EmbyFileCleaner.Model.Json
{
    public class ConnectionInfo
    {
        /// <summary>
        /// Emby server does not support deleting library items using ApiKey :(
        /// </summary>
        public string ApiKey { get; set; }

        public string Endpoint { get; set; }

        /// <summary>
        /// Username of account, for which cleaning will be performed
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password of account, for which cleaning will be performed
        /// </summary>
        public string Password { get; set; }
    }
}
