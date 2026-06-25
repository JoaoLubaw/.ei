using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Pontuei.Api.Models;

/// <summary>
/// Represents a user in the system, including their personal information, authentication details, and notification preferences.
/// </summary>
[Table("user_session"), DataContract]
public class UserSession : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the user session.
    /// </summary>
    [Column("user_session_id"), DataMember]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid UserSessionId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the user associated with this session.
    /// </summary>
    [Column("user_id"), DataMember]
    public required Guid UserId { get; set; }

    [ForeignKey("UserId"), IgnoreDataMember]
    public virtual User? User { get; set; }


    /// <summary>
    /// Gets or sets the access token for the user session.
    /// </summary>
    [Column("user_session_refresh_token"), DataMember]
    public required string UserSessionRefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the expiration date and time for the user session's refresh token, indicating when the session will no longer be valid.
    /// </summary>
    [Column("user_session_refresh_token_expires_at"), DataMember]
    public required DateTime UserSessionExpiration { get; set; }



    /// <summary>
    /// Gets or sets the device information associated with the user session, providing context about the device used for authentication and session management. This information can be useful for security audits, troubleshooting, and enhancing user experience by recognizing familiar devices.
    /// </summary>
    [Column("user_session_device_info"), DataMember]
    public string? UserSessionDeviceInfo { get; set; }

    /// <summary>
    /// Gets or sets the IP address from which the user session was initiated, providing additional context for security and auditing purposes. This information can help identify unusual login patterns, potential unauthorized access, and assist in troubleshooting user session issues.
    /// </summary>
    [Column("user_session_ip_address"), DataMember]
    public string? UserSessionIpAddress { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the user session has been revoked, allowing for the management of active sessions and enhancing security by enabling administrators to invalidate sessions when necessary.
    /// </summary>
    [Column("user_session_is_revoked"), DataMember]
    public bool UserSessionIsRevoked { get; set; } = false;


    /// <summary>
    /// Gets or sets the push notification token associated with the user session, enabling the system to send targeted notifications to the user's device. This token is essential for delivering real-time updates, alerts, and messages directly to the user's device, enhancing engagement and communication within the application.
    /// </summary>
    [Column("user_session_push_notification_token"), DataMember]
    public string? UserSessionPushNotificationToken { get; set; }
}