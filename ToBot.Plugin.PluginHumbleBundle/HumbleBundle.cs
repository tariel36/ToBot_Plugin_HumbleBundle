// The MIT License (MIT)
//
// Copyright (c) 2022 tariel36
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToBot.Common.Maintenance;
using ToBot.Common.Maintenance.Logging;
using ToBot.Common.Pocos;
using ToBot.Common.Watchers;
using ToBot.Communication.Messaging.Formatters;
using ToBot.Communication.Messaging.Providers;
using ToBot.Data.Repositories;
using ToBot.Plugin.GenericScrapperPlugin;
using ToBot.Plugin.PluginHumbleBundle.Data;

namespace ToBot.Plugin.PluginHumbleBundle
{
    public class HumbleBundle
        : PluginGenericScrapper<HumbleBundleEntry>
    {
        public const string Prefix = "hb";

        private const string BaseProductUrl = "https://www.humblebundle.com";
        private const string BundlesUrl = "https://www.humblebundle.com/bundles";

        public HumbleBundle(IRepository repository,
            ILogger logger,
            IMessageFormatter messageFormatter,
            IEmoteProvider emoteProvider,
            string commandsPrefix,
            ExceptionHandler exceptionHandler)
            : base(repository, logger, messageFormatter, emoteProvider, commandsPrefix, exceptionHandler, (caller) => ((HumbleBundle)caller).ContentWatcherProcedure, TimeSpan.FromHours(24.0))
        {
            Repository.PropertyMapper
                .Id<HumbleBundleEntry, string>(x => x.IdObject, false)
                .Ignore<HumbleBundleEntry, TimeSpan>(x => x.TimeLeft)
                .Ignore<HumbleBundleEntry, bool>(x => x.IsExpired)
                .Ignore<HumbleBundleEntry, bool>(x => x.IsAboutToEnd)
                .Id<SubscribedChannel, string>(x => x.IdObject, false)
                ;

            Repository.AddInvocator<SubscribedChannel>();
            Repository.AddInvocator<HumbleBundleEntry>();
        }

        private void ContentWatcherProcedure(ContentWatcherContext ctx)
        {
            try
            {
                foreach (HumbleBundleEntry item in Repository.TryGetItems<HumbleBundleEntry>(x => !x.IsExpired))
                {
                    Entries[item.Title] = item;
                }

                ProcessBundles();
            }
            catch (Exception ex)
            {
                Logger.LogMessage(LogLevel.Error, nameof(Name), ex.ToString());
            }
        }

        private void ProcessBundles()
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(BundlesUrl);

            string json = doc.GetElementbyId("landingPage-json-data")?.InnerText;

            JObject obj = JObject.Parse(json);

            List<Tuple<string, Poco.MosaicCategory>> bundles = (obj["data"] as JObject)?.Properties().Select(x => Tuple.Create($"{x.Name.First().ToString().ToUpper()}{x.Name.Substring(1)}", x.Value.ToObject<Poco.MosaicCategory>())).ToList();

            List<HumbleBundleEntry> entriesToNotify = new List<HumbleBundleEntry>();

            foreach (Tuple<string, Poco.MosaicCategory> tuple in bundles)
            {
                foreach (Poco.Product product in tuple.Item2.Mosaic.SelectMany(x => x.products))
                {
                    string itemCountHighlight = product.hover_highlights.FirstOrDefault();
                    string url = $"{BaseProductUrl}{product.product_url}";

                    HumbleBundleEntry entry = new HumbleBundleEntry()
                    {
                        Title = product.tile_name,
                        Category = tuple.Item1,
                        StarTime = product.StartDateDatetime,
                        EndTime = product.EndDateDatetime,
                        ItemCountHighlight = itemCountHighlight,
                        Url = url
                    };

                    if (!entry.IsExpired && Entries.TryAdd(entry.Url, entry))
                    {
                        Repository.SetItem(entry);

                        entriesToNotify.Add(entry);
                    }
                }
            }

            foreach (HumbleBundleEntry item in Entries.Values.ToList())
            {
                if (item.IsExpired)
                {
                    Entries.TryRemove(item.Title, out _);
                }
                else if (item.IsAboutToEnd)
                {
                    if (entriesToNotify.All(x => !string.Equals(x.Title, item.Title, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        entriesToNotify.Add(item); 
                    }
                }
            }

            SendNotifications(entriesToNotify, TruthFilter, EntryMessageFormatter);
        }

        private string EntryMessageFormatter(HumbleBundleEntry entry)
        {
            return new StringBuilder()
                    .Append($"[{entry.Category}]")
                    .Append(entry.IsAboutToEnd ? " [About to end!]" : string.Empty)
                    .Append($" {entry.Title} - {entry.ItemCountHighlight}")
                    .Append($" (Ends at {entry.EndTime:yyyy-MM-dd HH:mm})")
                    .Append($" {MessageFormatter.NoEmbed(entry.Url)}")
                    .ToString()
                ;
        }
    }
}
