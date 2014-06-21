using FoundationDB.Client;
using FoundationDB.Layers.Tuples;
using Newtonsoft.Json;
using NGuard;
using System;
using System.Linq;

namespace FoundationDbEventStore
{
    public static class EventValueEncoding
    {
        public static Slice Encode<T> (T @event) where T : Event
        {
            Requires.Because("To encode an event it must not be null").That(@event).IsNotNull();

            var typeName = @event.GetType().AssemblyQualifiedName;
            var serializedEvent = JsonConvert.SerializeObject(@event);
            return FdbTuple.Pack<string, string>(typeName, serializedEvent);
        }

        public static Event Decode(Slice encodedValue)
        {
            Requires
                .Because("To decode a slice it must not be null")
                .That(encodedValue)
                .IsNotNull();

            var tuple = ConvertToTuple(encodedValue);
            var typeName = tuple.Get<string>(0);
            var json = tuple.Get<string>(1);
            var decodedEvent = JsonConvert.DeserializeObject(json, Type.GetType(typeName));
            return (Event) decodedEvent;
        }

        private static IFdbTuple ConvertToTuple (Slice slice) {
            var tuple = slice.ToTuple();
            Requires
                .Because("To decode an event from a tuple the tuple must not be empty")
                .That(!FdbTuple.Empty.Equals(tuple));
            Requires
                .Because("To decode an event from a tuple the tuple must have two elements")
                .That(tuple.Count == 2);
            Requires.Because("To decode an event from a tuple all tuple elements must be strings")
                .That(tuple.ToArray().All(item => item is string));
            return tuple;
        }
    }
}
