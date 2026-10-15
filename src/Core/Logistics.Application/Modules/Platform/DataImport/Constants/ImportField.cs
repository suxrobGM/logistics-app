namespace Logistics.Application.Modules.Platform.DataImport.Constants;

/// <summary>
///     A target field plus the header spellings that auto-map to it. Synonyms are compared after
///     <see cref="Services.ImportHeaderMapper.Normalize"/>, so case, spaces and punctuation don't matter.
/// </summary>
internal sealed record ImportField(string Key, string Label, bool Required, IReadOnlyList<string> Synonyms);
