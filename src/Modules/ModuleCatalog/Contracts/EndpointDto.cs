namespace ModuleCatalog.Contracts;

/// <summary>
/// Represents an endpoint exposed by a module.
/// </summary>
/// <param name="Route">The route template for the endpoint.</param>
/// <param name="HttpMethods">The HTTP methods supported by the endpoint.</param>
/// <param name="DisplayName">The display name of the endpoint.</param>
public sealed record EndpointDto(
    string Route,
    string[] HttpMethods,
    string? DisplayName
);