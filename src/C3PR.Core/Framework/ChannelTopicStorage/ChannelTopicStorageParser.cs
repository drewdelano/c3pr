using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace C3PR.Core.Framework
{
    public class ChannelTopicStorageParser
    {
        public Train Parse(string text)
        {
            var leadingBits = text.Split(new[] { '<', '>' });

            var trainFlair = leadingBits.FirstOrDefault()
                .Split(':')
                .Where((_, i) => i % 2 == 1)
                .Select(f => new TrainFlair(f)).ToList();

            var phase = leadingBits.Skip(1).FirstOrDefault();
            var carriages = ParseCarriages(leadingBits.Skip(2).FirstOrDefault());

            var storage = new Train
            {
                Flair = trainFlair,
                Phase = new Phase(phase),
                Carriages = carriages
            };


            return storage;
        }

        List<Carriage> ParseCarriages(string carriagesText)
        {
            if (carriagesText == null)
            {
                return new List<Carriage>();
            }

            var carriages = carriagesText.Split('|')
                .Select(ParseCarriage)
                .ToList();

            return carriages;
        }

        Carriage ParseCarriage(string carriageText)
        {
            var pieces = carriageText.Split('+');
            var maybeCarriageFlair = pieces.First().Split(':');

            var i = 1;
            for (; i < maybeCarriageFlair.Length && !Constants.KnownFlair.Contains(maybeCarriageFlair[i]); i += 2)
            {
            }
            i--;

            var flair = string.Join(':', maybeCarriageFlair, 0, i);
            if (i > 1)
            {
                flair += ":";
            }
            var riderBits = string.Join(':', maybeCarriageFlair, i, maybeCarriageFlair.Length - i);
            var riderFullyReconstructed = string.Join('+', new[] { riderBits }.Concat(pieces.Skip(1)));


            var carriage = new Carriage
            {
                Flairs = ParseCarriageFlairs(flair),
                Riders = ParseRiders(riderFullyReconstructed)
            };

            return carriage;
        }

        CarriageFlairs ParseCarriageFlairs(string flair)
        {
            var bits = flair.Split(':');

            var result = new CarriageFlairs
            {
                PreflairWhitespace = bits[0],
                Flairs = new List<CarriageFlair>()
            };
            for (var i = 1; i < bits.Length - 1; i += 2)
            {
                result.Flairs.Add(new CarriageFlair(bits[i])
                {
                    TrailingWhitespace = bits[i + 1]
                });
            }

            return result;
        }

        List<Rider> ParseRiders(string riderFullyReconstructed)
        {
            return riderFullyReconstructed.Split('+')
                .Select(ParseRider)
                .ToList();
        }

        Rider ParseRider(string rider)
        {
            var riderBits = rider.Split('@');
            var flairs = riderBits.First();
            var name = $"@{riderBits.Last()}";

            var trailingWhitespaceLen = name.Length - name.Trim().Length;
            var trailingWhitespace = name.Substring(name.Length - trailingWhitespaceLen, trailingWhitespaceLen);
            name = name.Substring(0, name.Length - trailingWhitespaceLen);

            var result = new Rider
            {
                Flairs = ParseFlairs(flairs),
                Name = name,
                TrailingWhitespace = trailingWhitespace
            };
            return result;
        }

        RiderFlairContainer ParseFlairs(string flairs)
        {
            var bits = flairs.Split(':');
            if (bits.Length == 1)
            {
                return new RiderFlairContainer
                {
                    PreflairWhitepace = flairs,
                    Flairs = new List<RiderFlair>(),
                    PostflairWhitepace = ""
                };
            }

            var resultFlairs = new List<RiderFlair>();
            for (var i = 1; i < bits.Length - 1; i += 2)
            {
                resultFlairs.Add(new RiderFlair(bits[i])
                {
                    WhitespaceAfter = bits[i + 1]
                });
            }
            resultFlairs.Last().WhitespaceAfter = "";

            var result = new RiderFlairContainer
            {
                PreflairWhitepace = bits.First(),
                Flairs = resultFlairs,
                PostflairWhitepace = bits.Last()
            };
            return result;
        }
    }
}
