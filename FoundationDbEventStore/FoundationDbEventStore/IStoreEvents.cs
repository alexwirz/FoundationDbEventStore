using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FoundationDbEventStore
{
    public interface IStoreEvents
    {
        Task SaveEventsAsync(SaveEventsCommand saveEventsCommand, CancellationToken cancellationToken);
        Task<List<Event>> GetEventsForAggregateAsync(Guid aggregateId, CancellationToken cancellationToken);
        Task<IEnumerable<Event>> GetEventsSinceVersionAsync(Guid aggregateId, long version, CancellationToken cancellationToken);
    }
}