using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Pontuei.Api.Models;

/// <summary>
/// Common data between all tables.
/// </summary>
public class BaseEntity
{
    /// <summary>
    /// Row creation user.
    /// </summary>
    [Column("row_creation_user"), DataMember]
    public string CreationUser { get; set; } = "system";

    /// <summary>
    /// Row creation time.
    /// </summary>
    [Column("row_creation_time"), DataMember]
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Row update user.
    /// </summary>
    [Column("row_update_user"), DataMember]
    public string UpdateUser { get; set; } = "system";

    /// <summary>
    /// Row update time.
    /// </summary>
    [Column("row_update_time"), DataMember]
    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// If row is deleted.
    /// </summary>
    [Column("row_is_deleted"), DataMember]
    public bool IsDeleted { get; set; } = false;
}