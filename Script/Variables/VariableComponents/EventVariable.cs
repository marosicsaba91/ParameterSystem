using System;
using MUtility;

namespace PlayBox
{
    public class EventVariable : Variable
    {
        public event Action eventInvoked; 
        public TimeStamp LastInvoked { get; private set; }

        public void InvokeEvent()
        {
            LastInvoked = TimeStamp.Now();
            eventInvoked?.Invoke();
        }

        public override string ToString() => LastInvoked.SystemTimeInTicks == 0
            ? "Invoke"
            : $"Invoke ({LastInvoked.SystemTimeShortString})";

        internal override Type ValueType => typeof(void);
    }
}