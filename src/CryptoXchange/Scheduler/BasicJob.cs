using System;

namespace CryptoXchange.Scheduler
{
    public class BasicJob : IJob
    {
        private readonly Trigger _trigger;
        private DateTime _lastExecuted;

        public BasicJob(Trigger trigger)
        {
            _trigger = trigger;
        }

        public DateTime LastExecuted { get { return _trigger.StartTime; } set { _lastExecuted = value; } }

        public bool IsRepeatable
        {
            get { return false; }
        }

        public int RepetitionIntervalInSeconds
        {
            get { return 0; }
        }

        public int FailedAttemptCount { get; set; }

        public void Execute()
        {
            if (null != _trigger)
                _trigger.TaskAction.Invoke();
        }
    }
}
