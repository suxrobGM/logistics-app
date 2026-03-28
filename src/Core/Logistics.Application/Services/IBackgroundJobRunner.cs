namespace Logistics.Application.Services;

/// <summary>
/// Enqueues a job to run in the background via a durable job system (e.g., Hangfire).
/// Register concrete implementations for each job type in the DI container.
/// </summary>
/// <typeparam name="T">The type of request that the background job will process.</typeparam>
public interface IBackgroundJobRunner<in T>
{
    /// <summary>
    /// Enqueues a background job to process the given request. The job system will handle execution and retries.
    /// </summary>
    /// <param name="request">The request containing all necessary information for the background job to execute.</param>
    void Enqueue(T request);
}
