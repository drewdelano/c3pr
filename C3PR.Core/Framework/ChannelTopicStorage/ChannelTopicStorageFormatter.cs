using System;
using System.Collections.Generic;
using System.Linq;

namespace C3PR.Core.Framework
{
    public class ChannelTopicStorageFormatter
    {
        public string Format(Train channelTopicStorage)
        {
            var carriages = FormatCarriages(channelTopicStorage.Carriages);

            var phase = $"<{channelTopicStorage.Phase}>";
            if (carriages.Length == 0)
            {
                phase = "";
            }
            var trainFlair = string.Join(" ", channelTopicStorage.Flair.Select(f => $":{f}:"));
            if (!string.IsNullOrWhiteSpace(trainFlair))
            {
                trainFlair += " ";
            }
            return $"{trainFlair}{phase}{carriages}";
        }

        string FormatCarriages(List<Carriage> carriages)
        {
            return string.Join("|", carriages.Select(FormatCarriage));
        }

        string FormatCarriage(Carriage carriage)
        {
            var rider = FormatRiders(carriage.Riders);

            return $"{FormatCarriageFlairs(carriage.Flairs)}{rider}";
        }

        string FormatCarriageFlairs(CarriageFlairs carriageFlairs)
        {
            return $"{carriageFlairs.PreflairWhitespace}{string.Join("", carriageFlairs.Flairs.Select(FormatCarriageFlair))}";
        }

        string FormatCarriageFlair(CarriageFlair carriageFlair)
        {
            return $":{carriageFlair}:{carriageFlair.TrailingWhitespace}";
        }

        string FormatRiders(List<Rider> riders)
        {
            return string.Join("+", riders.Select(FormatRider));
        }

        string FormatRider(Rider rider)
        {
            var flairs = FormatFlairs(rider.Flairs);

            return $"{flairs}{rider.Name}{rider.TrailingWhitespace}";
        }

        string FormatFlairs(RiderFlairContainer flairs)
        {
            return $"{flairs.PreflairWhitepace}{string.Join("", flairs.Flairs.Select(FormatFlair))}{flairs.PostflairWhitepace}";
        }

        string FormatFlair(RiderFlair flair)
        {
            return $":{flair.FlairText}:{flair.WhitespaceAfter}";
        }
    }
}