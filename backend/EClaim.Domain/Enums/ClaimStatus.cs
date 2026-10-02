namespace EClaim.Domain.Enums;

public enum ClaimStatus
{
    Draft = 1,
    Submitted = 2,
    UnderReview = 3,
    AdditionalDocumentsRequired = 4,
    UnderAdjustment = 5,
    PendingApproval = 6,
    Approved = 7,
    Rejected = 8,
    PaymentPending = 9,
    PaymentProcessing = 10,
    Paid = 11,
    PaymentFailed = 12
}
