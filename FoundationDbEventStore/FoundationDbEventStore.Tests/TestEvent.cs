using System;

namespace FoundationDbEventStore.Tests
{
    class TestEvent : Event
    {
        public override bool Equals(object obj)
        {
            return Equals(obj as TestEvent);
        }

        public bool Equals(TestEvent testEvent)
        {
            if (testEvent == null) return false;
            return Version == testEvent.Version;
        }

        public override int GetHashCode()
        {
            return Version.GetHashCode ();
        }
    }
}
