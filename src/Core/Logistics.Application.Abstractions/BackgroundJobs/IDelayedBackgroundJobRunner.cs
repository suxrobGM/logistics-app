namespace Logistics.Application.Abstractions.BackgroundJobs;

/// <summary>
/// Enqueues a background job to run after a delay. Separate from
/// <see cref="IBackgroundJobRunner{T}"/> so existing runners keep a single-method contract.
/// </summary>
public interface IDelayedBackgroundJobRunner<in T>
{
    void Schedule(T request, TimeSpan delay);
}
