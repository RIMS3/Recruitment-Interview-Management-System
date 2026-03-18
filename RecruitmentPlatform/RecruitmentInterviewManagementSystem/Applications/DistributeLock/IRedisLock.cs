namespace RecruitmentInterviewManagementSystem.Applications.DistributeLock
{
    public interface IRedisLock
    {
        Task<bool> AcquireAsync(string key, string value, TimeSpan expiry);
        Task ReleaseAsync(string key, string value);
    }
}
