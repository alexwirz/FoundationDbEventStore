using FoundationDB.Client;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FoundationDbEventStore.Tests
{
    [TestFixture]
    public class ReadingFacts
    {
        private readonly IEnumerable<string> _testDirectoryPath = new[] { "FoundationDbEventStore", "Test" };

        [TestFixtureSetUp]
        public void ClearTestDirectory()
        {
            FoundationDb.RemoveDirectory(_testDirectoryPath);
        }

        [Test]
        public async Task GivenNonEmptyEventStream_GetEventsForAggregate_ReturnsAllEventsFromTheStream()
        {
            var aggregateId = Guid.NewGuid();
            var storedEvents = new[] { new TestEvent(), new TestEvent() };
            using (var database = await Fdb.OpenAsync())
            {
                // arrange:
                var eventStore = new FoundationDbEventStore(database, _testDirectoryPath);
                var saveEventsCommand = new SaveEventsCommand
                {
                    AggregateId = aggregateId,
                    Events = storedEvents,
                    ExpectedVersion = 0
                };
                await eventStore.SaveEventsAsync (saveEventsCommand, CancellationToken.None);

                // act:
                var actualEvents = await eventStore.GetEventsForAggregateAsync (aggregateId, CancellationToken.None);

                // assert:                
                CollectionAssert.AreEquivalent(storedEvents, actualEvents);
            }
        }

        [Test]
        public async Task GivenEmptyEventStream_GetEventsSinceVersionAsync_ReteurnsEmptyEnumerable()
        {
            var aggregateId = Guid.NewGuid();
            using (var database = await Fdb.OpenAsync())
            {
                // arrange:
                var eventStore = new FoundationDbEventStore(database, _testDirectoryPath);

                // act:
                var actualEvents = await eventStore.GetEventsSinceVersionAsync(
                    aggregateId, 1, CancellationToken.None);

                // assert:                
                CollectionAssert.AreEquivalent(Enumerable.Empty<Event>(), actualEvents);
            }
        }

        [Test]
        public async Task GivenStreamWithTwoEvents_GetEventsSinceVersionAsync_WithVersionEquals2_ReteurnsTheLastEvent()
        {
            var aggregateId = Guid.NewGuid();
            var storedEvents = new[] { new TestEvent(), new TestEvent() };
            using (var database = await Fdb.OpenAsync())
            {
                // arrange:
                var eventStore = new FoundationDbEventStore(database, _testDirectoryPath);
                var saveEventsCommand = new SaveEventsCommand
                {
                    AggregateId = aggregateId,
                    Events = storedEvents,
                    ExpectedVersion = 0
                };
                await eventStore.SaveEventsAsync(saveEventsCommand, CancellationToken.None);

                // act:
                var actualEvents = await eventStore.GetEventsSinceVersionAsync(
                    aggregateId, 2, CancellationToken.None);

                // assert:                
                CollectionAssert.AreEquivalent(storedEvents.Skip(1), actualEvents);
            }
        }

        [Test]
        public async Task GivenStreamWithThreeEvents_GetEventsSinceVersionAsync_WithVersionEquals1_ReteurnsTheLastTwoEvents()
        {
            var aggregateId = Guid.NewGuid();
            var storedEvents = new[] { new TestEvent(), new TestEvent(), new TestEvent() };
            using (var database = await Fdb.OpenAsync())
            {
                // arrange:
                var eventStore = new FoundationDbEventStore(database, _testDirectoryPath);
                var saveEventsCommand = new SaveEventsCommand
                {
                    AggregateId = aggregateId,
                    Events = storedEvents,
                    ExpectedVersion = 0
                };
                await eventStore.SaveEventsAsync(saveEventsCommand, CancellationToken.None);

                // act:
                var actualEvents = await eventStore.GetEventsSinceVersionAsync(
                    aggregateId, 2, CancellationToken.None);

                // assert:                
                CollectionAssert.AreEquivalent(storedEvents.Skip(1), actualEvents);
            }
        }
    }
}
