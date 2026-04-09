namespace GpMnrega.DataLayer.Models;

public class GpUser
{
    public int UserId { get; set; }
    public string UserName { get; set; } = "";
    public string UserEmail { get; set; } = "";
    public string Password { get; set; } = "";
    public string PanchayatCode { get; set; } = "";
    // Note: AuthenticateUser SP uses "PanchyatCode" (no 'a') as column name
    public string PanchyatCode { get; set; } = "";
    public string PanchyatName { get; set; } = "";
    public string PanchayatNameRegional { get; set; } = "";
    public string VidhanSabha { get; set; } = "";
    public string LokSabha { get; set; } = "";
    public string Phone { get; set; } = "";
    public bool Activated { get; set; }
    public bool Status { get; set; }   // true = subscribed
    public DateTime? EndDate { get; set; }
    public string? SubscriptionId { get; set; }
    // Geographic data — returned by AuthenticateUser SP via JOINs
    public string TalukName { get; set; } = "";
    public string TalukCode { get; set; } = "";
    public string TalukNameRegional { get; set; } = "";
    public string DistrictName { get; set; } = "";
    public string DistrictCode { get; set; } = "";
    public string DistrictNameRegional { get; set; } = "";
    public string StateName { get; set; } = "";
    public string StateCode { get; set; } = "";
    public string StateNameRegional { get; set; } = "";
    // Note: original cookie reads "short_name" — SP may return this as column name
    public string short_name { get; set; } = "";
    public string vidhanSabhaRegional { get; set; } = "";
    public string LokSabhaRegional { get; set; } = "";
}

public class DeptUser
{
    public int UserId { get; set; }
    public string UserName { get; set; } = "";
    public string UserEmail { get; set; } = "";
    public string Password { get; set; } = "";
    public string BlockCode { get; set; } = "";
    public string Agency { get; set; } = "";
    public string Phone { get; set; } = "";
    public bool Activated { get; set; }
    public bool Status { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SubscriptionId { get; set; }
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string UserType { get; set; } = "";   // "GP" or "Dept"
    public bool IsActivated { get; set; }
    public bool IsSubscribed { get; set; }
    public string UserEmail { get; set; } = "";
    public string UserName { get; set; } = "";
    public string? PanchayatCode { get; set; }
    public string? BlockCode { get; set; }
    public string? Agency { get; set; }
    // Geographic data from AuthenticateUser SP (carried through to claims)
    public string PanchyatName { get; set; } = "";
    public string PanchayatNameRegional { get; set; } = "";
    public string TalukName { get; set; } = "";
    public string TalukCode { get; set; } = "";
    public string TalukNameRegional { get; set; } = "";
    public string DistrictName { get; set; } = "";
    public string DistrictCode { get; set; } = "";
    public string DistrictNameRegional { get; set; } = "";
    public string VidhanSabha { get; set; } = "";
    public string LokSabha { get; set; } = "";
    public string VidhanSabhaRegional { get; set; } = "";
    public string LokSabhaRegional { get; set; } = "";
}

public class ProductAd
{
    public int AdId { get; set; }
    public string Title { get; set; } = "";
    public string ShortDesc { get; set; } = "";
    public string? CtaText { get; set; }
    public string? CtaUrl { get; set; }
    public string? IconClass { get; set; }       // Bootstrap icon class
    public string BadgeText { get; set; } = "";  // "Coming Soon", "Sponsored", "New"
    public string BadgeColor { get; set; } = "primary";
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}
