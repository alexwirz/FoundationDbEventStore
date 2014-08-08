using FoundationDB.Client;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoundationDbEventStore.Tests
{
    [TestFixture]
    public class WhenReadingEvents
    {
        private readonly IEnumerable<string> _testDirectoryPath = new[] { "FoundationDbEventStore", "Test" };
        private readonly Guid _aggregateId = Guid.NewGuid();
        private readonly IEnumerable<TestEvent> _expetedEvents
            = new[] { new TestEvent(), new TestEvent() };
        private IEnumerable<Event> _actualEvents;
        private Exception _thrownException;

        [TestFixtureSetUp]
        public async Task When()
        {
            FoundationDb.RemoveDirectory(_testDirectoryPath);
            using (var database = await Fdb.OpenAsync())
            {
                var eventStore = new FoundationDbEventStore(database, _testDirectoryPath);
                eventStore.SaveEvents(new SaveEventsCommand { AggregateId = _aggregateId, Events = _expetedEvents, ExpectedVersion = 0 });
                try
                {
                    _actualEvents = eventStore.GetEventsForAggregate(_aggregateId);
                }
                catch (Exception exception)
                {
                    _thrownException = exception;
                }
            }
        }

        [Test]
        public void ThenNoExceptionThrown()
        {
            Assert.That(_thrownException, Is.Null);
        }

        [Test]
        public void ThenResultIsNotNull()
        {
            Assert.That(_actualEvents, Is.Not.Null);
        }

        [Test]
        public void ThenAllEventsRead()
        {
            CollectionAssert.AreEquivalent(_expetedEvents, _actualEvents);
        }
    }
}
