namespace EmbyFileCleaner
{
    using Emby.ApiClient;
    using Emby.ApiClient.Cryptography;
    using Emby.ApiClient.Model;
    using Model.Json;
    using MediaBrowser.Model.Dto;
    using MediaBrowser.Model.Entities;
    using MediaBrowser.Model.Logging;
    using MediaBrowser.Model.Querying;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public class Cleaner
    {
        private readonly Config config;
        private readonly ApiClient apiClient;
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public Cleaner(Config config)
        {
            this.config = config;
            this.apiClient = this.GetApiClientInstance(this.config);
        }

        public void Run()
        {
            this.RunAsync().GetAwaiter().GetResult();
        }

        private async Task RunAsync()
        {
            var userId = await this.GetUserIdByUsername(this.config.ConnectionInfo.Username);

            foreach(var item in (await this.GetItems(userId))
                .Where(item => {
                    var lastPlayedDate = item.UserData?.LastPlayedDate;
                    return lastPlayedDate != null && lastPlayedDate < DateTime.Now.AddDays(-this.config.RemoveOlderThanDays) && this.IsNotIgnored(item);
                }))
            {
                if(this.config.IsTest)
                {
                    Logger.Info($"Picked - {this.GetItemNameFormattedByType(item)}");
                }
                else if(this.TryDelete(item))
                {
                    Logger.Info($"Deleted - {this.GetItemNameFormattedByType(item)}");
                }
            }
        }

        private bool TryDelete(BaseItemDto item)
        {
            try
            {
                if(item.CanDelete ?? true)
                {
                    this.apiClient.DeleteItemAsync(item.Id);
                    return true;
                }

                throw new InvalidOperationException("Item marked not to be deleted.");
            }
            catch(Exception e)
            {
                Logger.Error($"Could not delete {this.GetItemNameFormattedByType(item)}: {e.Message}");
                return false;
            }
        }

        private string GetItemNameByType(BaseItemDto item)
        {
            if(Enum.TryParse(item.Type, out ItemType itemType))
            {
                switch(itemType)
                {
                    case ItemType.Episode:
                        return item.SeriesName;
                    case ItemType.Movie:
                        return item.Name;
                    default:
                        throw new ArgumentOutOfRangeException(item.Type);
                }
            }

            throw new InvalidOperationException();
        }

        private string GetItemNameFormattedByType(BaseItemDto item)
        {
            if (Enum.TryParse(item.Type, out ItemType itemType))
            {
                switch (itemType)
                {
                    case ItemType.Episode:
                        return $"{item.SeriesName} - {item.Name}";
                    case ItemType.Movie:
                        return $"{item.Name} - {item.ProductionYear}";
                    default:
                        throw new ArgumentOutOfRangeException(item.Type);
                }
            }

            throw new InvalidOperationException();
        }

        private bool IsNotIgnored(BaseItemDto item)
        {
            var isIgnoredListContains = this.config.IgnoreListContains.Any(name => this.GetItemNameByType(item).ToLower().Contains(name.ToLower()));
            var isIgnoredEquals = this.config.IgnoreListEquals.Any(name => this.GetItemNameByType(item).ToLower() == name.ToLower());
            var ignored = isIgnoredListContains || isIgnoredEquals;

            if(ignored)
            {
                Logger.Info($"Ignored - {this.GetItemNameFormattedByType(item)}");
            }

            return ignored == false;
        }

        private async Task<string> GetUserIdByUsername(string username)
        {
            var users = await this.apiClient.GetUsersAsync(new UserQuery
            {
                IsDisabled = false,
                IsHidden = false
            });

            return users.SingleOrDefault(u => u.Name.ToLower() == username.ToLower())?.Id;
        }

        private async Task<BaseItemDto[]> GetItems(string userId)
        {
            var items = await this.apiClient.GetItemsAsync(new ItemQuery
            {
                SortBy = new[] { ItemSortBy.DatePlayed },
                SortOrder = SortOrder.Ascending,
                IncludeItemTypes = this.config.IncludeItemTypes.Select(t => t.ToString()).ToArray(),
                IsPlayed = true,
                Recursive = true,
                UserId = userId,
                Fields = new ItemFields[] { }
            });

            return items.Items;
        }

        private ApiClient GetApiClientInstance(Config configLocal)
        {
            var logger = new NullLogger();
            var cryptoProvider = new CryptographyProvider();
            var client = new ApiClient(logger, configLocal.ConnectionInfo.Endpoint, configLocal.ConnectionInfo.ApiKey, cryptoProvider);
            return client;
        }
    }
}
