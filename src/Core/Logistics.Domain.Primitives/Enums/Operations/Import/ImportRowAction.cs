namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// What an import does (validate) or did (import) with one spreadsheet row.
/// </summary>
public enum ImportRowAction
{
    Create,
    Update,
    Skip,
    Error
}
