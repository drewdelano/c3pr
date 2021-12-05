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
    public class ReadyCommandTests : TestBase
    {

        [Test]
        public async Task Ready()
        {
            await CommandLineTest<ReadyCommand>(
                "@wendy.darling",
                ":choo: <qa> @captain.hook + @wendy.darling",
                ".ready",
                ":choo: <qa> @captain.hook + :r: @wendy.darling");
        }


        [Test]
        public async Task EverReady()
        {
            await CommandLineTest<EverReadyCommand>(
                "@wendy.darling",
                ":choo: <qa> @captain.hook + @wendy.darling",
                ".everready",
                ":choo: <qa> @captain.hook + :er: @wendy.darling");
        }

        [Test]
        public async Task ReadyAdvanceWhenAllReady()
        {
            await CommandLineTest<ReadyCommand>(
                "@wendy.darling",
                ":choo: <rollcall> :r: @captain.hook + @wendy.darling",
                ".ready",
                ":choo: <merging> @captain.hook + @wendy.darling",
                "@captain.hook\n@wendy.darling\n\nMerge your PRs when you're ready and then .ready to indicate that you're done");
        }

        [Test]
        public async Task ReadyDoesNotAdvanceWhenHeld()
        {
            await CommandLineTest<ReadyCommand>(
                "@wendy.darling",
                ":choo: :hold: <rollcall> :r: @captain.hook + @wendy.darling",
                ".ready",
                null,
                "@wendy.darling: Train is held!  Can't continue.");
        }

        [Test]
        public async Task EverReadyDoesNotAdvanceWhenHeld()
        {
            await CommandLineTest<EverReadyCommand>(
                "@wendy.darling",
                ":choo: :hold: <rollcall> :r: @captain.hook + @wendy.darling",
                ".everready",
                null,
                "@wendy.darling: Train is held!  Can't continue.");
        }

        [Test]
        public async Task ReadyAdvanceCarriageWhenNeeded()
        {
            await CommandLineTest<ReadyCommand>(
                "@wendy.darling",
                ":choo: <prod> :r: @captain.hook + @wendy.darling | @peter.pan",
                ".ready",
                ":choo: <rollcall> @peter.pan",
                "@peter.pan\n\nEverybody ready-up and let's get this train a-rollin'");
        }

        [Test]
        public async Task ReadyAdvanceCarriageEmptyTrainWhenNeeded()
        {
            await CommandLineTest<ReadyCommand>(
                "@wendy.darling",
                ":choo: <prod> :r: @captain.hook + @wendy.darling",
                ".ready",
                ":choo: ");
        }


        [Test]
        public async Task ReadyEmptyTrain()
        {
            await CommandLineTest<ReadyCommand>(
                "@wendy.darling",
                ":choo:",
                ".ready",
                null,
                "@wendy.darling: You're not on the train.");
        }


        [Test]
        public async Task ReadySpecific()
        {

            await CommandLineTest<ReadyCommand>(
                "@wendy.darling",
                ":choo: <qa> @captain.hook + @wendy.darling",
                ".ready @captain.hook",
                ":choo: <qa> :r: @captain.hook + @wendy.darling");
        }

        [Test]
        public async Task UnreadySpecific()
        {

            await CommandLineTest<UnreadyCommand>(
                "@wendy.darling",
                ":choo: <qa> :r: @captain.hook + @wendy.darling",
                ".unready @captain.hook",
                ":choo: <qa> @captain.hook + @wendy.darling");
        }

        [Test]
        public async Task Hold()
        {

            await CommandLineTest<HoldCommand>(
                "@wendy.darling",
                ":choo: <qa> @captain.hook + @wendy.darling",
                ".hold",
                ":choo: :hold: <qa> @captain.hook + @wendy.darling");
        }

        [Test]
        public async Task Unhold()
        {

            await CommandLineTest<UnholdCommand>(
                "@wendy.darling",
                ":choo: :hold: <qa> @captain.hook + @wendy.darling",
                ".unhold",
                ":choo: <qa> @captain.hook + @wendy.darling");
        }

        [Test]
        public async Task UnreadySimple()
        {

            await CommandLineTest<UnreadyCommand>(
                "@wendy.darling",
                ":choo: <qa> @captain.hook + :r: @wendy.darling",
                ".unready",
                ":choo: <qa> @captain.hook + @wendy.darling");
        }


        [Test]
        public async Task UnreadyWorksForEverReadyToo()
        {

            await CommandLineTest<UnreadyCommand>(
                "@wendy.darling",
                ":choo: <qa> @captain.hook + :er: @wendy.darling",
                ".unready",
                ":choo: <qa> @captain.hook + @wendy.darling");
        }
    }
}