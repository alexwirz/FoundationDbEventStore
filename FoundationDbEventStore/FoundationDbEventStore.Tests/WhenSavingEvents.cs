using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace FoundationDbEventStore.Tests
{
    [TestFixture]
    public class WhenSavingEvents
    {
        IStoreEvents _eventStore;
        private readonly IEnumerable<string> _testDirectoryPath = new[] { "FoundationDbEventStore", "Test" };
        private readonly Guid _aggregateId = Guid.NewGuid();
        private readonly IEnumerable<TestEvent> _eventsExpectedToBeStored
            = new[] { new TestEvent(), new TestEvent() };
        private Exception _thrownException;

        [TestFixtureSetUp]
        public void When()
        {
            FoundationDb.RemoveDirectory(_testDirectoryPath);
            try
            {
                _eventStore = new FoundationDbEventStore(_testDirectoryPath);
                var saveEventsCommand = new SaveEventsCommand
                {
                    AggregateId = _aggregateId,
                    Events = _eventsExpectedToBeStored,
                    ExpectedVersion = 0
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
        public void ThenAllEventsSaved()
        {
            var actualEvents = _eventStore.GetEventsForAggregate(_aggregateId);
            CollectionAssert.AreEquivalent(_eventsExpectedToBeStored, actualEvents);
        }
    }
}
