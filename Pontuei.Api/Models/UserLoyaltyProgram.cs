using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Pontuei.Api.Models;

/// <summary>
/// Represents the association between a user and a loyalty program, including display order, favorite status, and audit information.
/// </summary>
[Table("user_loyalty_program"), DataContract]
public class UserLoyaltyProgram : BaseEntity
{
    [Column("user_loyalty_program_id"), DataMember]
    public int UserLoyaltyProgramId { get; set; }


    /// <summary>
    /// Gets or sets the unique identifier for the user associated with this loyalty program entry. 
    /// This field establishes a relationship between the user and their loyalty program participation,
    /// allowing for tracking of user-specific preferences and interactions within the loyalty program context.
    /// </summary>
    [Column("user_id"), DataMember]
    public Guid UserId { get; set; }

    [ForeignKey("UserId"), IgnoreDataMember]
    public virtual User? User { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the loyalty program associated with this user entry. 
    /// This field establishes a relationship between the loyalty program and the user, enabling the system to track which loyalty programs a user is enrolled in and manage their participation, 
    /// preferences, and rewards within the context of that specific program.
    /// </summary>
    [Column("loyalty_program_id"), DataMember]
    public int LoyaltyProgramId { get; set; }

    [ForeignKey("LoyaltyProgramId"), IgnoreDataMember]
    public virtual LoyaltyProgram? LoyaltyProgram { get; set; }


    /// <summary>
    /// Gets or sets the display order for the user's loyalty program entry.
    /// This field determines the sequence in which the loyalty program is displayed to the user.
    /// </summary>
    [Column("user_loyalty_program_display_order"), DataMember]
    public short UserLoyaltyProgramDisplayOrder { get; set; }

}
