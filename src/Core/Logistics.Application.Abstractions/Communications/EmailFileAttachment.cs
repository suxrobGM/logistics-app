namespace Logistics.Application.Abstractions.Communications;

/// <summary>
/// A file sent inline with an email.
/// </summary>
public record EmailFileAttachment(string FileName, byte[] Content, string ContentType);
