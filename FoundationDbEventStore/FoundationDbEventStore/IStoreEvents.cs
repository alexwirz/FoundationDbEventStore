using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoundationDbEventStore
{
    public interface IStoreEvents
    {
        Task SaveEvents(SaveEventsCommand saveEventsCommand);
        IEnumerable<Event> GetEventsForAggregate(Guid aggregateId);
        IEnumerable<Event> GetEventsSinceVersion(Guid aggregateId, long version);
    }
}