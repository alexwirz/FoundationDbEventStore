using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationDbEventStore.Tests
{
    [TestFixture]
    public class LookingUpLastVersionForAggregate
    {
        private readonly IEnumerable<string> _testDirectoryPath = new[] { "FoundationDbEventStore", "Test" };

        [Test]
        public void WhenNoEventsStoredForAggregateThenLastVersionIs0()
        {
            var eventStore = EmptyEventStore();
            var lastVersion = eventStore.GetLastVersion(Guid.NewGuid());
            Assert.AreEqual(0, lastVersion);
        }

        private FoundationDbEventStore EmptyEventStore()
        {
            FoundationDb.RemoveDirectory(_testDirectoryPath);
            return new FoundationDbEventStore(_testDirectoryPath);
        }

        [Test]
        public void WhenSomeEventsStoredForAggregateThenLastVersionIsNumberOfStoredEvents ()
        {
            var aggregateId = Guid.NewGuid ();
            var events = new[] { new TestEvent(), new TestEvent() };
            var eventStore = EmptyEventStore();
            eventStore.SaveEvents(new SaveEventsCommand
            {
                AggregateId = aggregateId,
                Events = events,
                ExpectedVersion = 0
            });
            var lastVersion = eventStore.GetLastVersion(aggregateId);
            Assert.AreEqual(events.LongCount (), lastVersion);
        }
    }
}
