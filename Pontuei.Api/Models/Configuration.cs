using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

using Pontuei.Api.Enums;

namespace Pontuei.Api.Models;

/// <summary>
/// Configuration table
/// </summary>
[Table("configuration"), DataContract]
public class Configuration : BaseEntity
{
    /// <summary>
    /// Table primary key.
    /// </summary>
    [Column("configuration_id"), DataMember]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int ConfigurationId { get; set; }

    /// <summary>
    /// Configuration name
    /// </summary>
    [Column("configuration_name"), DataMember]
    public required string ConfigurationName { get; set; }

    /// <summary>
    /// Configuration description
    /// </summary>
    [Column("configuration_description"), DataMember]
    public string? ConfigurationDescription { get; set; }

    /// <summary>
    /// Configuration value
    /// </summary>
    [Column("configuration_value"), DataMember]
    public required string ConfigurationValue { get; set; }

    /// <summary>
    /// Configuration type
    /// </summary>
    [Column("configuration_type"), DataMember]
    public required ConfigurationType ConfigurationType { get; set; }
}