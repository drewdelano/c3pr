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
                ":train: <qa> :r: @captain.hook + :er: @peter.pan | @wendy.darling | @tinkerbell + @john.darling",
                ".lock",
                ":train: <qa> :r: @captain.hook + :er: @peter.pan | :l: @wendy.darling | @tinkerbell + @john.darling");
        }

        [Test]
        public async Task LockSpecific()
        {
            await CommandLineTest<LockCommand>(
                "@wendy.darling",
                ":train: <qa> :r: @captain.hook + :er: @peter.pan | @tinkerbell + @john.darling",
                ".lock 0",
                ":train: <qa> :l: :r: @captain.hook + :er: @peter.pan | @tinkerbell + @john.darling");
        }


        [Test]
        public async Task Unlock()
        {
            await CommandLineTest<UnlockCommand>(
                "@wendy.darling",
                ":train: <rollcall> :l: @wendy.darling",
                ".unlock",
                ":train: <rollcall> @wendy.darling");
        }
    }
}