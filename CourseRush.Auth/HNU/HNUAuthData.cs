using System.Diagnostics.CodeAnalysis;

namespace CourseRush.Auth.HNU;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class HNUAuthData
{
    public static readonly AuthDataKey<string> PC0 = new("_pc0");
    public static readonly AuthDataKey<string> PF0 = new("_pf0");
    public static readonly AuthDataKey<string> PV0 = new("_pv0");
    public static readonly AuthDataKey<string> JSESSIONID = new("JSESSIONID");
    public static readonly AuthDataKey<string> SDP_USER_TOKEN = new("sdp_user_token");  
    public static readonly AuthDataKey<string> SESSION = new("SESSION");  
    public static readonly AuthDataKey<string> TOKEN = new("TOKEN");  
    public static readonly AuthDataKey<string> SID = new("sid");  
    public static readonly AuthDataKey<string> SID_SIG = new("sid.sig");  
    public static readonly AuthDataKey<string> SID_LEGACY = new("sid-legacy");   
    public static readonly AuthDataKey<string> SID_LEGACY_SIG = new("sid-legacy.sig");   
    
    public static readonly AuthDataKey<Uri> CAS_AUTH_REDIRECT_URL = new("CAS_AUTH_REDIRECT_URL");
}