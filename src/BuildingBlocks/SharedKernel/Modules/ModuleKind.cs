namespace SharedKernel.Modules;

/// <summary>
/// Defines the kind of a module, 
/// indicating whether it is a standard module or a system feature.
/// </summary>
public enum ModuleKind 
{ 
    /// <summary>
    /// Indicates that the module is a standard module,
    /// </summary>
    /// <remarks>
    /// Standard modules are typically user-facing and provide core functionality to the application.
    /// </remarks>
    Standard, 

    /// <summary>
    /// Indicates that the module is a system feature.
    /// </summary>
    /// <remarks>
    /// System features are typically internal modules that provide essential services or functionality to the application, 
    /// but may not be directly interacted with by end-users.
    /// </remarks>
    SystemFeature 
}