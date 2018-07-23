using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace CryptoXchange.Scheduler
{
    public class JobManager : IJobManager
    {
        private readonly List<IJob> _jobQueue;
        private AutoResetEvent _autoResetEvent;
        private Thread _thread;
        private readonly object _startStopLock = new object();
        private readonly object _operationsLock = new object();

        public JobManager()
        {
            _jobQueue = new List<IJob>();
            _autoResetEvent = new AutoResetEvent(false);
        }

        public bool Started { get; private set; } = false;

        #region [ IJobManager - Implementation ]
        public void Add(Trigger task)
        {
            Add(new BasicJob(task));
        }

        public void Add(IJob job)
        {
            IJob earliestTask;
            DateTime @nowTime = DateTime.Now;
            lock (_operationsLock)
            {
                earliestTask = _jobQueue.FirstOrDefault(t => @nowTime >= t.LastExecuted.AddSeconds(t.RepetitionIntervalInSeconds));
                _jobQueue.Add(job);
            }

            if (Started && earliestTask == null || @nowTime > job.LastExecuted.AddSeconds(job.RepetitionIntervalInSeconds))
            {
                _autoResetEvent.Set();
            }
        }

        public void Delete(IJob job)
        {
            lock (_operationsLock)
            {
                _jobQueue.Remove(job);
            }
        }

        public void Start()
        {
            lock (_startStopLock)
            {
                if (!Started)
                {
                    _thread = new Thread(Run);

                    Started = true;
                    _thread.Start();
                }
            }
        }

        public void Stop()
        {
            lock (_startStopLock)
            {
                if (Started)
                {
                    lock (_operationsLock)
                    {
                        _jobQueue.Clear();
                    }

                    Started = false;
                    _autoResetEvent.Set();
                    _thread.Join();
                }
            }
        }
        #endregion

        private void Run()
        {
            TimeSpan waitTime = TimeSpan.FromSeconds(1);
            while (Started)
            {
                IJob task = GetEarliestScheduledTask();
                if (null != task)
                {
                    Exception thrownException = RunActionWithinTryCatch(task.Execute);
                    task.LastExecuted = DateTime.Now;
                    if (null != thrownException)
                        task.FailedAttemptCount++;
                    else
                        task.FailedAttemptCount = 0;

                    if (!task.IsRepeatable)
                        Delete(task);
                }
                else
                {
                    _autoResetEvent.WaitOne(waitTime);
                }
            }
        }

        Exception RunActionWithinTryCatch(Action action)
        {
            Exception thrownException = null;
            try
            {
                action();
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            return thrownException;
        }

        private IJob GetEarliestScheduledTask()
        {
            IJob response;
            DateTime @nowTime = DateTime.Now;
            lock (_operationsLock)
            {
                response = _jobQueue.FirstOrDefault(t => @nowTime >= t.LastExecuted.AddSeconds(t.RepetitionIntervalInSeconds));
            }

            return response;
        }
    }
}
