using Emby.ApiClient;
using Emby.ApiClient.Cryptography;
using Emby.ApiClient.Model;
using EmbyFileCleaner.Model.Json;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EmbyFileCleaner
{
    public class Cleaner
    {
        private readonly Config config;
        private readonly ApiClient apiClient;

        public Cleaner(Config config)
        {
            this.config = config;
            this.apiClient = this.GetApiClientInstance(this.config);
        }

        public void Run()
        {
            var task = this.RunAsync();
            try
            {
                task.Wait();
            }
            catch(Exception)
            {
                throw task.Exception;
            }
        }

        private async Task RunAsync()
        {
            var userId = await this.GetUserIdByUsername(this.config.ConnectionInfo.Username);

            foreach(var item in (await this.GetItems(userId))
                .Where(item => item.UserData?.LastPlayedDate < DateTime.Now.AddDays(- this.config.RemoveOlderThanDays) &&
                this.IsNotIgnored(item)))
            {
                if(this.config.IsTest)
                {
                    Console.WriteLine(this.GetItemNameFormattedByType(item));
                }
                else
                {
                    if (this.TryDelete(item))
                    {
                        Console.WriteLine($"Successfully deleted {this.GetItemNameFormattedByType(item)}.");
                    }
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
                Console.WriteLine($"Could not delete {this.GetItemNameFormattedByType(item)}.");
                Console.Write(e.Message);
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
            var isIgnoredListContains = this.config.IgnoreListContains.Any(name => name.ToLower().Contains(this.GetItemNameByType(item).ToLower()));
            var isIgnoredEquals = this.config.IgnoreListContains.Any(name => name == this.GetItemNameByType(item).ToLower());
            return (isIgnoredListContains || isIgnoredEquals) == false;
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

        private ApiClient GetApiClientInstance(Config config)
        {
            var logger = new NullLogger();
            var cryptoProvider = new CryptographyProvider();
            var client = new ApiClient(logger, config.ConnectionInfo.Endpoint, config.ConnectionInfo.ApiKey, cryptoProvider);
            return client;
        }
    }
}
