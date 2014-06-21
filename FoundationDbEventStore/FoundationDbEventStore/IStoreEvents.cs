using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FoundationDbEventStore
{
    public interface IStoreEvents
    {
        void SaveEvents(SaveEventsCommand saveEventsCommand);
        Task SaveEventsAsync(SaveEventsCommand saveEventsCommand, CancellationToken cancellationToken);
        IEnumerable<Event> GetEventsForAggregate(Guid aggregateId);
        Task<List<Event>> GetEventsForAggregateAsync(Guid aggregateId, CancellationToken cancellationToken);
        IEnumerable<Event> GetEventsSinceVersion(Guid aggregateId, long version);
    }
}