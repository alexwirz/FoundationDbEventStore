using System;
using System.Collections.Generic;

namespace FoundationDbEventStore
{
    public interface IStoreEvents
    {
        void SaveEvents(Guid aggregateId, IEnumerable<Event> events, long expectedVersion);
        IEnumerable<Event> GetEventsForAggregate(Guid aggregateId);
        IEnumerable<Event> GetEventsSinceVersion(Guid aggregateId, long version);
    }
}