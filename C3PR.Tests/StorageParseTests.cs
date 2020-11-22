using System.Threading.Tasks;
using C3PR.Core.Framework;
using NUnit.Framework;

namespace C3PR.Tests
{
    public class StorageParseTests
    {
        [Test]
        public async Task RoundTrip()
        {
            var textToParse = ":train: <qa> :l: :r: @captain.hook + :er: @peter.pan + @wendy.darling | :m: @tinkerbell | @tinkerbell + @john.darling";
            var storage = Train.Parse(textToParse);

            var backToText = storage.ToString();

            Assert.AreEqual(textToParse, backToText);
        }
    }
}