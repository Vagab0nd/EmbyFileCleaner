using System.Collections.Generic;

namespace EmbyFileCleaner.Model.Json
{
    public class Config
    {
        public ConnectionInfo ConnectionInfo { get; set; }

        /// <summary>
        /// Select, which emby item types should be included in cleaning. Suppoerted values: Episode, Movie.
        /// </summary>
        public List<ItemType> IncludeItemTypes { get; set; }

        public int RemoveOlderThanDays { get; set; }

        /// <summary>
        /// Names that contain value should be ignored when deleting files
        /// </summary>
        public List<string> IgnoreListContains { get; set; }

        /// <summary>
        /// Names that should be ignored when deleting files
        /// </summary>
        public List<string> IgnoreListEquals { get; set; }

        /// <summary>
        /// If true run in test mode without clearing anything
        /// </summary>
        public bool IsTest { get; set; }

        /// <summary>
        /// If true also ignored items will be logged
        /// </summary>
        public bool PrintIgnored { get; set; }
    }
}
