using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Abstractions.Documents;

/// <summary>
/// Renders a quarterly IFTA report as a filing-ready PDF.
/// </summary>
public interface IIftaReportPdfService
{
    byte[] GenerateIftaReportPdf(IftaReportDto report, Tenant tenant);
}
