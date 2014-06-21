using FoundationDB.Client;
using NUnit.Framework;
using System;

namespace FoundationDbEventStore.Tests
{
    [TestFixture]
    public class WhenEncodingEvents
    {
        private readonly TestEvent _givenEvent = new TestEvent { SomeData = "foo" };
        private Slice _encodedValue;
        private Exception _thrownException;

        [TestFixtureSetUp]
        public void When()
        {
            try
            {
                _encodedValue = EventValueEncoding.Encode(_givenEvent);
            }
            catch (Exception exception)
            {
                _thrownException = exception;
            }
        }

        [Test]
        public void ThenNoExceptionThrown()
        {
            Assert.IsNull(_thrownException);
        }

        [Test]
        public void ThenEncodedEventCanBeDecodedToOriginalValue ()
        {
            var decodedEvent = EventValueEncoding.Decode(_encodedValue);
            Assert.That(decodedEvent, Is.EqualTo(_givenEvent));
        }
    }
}
