using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

using Pontuei.Api.Enums;

namespace Pontuei.Api.Models;

/// <summary>
/// Represents media files associated with a transaction, including their type, URL, and display order.
/// </summary>
[Table("transaction_media"), DataContract]
public class TransactionMedia
{
    [Column("transaction_media_id"), DataMember]
    public Guid TransactionMediaId { get; set; }

    /// <summary>
    /// Unique identifier for the transaction associated with this media file.
    /// </summary>
    [Column("transaction_id"), DataMember]
    public Guid TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the transaction associated with this media file, establishing a relationship between the media and its corresponding transaction. 
    /// This allows for tracking and managing media files related to specific transactions, such as receipts or images of purchased items.
    /// </summary>
    [ForeignKey("TransactionId"), IgnoreDataMember]
    public Transaction? Transaction { get; set; }

    /// <summary>
    /// Gets or sets the URL of the media file associated with the transaction, which is an optional field and can be used to store links to images, documents, 
    /// or other media related to the transaction.
    /// </summary>
    [Column("transaction_media_file_url"), DataMember]
    public string? TransactionMediaFileUrl { get; set; }

    /// <summary>
    /// Gets or sets the type of the media file associated with the transaction, which is a required field and indicates whether the media is an image, document, or other supported format.
    /// This field helps categorize and manage different types of media files related to transactions.
    /// </summary>
    [Column("transaction_media_file_type"), DataMember]
    public TransactionMediaFileType TransactionMediaFileType { get; set; }

    /// <summary>
    /// Gets or sets the display order for the media file associated with the transaction, which is a required field and determines the sequence in which media files are presented to users.
    /// This field allows for organizing and prioritizing media files related to a transaction, ensuring that important or relevant media is displayed first.
    /// </summary>
    [Column("transaction_media_display_order"), DataMember]
    public short TransactionMediaDisplayOrder { get; set; }
}