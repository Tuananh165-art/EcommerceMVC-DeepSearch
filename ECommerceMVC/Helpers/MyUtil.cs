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

				var extension = Path.GetExtension(safeFileName)?.ToLowerInvariant() ?? string.Empty;
				if (extension.Length > 10)
				{
					extension = extension.Substring(0, 10);
				}

				// Cột Hinh trong DB đang giới hạn 50 ký tự -> dùng tên ngắn cố định để tránh truncate
				var finalFileName = $"{Guid.NewGuid():N}{extension}";
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

		public static string GetHangHoaImageUrl(string? fileName, int? maHh = null)
		{
			if (!string.IsNullOrWhiteSpace(fileName))
			{
				var safeFileName = Path.GetFileName(fileName.Trim());
				var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", "HangHoa", safeFileName);
				if (File.Exists(fullPath))
				{
					return $"/Hinh/HangHoa/{safeFileName}";
				}
			}

			var fallbackIndex = ((maHh ?? 1) - 1) % 6 + 1;
			return $"/amado/img/product-img/product{fallbackIndex}.jpg";
		}

		public static string GetLoaiImageUrl(string? fileName, int? maLoai = null)
		{
			if (!string.IsNullOrWhiteSpace(fileName))
			{
				var safeFileName = Path.GetFileName(fileName.Trim());
				var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", "Loai", safeFileName);
				if (File.Exists(fullPath))
				{
					return $"/Hinh/Loai/{safeFileName}";
				}
			}

			var fallbackIndex = ((maLoai ?? 1) - 1) % 3 + 1;
			return $"/amado/img/bg-img/{fallbackIndex}.jpg";
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
