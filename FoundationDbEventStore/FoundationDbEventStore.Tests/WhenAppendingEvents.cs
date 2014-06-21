using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public WhenAppendingEvents()
        {
            GivenEventStoreWithEvents(_alreadySavedEvents);
        }

        private void GivenEventStoreWithEvents(IEnumerable<TestEvent> _alreadySavedEvents)
        {
            FoundationDb.RemoveDirectory(_testDirectoryPath);
            _eventStore = new FoundationDbEventStore(_testDirectoryPath);
            _eventStore.SaveEvents(new SaveEventsCommand { AggregateId = _aggregateId, ExpectedVersion = 0, Events = _alreadySavedEvents });
        }

        [TestFixtureSetUp]
        public void When()
        {            
            try
            {                
                var saveEventsCommand = new SaveEventsCommand
                {
                    AggregateId = _aggregateId,
                    Events = _eventsExpectedToBeStored,
                    ExpectedVersion = _alreadySavedEvents.LongCount ()
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
