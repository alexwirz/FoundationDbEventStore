using FoundationDB.Client;
using NGuard;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FoundationDbEventStore
{
    public class FoundationDbEventStore : IStoreEvents
    {
        private const string EventStorePath = "FoundationDbEventStore";

        private readonly IEnumerable<string> _directoryPath;
        private readonly ICompositeKeyEncoder<Guid, long> _keyEncoder
            = KeyValueEncoders.Tuples.CompositeKey<Guid, long>();

        public FoundationDbEventStore(IEnumerable<string> directoryPath)
        {
            Requires.Because("directoryPath must not be null")
                .That(directoryPath).IsNotNull();
            Requires.Because("directoryPath must contain at least one directory name")
                .ThatCollection(directoryPath).IsNotEmpty();
            _directoryPath = directoryPath;
        }

        public void SaveEvents(SaveEventsCommand saveEventsCommand)
        {
            SaveEventsAsync(saveEventsCommand, new CancellationToken ()).Wait();
        }

        public async Task SaveEventsAsync(
            SaveEventsCommand saveEventsCommand, CancellationToken cancellationToken)
        {
            Requires.Because("saveEventsCommand must not be null").That(saveEventsCommand).IsNotNull();
            Requires.Because("Collection with events to save must not be null").That(saveEventsCommand.Events).IsNotNull();
            Requires.Because("cancellationToken must not be null").That(cancellationToken).IsNotNull();
            Requires.Because("expectedVersion must be a positive integer or 0").That(saveEventsCommand.ExpectedVersion >= 0);

            cancellationToken.ThrowIfCancellationRequested();
            using (var database = await Fdb.OpenAsync())
            {
                var location = await GetLocationAsync(database, cancellationToken);
                var version = await GetLastVersionAsync (saveEventsCommand.AggregateId, cancellationToken);
                await database.WriteAsync((trans) =>
                {
                    
                    foreach (var item in saveEventsCommand.Events)
                    {
                        trans.Set(
                            location.EncodeKey(saveEventsCommand.AggregateId, ++version),
                            EventValueEncoding.Encode(item)
                        );
                    }
                }, cancellationToken);
            }       
        }

        private async Task<FdbEncoderSubspace<Guid, long>> GetLocationAsync(
            IFdbDatabase database, CancellationToken cancellationToken)
        {
            var folder = await database.Directory.CreateOrOpenAsync(_directoryPath, cancellationToken);
            return new FdbEncoderSubspace<Guid, long>(folder, _keyEncoder);
        }

        public IEnumerable<Event> GetEventsForAggregate(Guid aggregateId)
        {
            return GetEventsForAggregateAsync(aggregateId, new CancellationToken()).Result;
        }

        public async Task<List<Event>> GetEventsForAggregateAsync(
            Guid aggregateId, CancellationToken cancellationToken)
        {
            Requires.Because("cancellationToken must not be null").That(cancellationToken).IsNotNull();
            cancellationToken.ThrowIfCancellationRequested();
            using (var database = await Fdb.OpenAsync())
            {
                var location = await GetLocationAsync(database, cancellationToken);
                return await database.QueryAsync(
                    (trans) => trans
                        .GetRange(FdbKeyRange.StartsWith(location.Partial.EncodeKey(aggregateId)))
                        .Select(kv => EventValueEncoding.Decode(kv.Value)),
                    cancellationToken
                );
            }
        }        

        public IEnumerable<Event> GetEventsSinceVersion(Guid aggregateId, long version)
        {
            throw new NotImplementedException();
        }

        public long GetLastVersion(Guid aggregateId)
        {
            return GetLastVersionAsync (aggregateId, new CancellationToken ()).Result;
        }

        private async Task<long> GetLastVersionAsync(Guid aggregateId, CancellationToken cancellationToken)
        {
            Requires.Because("cancellationToken must not be null").That(cancellationToken).IsNotNull();
            cancellationToken.ThrowIfCancellationRequested();
            using (var database = await Fdb.OpenAsync())
            {
                var location = await GetLocationAsync(database, cancellationToken);
                var range = FdbKeyRange.PrefixedBy(location.Pack<Guid> (aggregateId));
                return await Fdb.System.EstimateCountAsync(database, range, cancellationToken);
            }
        }
    }
}
