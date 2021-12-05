using Newtonsoft.Json;
using System.Collections.Generic;

namespace C3PR.Core.Framework.Slack
{
    public class SlackMessageStorage
    {
        public static List<SlackMessageStorage> Parse(string json)
        {
            return JsonConvert.DeserializeObject<List<SlackMessageStorage>>(json) ?? new List<SlackMessageStorage>();
        }

        public static string Stringify(List<SlackMessageStorage> channelConfigs)
        {
            return JsonConvert.SerializeObject(channelConfigs);
        }

        public string ChannelName { get; set; }

        public string ShipUrl { get; set; }
    }
}
