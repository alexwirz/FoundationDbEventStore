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

        [SetUp]
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
    }
}
