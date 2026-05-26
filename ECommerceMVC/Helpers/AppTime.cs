namespace ECommerceMVC.Helpers
{
	public static class AppTime
	{
		private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();

		public static DateTime VietnamNow()
		{
			return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);
		}

		private static TimeZoneInfo ResolveVietnamTimeZone()
		{
			try
			{
				// Windows timezone id
				return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
			}
			catch (TimeZoneNotFoundException)
			{
				try
				{
					// Linux/macOS timezone id
					return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
				}
				catch (TimeZoneNotFoundException)
				{
					return TimeZoneInfo.CreateCustomTimeZone("UTC+07", TimeSpan.FromHours(7), "UTC+07", "UTC+07");
				}
			}
		}
	}
}
