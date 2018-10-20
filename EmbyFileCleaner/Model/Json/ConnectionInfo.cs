namespace EmbyFileCleaner.Model.Json
{
    public class ConnectionInfo
    {
        public string ApiKey { get; set; }

        public string Endpoint { get; set; }

        /// <summary>
        /// Username of account, for which cleaning will be performed
        /// </summary>
        public string Username { get; set; }
    }
}
