using System;
using System.Collections.Generic;
using System.Linq;

namespace C3PR.Core.Framework
{
    public class Train
    {
        public static Train Parse(string text)
        {
            var parser = new ChannelTopicStorageParser();
            return parser.Parse(text);
        }

        public override string ToString()
        {
            var formatter = new ChannelTopicStorageFormatter();
            return formatter.Format(this);
        }

        public List<TrainFlair> Flair { get; set; }
        public Phase Phase { get; set; }
        public bool IsEmpty => !Carriages.Any();
        public List<Carriage> Carriages { get; set; }

    }
}
