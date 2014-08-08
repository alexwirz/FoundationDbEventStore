using FoundationDB.Client;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoundationDbEventStore.Tests
{
    [TestFixture]
    public class WhenAppendingEvents
    {
        IStoreEvents _eventStore;
        private readonly IEnumerable<string> _testDirectoryPath = new[] { "FoundationDbEventStore", "Test" };
        private readonly Guid _aggregateId = Guid.NewGuid();
        private readonly IEnumerable<TestEvent> _eventsExpectedToBeStored
            = new[] { new TestEvent(), new TestEvent() };
        private readonly IEnumerable<TestEvent> _alreadySavedEvents
            = new[] { new TestEvent { SomeData = "foo" }, new TestEvent { SomeData = "bar" } };
        private Exception _thrownException;        

        private async Task GivenEventStoreWithEvents(IEnumerable<TestEvent> _alreadySavedEvents)
        {
            FoundationDb.RemoveDirectory(_testDirectoryPath);
            using (var database = await Fdb.OpenAsync())
            {
                _eventStore = new FoundationDbEventStore(database, _testDirectoryPath);
                _eventStore.SaveEvents(new SaveEventsCommand { AggregateId = _aggregateId, ExpectedVersion = 0, Events = _alreadySavedEvents });
            }
        }

        [TestFixtureSetUp]
        public async Task When()
        {
            await GivenEventStoreWithEvents(_alreadySavedEvents);
            TrySaveMoreEvents();
        }

        private void TrySaveMoreEvents()
        {
            try
            {
                var saveEventsCommand = new SaveEventsCommand
                {
                    AggregateId = _aggregateId,
                    Events = _eventsExpectedToBeStored,
                    ExpectedVersion = _alreadySavedEvents.LongCount()
                };
                _eventStore.SaveEvents(saveEventsCommand);
            }
            catch (Exception exception)
            {
                _thrownException = exception;
            }
        }

        [Test]
        public void ThenNoExceptionThrown()
        {
            Assert.IsNull(_thrownException);
        }

        [Test]
        public void ThenEventStoreContainsAllEvents()
        {
            var allEvents = _alreadySavedEvents.Concat(_eventsExpectedToBeStored);
            var actualEvents = _eventStore.GetEventsForAggregate(_aggregateId);
            CollectionAssert.AreEquivalent(allEvents, actualEvents);
        }
    }
}
