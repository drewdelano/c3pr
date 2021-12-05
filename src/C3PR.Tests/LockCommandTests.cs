using System;
using System.Linq;
using System.Threading.Tasks;
using C3PR.Core.Commands;
using C3PR.Core.Framework;
using C3PR.Core.Services;
using Moq;
using NUnit.Framework;

namespace C3PR.Tests
{
    public class LockCommandTests : TestBase
    {

        [Test]
        public async Task Lock()
        {
            await CommandLineTest<LockCommand>(
                "@wendy.darling",
                ":choo: <qa> :r: @captain.hook + :er: @peter.pan | @wendy.darling | @tinkerbell + @john.darling",
                ".lock",
                ":choo: <qa> :r: @captain.hook + :er: @peter.pan | :l: @wendy.darling | @tinkerbell + @john.darling");
        }

        [Test]
        public async Task LockSpecific()
        {
            await CommandLineTest<LockCommand>(
                "@wendy.darling",
                ":choo: <qa> :r: @captain.hook + :er: @peter.pan | @tinkerbell + @john.darling",
                ".lock 0",
                ":choo: <qa> :l: :r: @captain.hook + :er: @peter.pan | @tinkerbell + @john.darling");
        }


        [Test]
        public async Task Unlock()
        {
            await CommandLineTest<UnlockCommand>(
                "@wendy.darling",
                ":choo: <rollcall> :l: @wendy.darling",
                ".unlock",
                ":choo: <rollcall> @wendy.darling");
        }
    }
}