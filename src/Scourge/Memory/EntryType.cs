namespace Scourge.Memory;

/// <summary>
/// Represents the type of memory allocation.
/// </summary>
public enum EntryType
{
    /// <summary>
    /// Undefined
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// Managed memory
    /// </summary>
    Managed = 1,

    /// <summary>
    /// Unmanaged memory
    /// </summary>
    Unmanaged = 2,
}