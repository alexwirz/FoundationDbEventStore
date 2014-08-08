using FoundationDB.Client;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FoundationDbEventStore.Tests
{
    [TestFixture]
    public class SavingFacts
    {
        private readonly IEnumerable<string> _testDirectoryPath = new[] { "FoundationDbEventStore", "Test" };

        [SetUp]
        public void ClearTestDirectory()
        {
            FoundationDb.RemoveDirectory(_testDirectoryPath);
        }

        [Test]
        public async Task GivenEmptyEventStream_StoresAllEvents()
        {
            var aggregateId = Guid.NewGuid();
            var eventsExpectedToBeStored = new[] { new TestEvent(), new TestEvent() };
            using (var database = await Fdb.OpenAsync())
            {
                // arrange:
                var eventStore = new FoundationDbEventStore(database, _testDirectoryPath);

                // act:
                var saveEventsCommand = new SaveEventsCommand
                {
                    AggregateId = aggregateId,
                    Events = eventsExpectedToBeStored,
                    ExpectedVersion = 0
                };
                await eventStore.SaveEventsAsync (saveEventsCommand, CancellationToken.None);

                // assert:
                var actualEvents = await eventStore.GetEventsForAggregateAsync (aggregateId, CancellationToken.None);
                CollectionAssert.AreEquivalent(eventsExpectedToBeStored, actualEvents);
            }
        }

        [Test]
        public async Task GivenNonEmptyEventStream_StoresAllEvents()
        {
            var aggregateId = Guid.NewGuid();
            var eventsExpectedToBeStored = new[] { new TestEvent(), new TestEvent() };
            var alreadySavedEvents = new[] { new TestEvent { SomeData = "foo" }, new TestEvent { SomeData = "bar" } };
            using (var database = await Fdb.OpenAsync())
            {
                // arrange
                var eventStore = new FoundationDbEventStore(database, _testDirectoryPath);
                await eventStore.SaveEventsAsync (new SaveEventsCommand { AggregateId = aggregateId, ExpectedVersion = 0, Events = alreadySavedEvents }, CancellationToken.None);

                // act:
                var saveEventsCommand = new SaveEventsCommand
                {
                    AggregateId = aggregateId,
                    Events = eventsExpectedToBeStored,
                    ExpectedVersion = alreadySavedEvents.Count()
                };
                await eventStore.SaveEventsAsync (saveEventsCommand, CancellationToken.None);

                // assert:
                var expectedEvents = alreadySavedEvents.Concat(eventsExpectedToBeStored);
                var actualEvents = await eventStore.GetEventsForAggregateAsync (aggregateId, CancellationToken.None);
                CollectionAssert.AreEquivalent(expectedEvents, actualEvents);
            }
        }
    }
}
