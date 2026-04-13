using System.Text;

namespace ECommerceMVC.Helpers
{
	public class MyUtil
	{
		public static string UploadHinh(IFormFile Hinh, string folder)
		{
			try
			{
				var safeFileName = Path.GetFileName(Hinh.FileName);
				if (string.IsNullOrWhiteSpace(safeFileName))
				{
					return string.Empty;
				}

				var targetDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", folder);
				Directory.CreateDirectory(targetDir);

				var extension = Path.GetExtension(safeFileName);
				var finalFileName = $"{Path.GetFileNameWithoutExtension(safeFileName)}_{Guid.NewGuid():N}{extension}";
				var fullPath = Path.Combine(targetDir, finalFileName);

				using (var myfile = new FileStream(fullPath, FileMode.Create))
				{
					Hinh.CopyTo(myfile);
				}
				return finalFileName;
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		public static string GenerateRamdomKey(int length = 5)
		{
			var pattern = @"qazwsxedcrfvtgbyhnujmiklopQAZWSXEDCRFVTGBYHNUJMIKLOP!";
			var sb = new StringBuilder();
			var rd = new Random();
			for (int i = 0; i < length; i++)
			{
				sb.Append(pattern[rd.Next(0, pattern.Length)]);
			}

			return sb.ToString();
		}
	}
}
