using RedLockNet;

namespace PollingDomainEvents;

public interface IDistributedLockService
{
    Task<IRedLock> LockAsync(string key, TimeSpan expire, TimeSpan wait, CancellationToken? cancellationToken = null);
}