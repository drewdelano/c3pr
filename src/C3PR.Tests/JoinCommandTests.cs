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
                ":train: <qa> :r: @captain.hook + :er: @peter.pan + @wendy.darling | :m: @tinkerbell | @tinkerbell + @john.darling",
                ".join",
                ":train: <qa> :r: @captain.hook + :er: @peter.pan + @wendy.darling | :m: @tinkerbell | @tinkerbell + @john.darling | @wendy.darling");
        }


        [Test]
        public async Task Add()
        {
            await CommandLineTest<AddCommand>(
                "@wendy.darling",
                ":train: <rollcall> @wendy.darling",
                ".add @captain.hook 0",
                ":train: <rollcall> @wendy.darling + @captain.hook");
        }

        [Test]
        public async Task JoinEmptyTrain()
        {
            await CommandLineTest<JoinCommand>(
                "@wendy.darling",
                "",
                ".join",
                ":train: <rollcall> @wendy.darling");
        }


        [Test]
        public async Task JoinSpecific()
        {
            await CommandLineTest<JoinCommand>(
                "@wendy.darling",
                ":train: <qa> :r: @captain.hook + :er: @peter.pan + @wendy.darling | :m: @tinkerbell | @tinkerbell + @john.darling",
                ".join 1",
                ":train: <qa> :r: @captain.hook + :er: @peter.pan + @wendy.darling | :m: @tinkerbell + @wendy.darling | @tinkerbell + @john.darling");
        }


        [Test]
        public async Task JoinSpecificSkipDuplicate()
        {
            await CommandLineTest<JoinCommand>(
                "@wendy.darling",
                ":train: <qa> :r: @captain.hook + :er: @peter.pan + @wendy.darling | :m: @tinkerbell | @tinkerbell + @john.darling",
                ".join 0",
                null,
                "@wendy.darling: You're already on that carriage.");

        }

        [Test]
        public async Task JoinSpecificRespectLock()
        {
            await CommandLineTest<JoinCommand>(
                "@wendy.darling",
                ":train: <qa> :l: @captain.hook + :er: @peter.pan | :m: @tinkerbell | @tinkerbell + @john.darling",
                ".join 0",
                null,
                $"@wendy.darling: That carriage is locked, please check with the driver to coordinate shipping or join another carriage.");
        }




        [Test]
        public async Task Kick()
        {
            await CommandLineTest<KickCommand>(
                "@wendy.darling",
                ":train: <qa> :r: @captain.hook + :er: @peter.pan",
                ".kick @peter.pan",
                ":train: <qa> :r: @captain.hook");

        }

        [Test]
        public async Task Part()
        {

            await CommandLineTest<PartCommand>(
                "@wendy.darling",
                ":train: <rollcall> :r: @captain.hook + :er: @peter.pan + @wendy.darling | :m: @tinkerbell | @tinkerbell + @john.darling",
                ".part",
                ":train: <rollcall> :r: @captain.hook + :er: @peter.pan | :m: @tinkerbell | @tinkerbell + @john.darling");
        }

        [Test]
        public async Task PartSpecific()
        {
            await CommandLineTest<PartCommand>(
                "@wendy.darling",
                ":train: <qa> :r: @captain.hook + :er: @peter.pan + @wendy.darling | :m: @wendy.darling | @tinkerbell + @john.darling",
                ".part 1",
                ":train: <qa> :r: @captain.hook + :er: @peter.pan + @wendy.darling | @tinkerbell + @john.darling");
        }


        [Test]
        public async Task Driver()
        {
            await CommandLineTest<DriverCommand>(
                "@wendy.darling",
                ":train: <qa> @captain.hook + @wendy.darling",
                ".driver",
                ":train: <qa> @wendy.darling + @captain.hook");
        }

        [Test]
        public async Task DriverResetsReady()
        {
            await CommandLineTest<DriverCommand>(
                "@wendy.darling",
                ":train: <qa> @captain.hook + :r: @wendy.darling",
                ".driver",
                ":train: <qa> @wendy.darling + @captain.hook");
        }

        [Test]
        public async Task DriverResetsWhenTargeting()
        {
            await CommandLineTest<DriverCommand>(
                "@wendy.darling",
                ":train: <qa> @captain.hook + :r: @peter.pan",
                ".driver @peter.pan",
                ":train: <qa> @peter.pan + @captain.hook");
        }

        [Test]
        public async Task DriverWorksEvenWhenTheCarriageIsntUpYet()
        {
            await CommandLineTest<DriverCommand>(
                "@wendy.darling",
                ":train: <qa> @captain.hook + @peter.pan | @captain.hook + @wendy.darling",
                ".driver",
                ":train: <qa> @captain.hook + @peter.pan | @wendy.darling + @captain.hook");
        }

        [Test]
        public async Task DriverFailsWhenAmbiguous()
        {
            await CommandLineTest<DriverCommand>(
                "@wendy.darling",
                ":train: <qa> @captain.hook + @wendy.darling | @captain.hook + @wendy.darling",
                ".driver",
                null,
                "@wendy.darling: You're on multiple carriages, which one did you mean?  Remember it's zero-based.");
        }

        [Test]
        public async Task DriverWorksWhenSpecific()
        {
            await CommandLineTest<DriverCommand>(
                "@wendy.darling",
                ":train: <qa> @captain.hook + @wendy.darling | @captain.hook + @wendy.darling",
                ".driver 1",
                ":train: <qa> @captain.hook + @wendy.darling | @wendy.darling + @captain.hook");
        }
    }
}