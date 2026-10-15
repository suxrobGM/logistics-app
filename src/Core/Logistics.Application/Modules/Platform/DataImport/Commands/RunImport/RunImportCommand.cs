using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.DataImport.Commands;

public class RunImportCommand : ICommand<Result<ImportResultDto>>, IImportFile
{
    public required Stream Content { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public long FileSizeBytes { get; init; }
    public ImportEntityType EntityType { get; init; }
    public IReadOnlyList<ImportColumnMappingDto> Mapping { get; init; } = [];

    /// <summary>
    ///     Validate only: report what each row would do and write nothing.
    /// </summary>
    public bool DryRun { get; init; }

    public bool UpdateExisting { get; init; }

    /// <summary>
    ///     Drivers only: email each newly imported driver an invitation to set a password.
    /// </summary>
    public bool SendInvitations { get; init; }
}
