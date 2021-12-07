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
    public class JoinCommandTests : TestBase
    {
        [Test]
        public async Task Join()
        {
            await CommandLineTest<JoinCommand>(
                "@wendy.darling",
                ":choo: <qa> :r: @captain.hook + :er: @peter.pan + @wendy.darling | :m: @tinkerbell | @tinkerbell + @john.darling",
                ".join",
                ":choo: <qa> :r: @captain.hook + :er: @peter.pan + @wendy.darling | :m: @tinkerbell | @tinkerbell + @john.darling | @wendy.darling");
        }


        [Test]
        public async Task Add()
        {
            await CommandLineTest<AddCommand>(
                "@wendy.darling",
                ":choo: <rollcall> @wendy.darling",
                ".add @captain.hook 0",
                ":choo: <rollcall> @wendy.darling + @captain.hook",
                "@wendy.darling: A new rider has joined the carriage!  @captain.hook start a thread here to say what you're shipping");
        }

        [Test]
        public async Task JoinEmptyTrain()
        {
            await CommandLineTest<JoinCommand>(
                "@wendy.darling",
                "",
                ".join",
                ":choo: <rollcall> @wendy.darling");
        }


        [Test]
        public async Task JoinSpecific()
        {
            await CommandLineTest<JoinCommand>(
                "@wendy.darling",
                ":choo: <qa> :r: @captain.hook + :er: @peter.pan + @wendy.darling | :m: @tinkerbell | @tinkerbell + @john.darling",
                ".join 1",
                ":choo: <qa> :r: @captain.hook + :er: @peter.pan + @wendy.darling | :m: @tinkerbell + @wendy.darling | @tinkerbell + @john.darling",
                "@tinkerbell: A new rider has joined the carriage!  @wendy.darling start a thread here to say what you're shipping");
        }


        [Test]
        public async Task JoinSpecificSkipDuplicate()
        {
            await CommandLineTest<JoinCommand>(
                "@wendy.darling",
                ":choo: <qa> :r: @captain.hook + :er: @peter.pan + @wendy.darling | :m: @tinkerbell | @tinkerbell + @john.darling",
                ".join 0",
                null,
                "@wendy.darling: You're already on that carriage.");

        }

        [Test]
        public async Task JoinSpecificRespectLock()
        {
            await CommandLineTest<JoinCommand>(
                "@wendy.darling",
                ":choo: <qa> :l: @captain.hook + :er: @peter.pan | :m: @tinkerbell | @tinkerbell + @john.darling",
                ".join 0",
                null,
                $"@wendy.darling: That carriage is locked, please check with the driver to coordinate shipping or join another carriage.");
        }




        [Test]
        public async Task Kick()
        {
            await CommandLineTest<KickCommand>(
                "@wendy.darling",
                ":choo: <qa> :r: @captain.hook + :er: @peter.pan",
                ".kick @peter.pan",
                ":choo: <qa> :r: @captain.hook");

        }

        [Test]
        public async Task Part()
        {

            await CommandLineTest<PartCommand>(
                "@wendy.darling",
                ":choo: <rollcall> :r: @captain.hook + :er: @peter.pan + @wendy.darling | :m: @tinkerbell | @tinkerbell + @john.darling",
                ".part",
                ":choo: <rollcall> :r: @captain.hook + :er: @peter.pan | :m: @tinkerbell | @tinkerbell + @john.darling");
        }

        [Test]
        public async Task PartSpecific()
        {
            await CommandLineTest<PartCommand>(
                "@wendy.darling",
                ":choo: <qa> :r: @captain.hook + :er: @peter.pan + @wendy.darling | :m: @wendy.darling | @tinkerbell + @john.darling",
                ".part 1",
                ":choo: <qa> :r: @captain.hook + :er: @peter.pan + @wendy.darling | @tinkerbell + @john.darling");
        }


        [Test]
        public async Task Driver()
        {
            await CommandLineTest<DriverCommand>(
                "@wendy.darling",
                ":choo: <qa> @captain.hook + @wendy.darling",
                ".driver",
                ":choo: <qa> @wendy.darling + @captain.hook");
        }

        [Test]
        public async Task DriverResetsReady()
        {
            await CommandLineTest<DriverCommand>(
                "@wendy.darling",
                ":choo: <qa> @captain.hook + :r: @wendy.darling",
                ".driver",
                ":choo: <qa> @wendy.darling + @captain.hook");
        }

        [Test]
        public async Task DriverResetsWhenTargeting()
        {
            await CommandLineTest<DriverCommand>(
                "@wendy.darling",
                ":choo: <qa> @captain.hook + :r: @peter.pan",
                ".driver @peter.pan",
                ":choo: <qa> @peter.pan + @captain.hook");
        }

        [Test]
        public async Task DriverWorksEvenWhenTheCarriageIsntUpYet()
        {
            await CommandLineTest<DriverCommand>(
                "@wendy.darling",
                ":choo: <qa> @captain.hook + @peter.pan | @captain.hook + @wendy.darling",
                ".driver",
                ":choo: <qa> @captain.hook + @peter.pan | @wendy.darling + @captain.hook");
        }

        [Test]
        public async Task DriverFailsWhenAmbiguous()
        {
            await CommandLineTest<DriverCommand>(
                "@wendy.darling",
                ":choo: <qa> @captain.hook + @wendy.darling | @captain.hook + @wendy.darling",
                ".driver",
                null,
                "@wendy.darling: You're on multiple carriages, which one did you mean?  Remember it's zero-based.");
        }

        [Test]
        public async Task DriverWorksWhenSpecific()
        {
            await CommandLineTest<DriverCommand>(
                "@wendy.darling",
                ":choo: <qa> @captain.hook + @wendy.darling | @captain.hook + @wendy.darling",
                ".driver 1",
                ":choo: <qa> @captain.hook + @wendy.darling | @wendy.darling + @captain.hook");
        }
    }
}