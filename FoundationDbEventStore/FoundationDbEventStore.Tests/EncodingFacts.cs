using FoundationDB.Client;
using NUnit.Framework;
using System;

namespace FoundationDbEventStore.Tests
{
    [TestFixture]
    public class EncodingFacts
    {       
        [Test]
        public void EncodedEventCanBeDecodedToOriginalValue ()
        {
            var data = new TestEvent { SomeData = "foo" };
            var encodedValue = EventValueEncoding.Encode(data);
            var decodedEvent = EventValueEncoding.Decode(encodedValue);
            Assert.That(decodedEvent, Is.EqualTo(data));
        }
    }
}
