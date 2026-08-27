namespace ModuleCatalog.Contracts;

/// <summary>
/// Represents a module in the system.
/// </summary>
/// <param name="Slug">The unique identifier for the module.</param>
/// <param name="DisplayName">The display name of the module.</param>
/// <param name="Description">The description of the module.</param>
/// <param name="Kind">The kind of the module.</param>
/// <param name="Url">The URL to access the module.</param>
public sealed record ModuleDto(
    string Slug,
    string DisplayName,
    string? Description,
    string Kind,
    string Url
);