namespace Logistics.Application.Abstractions.Common;

/// <summary>
/// Marker interface for application workflow services. Implementations are auto-registered
/// by Scrutor in <c>Logistics.Application.Registrar.AddApplicationLayer</c> with Scoped
/// lifetime. Use only for services whose implementation lives in
/// <c>Logistics.Application</c>; ports with Infrastructure implementations are registered
/// in their respective Infrastructure registrars.
/// </summary>
public interface IApplicationService;
