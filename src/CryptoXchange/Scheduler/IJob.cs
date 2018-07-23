using System;

namespace CryptoXchange.Scheduler
{
    public interface IJob
    {
        DateTime LastExecuted { get; set; }
        bool IsRepeatable { get; }
        int RepetitionIntervalInSeconds { get; }
        int FailedAttemptCount { get; set; }
        void Execute();
    }
}
