using System.ComponentModel.DataAnnotations;

namespace ECommerceMVC.ViewModels
{
	public class RegisterVM
	{
		[Key]
		[Display(Name = "Tên đăng nhập")]
		[Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
		[MaxLength(20, ErrorMessage = "Tên đăng nhập tối đa 20 ký tự")]
		public string MaKh { get; set; } = string.Empty;


		[Display(Name ="Mật khẩu")]
		[Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
		[DataType(DataType.Password)]
		[MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
		public string MatKhau { get; set; } = string.Empty;

		[Display(Name ="Họ tên")]
		[Required(ErrorMessage = "Vui lòng nhập họ tên")]
		[MaxLength(50, ErrorMessage = "Họ tên tối đa 50 ký tự")]
		public string HoTen { get; set; } = string.Empty;

		public bool GioiTinh { get; set; } = true;

		[Display(Name ="Ngày sinh")]
		[DataType(DataType.Date)]
		[Required(ErrorMessage = "Vui lòng chọn ngày sinh")]
		public DateTime? NgaySinh { get; set; }

		[Display(Name ="Địa chỉ")]
		[Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
		[MaxLength(60, ErrorMessage = "Địa chỉ tối đa 60 ký tự")]
		public string DiaChi { get; set; } = string.Empty;

		[Display(Name = "Điện thoại")]
		[Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
		[MaxLength(24, ErrorMessage = "Điện thoại tối đa 24 ký tự")]
		[RegularExpression(@"0[9875]\d{8}", ErrorMessage ="Số điện thoại chưa đúng định dạng di động Việt Nam")]
		public string DienThoai { get; set; } = string.Empty;


		[Required(ErrorMessage = "Vui lòng nhập email")]
		[EmailAddress(ErrorMessage ="Email chưa đúng định dạng")]
		public string Email { get; set; } = string.Empty;

		public string? Hinh { get; set; }
	}
}
