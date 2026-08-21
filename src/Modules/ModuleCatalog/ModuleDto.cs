namespace ModuleCatalog;

/// <summary>
/// Represents a module in the system.
/// </summary>
public record ModuleDto(
    string Slug,
    string DisplayName,
    string? Description,
    string Kind,
    string Url
);