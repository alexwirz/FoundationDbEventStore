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

        private readonly IFdbDatabase _database;
        private readonly IEnumerable<string> _directoryPath;
        private readonly ICompositeKeyEncoder<Guid, long> _keyEncoder
            = KeyValueEncoders.Tuples.CompositeKey<Guid, long>();

        public FoundationDbEventStore(IFdbDatabase database, IEnumerable<string> directoryPath)
        {
            Requires.That(database != null);
            Requires.Because("directoryPath must not be null")
                .That(directoryPath).IsNotNull();
            Requires.Because("directoryPath must contain at least one directory name")
                .ThatCollection(directoryPath).IsNotEmpty();
            _database = database;
            _directoryPath = directoryPath;
        }

        public async Task SaveEventsAsync(
            SaveEventsCommand saveEventsCommand, CancellationToken cancellationToken)
        {
            Requires.Because("saveEventsCommand must not be null").That(saveEventsCommand).IsNotNull();
            Requires.Because("Collection with events to save must not be null").That(saveEventsCommand.Events).IsNotNull();
            Requires.Because("cancellationToken must not be null").That(cancellationToken).IsNotNull();
            Requires.Because("expectedVersion must be a positive integer or 0").That(saveEventsCommand.ExpectedVersion >= 0);

            cancellationToken.ThrowIfCancellationRequested();
            var location = await GetLocationAsync(_database, cancellationToken);
            var version = await GetLastVersionAsync(
                saveEventsCommand.AggregateId, _database, location, cancellationToken);
            await _database.WriteAsync((trans) =>
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

        private async Task<FdbEncoderSubspace<Guid, long>> GetLocationAsync(
            IFdbDatabase database, CancellationToken cancellationToken)
        {
            var folder = await database.Directory.CreateOrOpenAsync(_directoryPath, cancellationToken);
            return new FdbEncoderSubspace<Guid, long>(folder, _keyEncoder);
        }

        public async Task<List<Event>> GetEventsForAggregateAsync(
            Guid aggregateId, CancellationToken cancellationToken)
        {
            Requires.Because("cancellationToken must not be null").That(cancellationToken).IsNotNull();
            cancellationToken.ThrowIfCancellationRequested();
            var location = await GetLocationAsync(_database, cancellationToken);
            return await _database.QueryAsync(
                (trans) => trans
                    .GetRange(FdbKeyRange.StartsWith(location.Partial.EncodeKey(aggregateId)))
                    .Select(kv => EventValueEncoding.Decode(kv.Value)),
                cancellationToken
            );
        }

        public async Task<IEnumerable<Event>> GetEventsSinceVersionAsync (Guid aggregateId, long version, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public long GetLastVersion(Guid aggregateId)
        {
            return GetLastVersionAsync(aggregateId, new CancellationToken()).Result;
        }

        private async Task<long> GetLastVersionAsync(Guid aggregateId, CancellationToken cancellationToken)
        {
            Requires.Because("cancellationToken must not be null").That(cancellationToken).IsNotNull();
            cancellationToken.ThrowIfCancellationRequested();
            var location = await GetLocationAsync(_database, cancellationToken);
            return await GetLastVersionAsync(aggregateId, _database, location, cancellationToken);
        }

        private async Task<long> GetLastVersionAsync(Guid aggregateId, IFdbDatabase database,
            FdbEncoderSubspace<Guid, long> location, CancellationToken cancellationToken)
        {
            var range = FdbKeyRange.PrefixedBy(location.Pack<Guid>(aggregateId));
            return await Fdb.System.EstimateCountAsync(database, range, cancellationToken);
        }
    }
}
