namespace Core.Entities.Auth
{
	public class AppSettings
	{
		#region Token data
		public string Secret { get; set; }
		// Refresh token expiration time in days 
		// Expired tokens are automatically deleted from the database when expired
		public int RefreshTokenExpiry { get; set; }
		#endregion

		#region Verification Data
		public string EmailFrom { get; set; }
		public string SmtpHost { get; set; }
		public int SmtpPort { get; set; }
		public string SmtpUser { get; set; }
		public string SmtpPass { get; set; }
		#endregion
	}
}
