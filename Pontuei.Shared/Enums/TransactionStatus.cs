namespace Pontuei.Shared.Enums;

/// <summary>
/// Describes the status of a transaction in the system.
/// </summary>
public enum TransactionStatus
{
    /// <summary>
    /// Indicates that the transaction is pending and has not yet been completed or processed.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Indicates that the transaction has been successfully received and processed.
    /// </summary>
    Received = 1,

    /// <summary>
    /// Indicates that the transaction is currently under dispute and requires further investigation or resolution.
    /// </summary>
    Disputed = 2,

    /// <summary>
    /// Indicates that the transaction is late and has not been processed within the expected timeframe.
    /// </summary>
    Late = 3
}
