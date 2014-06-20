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
    public class WhenSavingEvents
    {
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
                var eventStore = new FoundationDbEventStore(_testDirectoryPath);
                var saveEventsCommand = new SaveEventsCommand
                {
                    AggregateId = _aggregateId,
                    CancellationToken = new CancellationTokenSource().Token,
                    Events = _eventsExpectedToBeStored,
                    ExpectedVersion = 0
                };
                eventStore.SaveEvents(saveEventsCommand).Wait ();
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
        public void ThenAllEventsSaves()
        {

        }
    }
}
