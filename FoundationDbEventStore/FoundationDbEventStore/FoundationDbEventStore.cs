﻿using FoundationDB.Client;
using Newtonsoft.Json;
using NGuard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FoundationDbEventStore
{
    public class FoundationDbEventStore : IStoreEvents
    {
        private const string EventStorePath = "FoundationDbEventStore";

        private readonly IEnumerable<string> _directoryPath;

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

            using (var db = await Fdb.OpenAsync())
            {
                var folder = await db.Directory.CreateOrOpenAsync(
                    _directoryPath, saveEventsCommand.CancellationToken);                
                var location = new FdbEncoderSubspace<Guid, int>(
                    folder,
                    KeyValueEncoders.Tuples.CompositeKey<Guid, int>()
                );
                var encoder = KeyValueEncoders.Values.StringEncoder;
                await db.WriteAsync((trans) =>
                {
                    var version = 0;
                    foreach (var item in saveEventsCommand.Events)
                    {
                        trans.Set(
                            location.EncodeKey(saveEventsCommand.AggregateId, ++version),
                            encoder.EncodeValue(JsonConvert.SerializeObject(item))
                        );
                    }
                }, saveEventsCommand.CancellationToken);
            }
        }

        public IEnumerable<Event> GetEventsForAggregate(Guid aggregateId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Event> GetEventsSinceVersion(Guid aggregateId, long version)
        {
            throw new NotImplementedException();
        }
    }
}
