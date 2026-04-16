namespace AngularEnterpriseAPI.DTOs.PasswordReset
{
    public class PasswordResetTokenDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsValid { get; set; }
    }
}
