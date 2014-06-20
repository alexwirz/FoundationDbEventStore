using System;
using System.Collections.Generic;
using System.Threading;

namespace FoundationDbEventStore
{
    public class SaveEventsCommand
    {
        public Guid AggregateId { get; set; }

        public IEnumerable<Event> Events { get; set; }

        public long ExpectedVersion { get; set; }

        public CancellationToken CancellationToken { get; set; }
    }
}
