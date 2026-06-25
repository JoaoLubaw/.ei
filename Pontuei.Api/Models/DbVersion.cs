using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Pontuei.Api.Models;

/// <summary>
/// Represents a database version entry.
/// </summary>
[Table("db_version"), DataContract]
public class DbVersion
{
    [Column("db_version_id"), DataMember]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int DbVersionId { get; set; }

    /// <summary>
    /// The version number of the database.
    /// </summary>
    [Column("version_number"), DataMember]
    public required string VersionNumber { get; set; }

    /// <summary>
    /// Notes about the database version.
    /// </summary>
    [Column("version_notes"), DataMember]
    public string? VersionNotes { get; set; }
}