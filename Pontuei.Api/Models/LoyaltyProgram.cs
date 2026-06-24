using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Pontuei.Api.Models;

[Table("loyalty_program"), DataContract]
public class LoyaltyProgram : BaseEntity
{
    [Column("loyalty_program_id"), DataMember]
    public int LoyaltyProgramId { get; set; }

    /// <summary>
    /// Gets or sets the name of the loyalty program, which is a required field and represents the program's title or identifier.
    /// </summary>
    [Column("loyalty_program_name"), DataMember]
    public required string LoyaltyProgramName { get; set; }

    /// <summary>
    /// Gets or sets the URL of the logo image for the loyalty program, which is an optional field and can be used to display the program's branding in user interfaces.
    /// </summary>
    [Column("loyalty_program_logo_url"), DataMember]
    public string? LoyaltyProgramLogoUrl { get; set; }

    /// <summary>
    /// Gets or sets the primary color of the loyalty program's branding, which is an optional field and can be used to customize the appearance of the program in user interfaces.
    /// </summary>
    [Column("loyalty_program_brand_primary_color"), DataMember]
    public string? LoyaltyProgramBrandPrimaryColor { get; set; }

    /// <summary>
    /// Gets or sets the secondary color of the loyalty program's branding, which is an optional field and can be used to further customize the appearance of the program in user interfaces.
    /// </summary>
    [Column("loyalty_program_brand_secondary_color"), DataMember]
    public string? LoyaltyProgramBrandSecondaryColor { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the loyalty program is currently active. This boolean field can be used to enable or disable the program for users, allowing for temporary suspensions or permanent deactivations as needed.
    /// </summary>
    [Column("loyalty_program_is_active"), DataMember]
    public bool LoyaltyProgramIsActive { get; set; }

}