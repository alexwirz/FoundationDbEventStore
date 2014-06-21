using System;

namespace FoundationDbEventStore.Tests
{
    class TestEvent : Event
    {
        public string SomeData { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as TestEvent);
        }

        public bool Equals(TestEvent testEvent)
        {
            if (testEvent == null) return false;
            return Version == testEvent.Version && AreEqual (SomeData, testEvent.SomeData);
        }

        private bool AreEqual(string first, string second)
        {
            return string.Equals(first, second);
        }

        public override int GetHashCode()
        {
            if (SomeData != null)
            {
                return Version.GetHashCode() ^ SomeData.GetHashCode();
            }

            return Version.GetHashCode();
        }
    }
}
