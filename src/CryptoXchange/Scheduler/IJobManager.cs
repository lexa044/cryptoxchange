namespace CryptoXchange.Scheduler
{
    public interface IJobManager
    {
        void Start();
        void Stop();
        void Add(IJob job);
        void Add(Trigger task);
        void Delete(IJob job);
    }
}
