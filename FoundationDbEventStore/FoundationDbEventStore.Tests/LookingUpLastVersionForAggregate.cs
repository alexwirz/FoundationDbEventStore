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
    //[TestFixture]
    //public class LookingUpLastVersionForAggregate
    //{
    //    private readonly IEnumerable<string> _testDirectoryPath = new[] { "FoundationDbEventStore", "Test" };

    //    [Test]
    //    public async Task WhenNoEventsStoredForAggregateThenLastVersionIs0()
    //    {
    //        FoundationDb.RemoveDirectory(_testDirectoryPath);
    //        using (var database = await Fdb.OpenAsync())
    //        {
    //            var eventStore = new FoundationDbEventStore(database, _testDirectoryPath);
    //            var lastVersion = eventStore.GetLastVersion(Guid.NewGuid());
    //            Assert.AreEqual(0, lastVersion);
    //        }
    //    }

    //    [Test]
    //    public async Task WhenSomeEventsStoredForAggregateThenLastVersionIsNumberOfStoredEvents ()
    //    {
    //        FoundationDb.RemoveDirectory(_testDirectoryPath);
    //        var aggregateId = Guid.NewGuid ();
    //        var events = new[] { new TestEvent(), new TestEvent() };
    //        using (var database = await Fdb.OpenAsync())
    //        {
    //            var eventStore = new FoundationDbEventStore(database, _testDirectoryPath);
    //            await eventStore.SaveEventsAsync (new SaveEventsCommand
    //            {
    //                AggregateId = aggregateId,
    //                Events = events,
    //                ExpectedVersion = 0
    //            }, CancellationToken.None);
    //            var lastVersion = eventStore.GetLastVersion(aggregateId);
    //            Assert.AreEqual(events.LongCount(), lastVersion);
    //        }
    //    }
    //}
}
