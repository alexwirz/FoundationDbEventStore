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
        private readonly ICompositeKeyEncoder<Guid, int> _keyEncoder
            = KeyValueEncoders.Tuples.CompositeKey<Guid, int>();

        public FoundationDbEventStore(IEnumerable<string> directoryPath)
        {
            Requires.Because("directoryPath must not be null")
                .That(directoryPath).IsNotNull();
            Requires.Because("directoryPath must contain at least one directory name")
                .ThatCollection(directoryPath).IsNotEmpty();
            _directoryPath = directoryPath;
        }

        public async Task SaveEvents(SaveEventsCommand saveEventsCommand)
        {
            Requires.Because("saveEventsCommand must not be null").That(saveEventsCommand).IsNotNull();
            Requires.Because("Collection with events to save must not be null").That(saveEventsCommand.Events).IsNotNull();
            Requires.Because("CancellationToken must not be null").That(saveEventsCommand.CancellationToken).IsNotNull();
            Requires.Because("expectedVersion must be a positive integer or 0").That(saveEventsCommand.ExpectedVersion >= 0);

            using (var database = await Fdb.OpenAsync())
            {
                var location = await GetLocationAsync(database, saveEventsCommand.CancellationToken);
                await database.WriteAsync((trans) =>
                {
                    var version = 0;
                    foreach (var item in saveEventsCommand.Events)
                    {
                        trans.Set(
                            location.EncodeKey(saveEventsCommand.AggregateId, ++version),
                            EventValueEncoding.Encode(item)
                        );
                    }
                }, saveEventsCommand.CancellationToken);
            }       
        }

        private async Task<FdbEncoderSubspace<Guid, int>> GetLocationAsync(
            IFdbDatabase database, CancellationToken cancellationToken)
        {
            var folder = await database.Directory.CreateOrOpenAsync(_directoryPath, cancellationToken);
            return new FdbEncoderSubspace<Guid, int>(folder, _keyEncoder);
        }

        public IEnumerable<Event> GetEventsForAggregate(Guid aggregateId)
        {
            return GetEventsForAggregateAsync(aggregateId, new CancellationToken()).Result;
        }

        public async Task<List<Event>> GetEventsForAggregateAsync(
            Guid aggregateId, CancellationToken cancellationToken)
        {
            Requires.Because("cancellationToken must not be null").That(cancellationToken).IsNotNull();
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
    }
}
