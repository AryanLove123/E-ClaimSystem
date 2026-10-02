using EClaim.Domain.Common;

namespace EClaim.Domain.Entities;

public class ClaimDocument : BaseEntity
{
    public int ClaimId { get; set; }
    public Claim Claim { get; set; } = null!;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int UploadedByUserId { get; set; }
    public User UploadedBy { get; set; } = null!;
}
