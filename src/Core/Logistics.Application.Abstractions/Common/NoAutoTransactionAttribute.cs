namespace Logistics.Application.Abstractions.Common;

/// <summary>
/// Opts a handler out of the pipeline's automatic Unit-of-Work transaction wrapping.
/// Apply when the handler manages its own transaction, performs external side-effects
/// that must not be rolled back (webhooks, blob writes, third-party API calls), or is
/// a query that intentionally mutates DB state as a bookkeeping side-effect.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class NoAutoTransactionAttribute : Attribute;
