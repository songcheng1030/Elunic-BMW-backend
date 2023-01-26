using System;
using System.Collections.Generic;

namespace AIQXCoreService.Implementation.Persistence
{
    public sealed class KpiCache
    {
        private static readonly Lazy<KpiCache> lazy =
            new Lazy<KpiCache>(() => new KpiCache());

        public static KpiCache Instance { get { return lazy.Value; } }

        public Dictionary<string, Dictionary<string, int>> Plants = new Dictionary<string, Dictionary<string, int>>();
    }
}