# 🛋️ DEEPSEARCH — Cổng Thương Mại Điện Tử Nội Thất Luxury & Showroom 3D

Chào mừng bạn đến với **DEEPSEARCH**, cổng thương mại điện tử mua sắm nội thất cao cấp mang phong cách thiết kế tạp chí (editorial aesthetic) sang trọng, kết hợp với các công nghệ tương tác 3D chân thực và hệ thống quản trị dữ liệu kinh doanh tối tân. Dự án được phát triển trên nền tảng **ASP.NET Core 8.0 MVC (C#)**.

---

## 📖 Giới thiệu Dự án
**DEEPSEARCH** không chỉ là một trang web bán hàng thông thường mà là một tác phẩm nghệ thuật số hóa dành cho không gian sống hiện đại. Lấy cảm hứng từ các tạp chí nội thất Bắc Âu và Ý, dự án tối ưu hóa trải nghiệm khách hàng thông qua ngôn ngữ thiết kế tối giản, tông màu ấm áp sang trọng (Beige, Walnut, Sand Gold) kết hợp với **Showroom 3D nội thất tương tác trực quan**, giúp khách hàng quan sát tỉ mỉ chất liệu, ánh sáng và tỉ lệ thực tế của sản phẩm trước khi đưa ra quyết định mua hàng.

Đồng thời, hệ thống cung cấp một **Bảng điều khiển Admin toàn năng**, hiển thị các đồ thị doanh thu chuyển sắc thời gian thực, quản lý phân hệ catalog đa cấp, kiểm soát cổng thanh toán trực tuyến và giám sát trạng thái đơn hàng một cách trực quan.

---

## 🛠️ Kiến trúc & Công nghệ Sử dụng

Dự án áp dụng các công nghệ hiện đại ở cả hai phía Client-side và Server-side để đảm bảo hiệu năng tối ưu, khả năng bảo mật cao và trải nghiệm người dùng tuyệt vời:

### 1. Backend & Dịch vụ Hệ thống
*   **Core Framework**: `ASP.NET Core 8.0 MVC` (C#) đem lại cấu trúc phân tách rõ ràng (Model - View - Controller), khả năng mở rộng cao và hiệu năng xử lý mạnh mẽ.
*   **Database ORM**: `Entity Framework Core` tích hợp với `SQL Server/LocalDB`, kết nối và xử lý dữ liệu thông qua cơ chế `Hshop2023Context`.
*   **Security & Hashing**: 
    *   Mã hóa mật khẩu bằng thuật toán PBKDF2/SHA-256 an toàn thông qua dịch vụ `IPasswordService`.
    *   Cơ chế xác thực dựa trên Cookie-Session an toàn với cờ `HttpOnly` giúp ngăn chặn triệt để tấn công XSS/CSRF.
    *   Hệ thống phục hồi mật khẩu bảo mật bằng liên kết Token OTP gửi trực tiếp qua email.
*   **Email Service**: Gửi thư tự động qua SMTP (`SmtpEmailService`) dùng để xác nhận đơn hàng, chào mừng và đặt lại mật khẩu.
*   **AutoMapper**: Chuẩn hóa việc truyền tải dữ liệu giữa các thực thể Entity và ViewModel thông qua `AutoMapperProfile`.

### 2. Frontend & Trải nghiệm Người dùng
*   **Design Tokens & Styling**: 
    *   CSS thuần (`Vanilla CSS`) đem lại khả năng kiểm soát giao diện tối đa, không phụ thuộc vào Tailwind, được viết tại `modern-theme.css` và `modern-animations.css`.
    *   Ứng dụng thiết kế **Glassmorphism** (kính mờ), bo góc tinh tế (`border-radius: 1.2rem`), cùng hiệu ứng lướt sáng (gradient hover) và các chuyển động micro-animations mềm mại.
*   **Typography**: Sử dụng cặp font chữ cao cấp từ Google Fonts: **Cormorant Garamond** (font chữ serif quý phái dành cho các tiêu đề chính) và **Manrope** (font chữ sans-serif hiện đại, rõ nét dành cho nội dung và dữ liệu số).
*   **Interactive 3D**: Tích hợp module **Showroom 3D** chất lượng cao sử dụng WebGL/Model-Viewer cho phép xoay, thu phóng và tương tác mô hình ghế sofa, tủ nội thất ngay trên giao diện web.
*   **Analytics Charts**: Sử dụng thư viện `Chart.js` truyền tải dữ liệu phân tích trực quan với màu sắc chuyển sắc (gradient).

### 3. Tích hợp Cổng thanh toán & Vận chuyển
*   **Online Gateways**: Hỗ trợ hai cổng thanh toán phổ biến nhất Việt Nam là **VNPAY** và **MOMO** (với cơ chế IPN và Sandbox thử nghiệm).
*   **Shipping System**: Tự động tính toán phí vận chuyển thông minh (`IShippingFeeService`) dựa trên địa chỉ và hình thức giao hàng.
*   **Voucher Engine**: Quản lý và áp dụng mã giảm giá tự động theo điều kiện hóa đơn (`IVoucherService`).

---

## ✨ Các Tính năng Nổi bật

### 🌌 1. Showroom 3D Tương Tác Trực Quan
*   Khách hàng có thể khám phá sản phẩm nội thất trong một không gian giả lập Villa cao cấp.
*   Hỗ trợ xoay góc nhìn camera 360 độ, cuộn để zoom cận cảnh chi tiết thớ gỗ, sợi vải, da thuộc.
*   Trải nghiệm mượt mà được tích hợp ngay trên trang sản phẩm mà không cần cài đặt thêm bất kỳ ứng dụng nào.

### 🎨 2. Đồng bộ màu sắc Theme Luxury (Editorial Aesthetic)
*   Sự phối hợp hài hòa giữa các dải màu ấm: Beige đất ấm, màu cát, màu nâu hạt dẻ Chestnut sâu lắng và điểm xuyết các đường vân viền hạt dẻ tinh xảo.
*   Giao diện trang Admin hoàn toàn đồng bộ màu sắc với trang người dùng khách hàng, loại bỏ cảm giác thô cứng thường thấy ở các hệ thống CMS cũ.

### 📈 3. Bảng Quản trị Admin Toàn Năng (Premium Dashboard)
*   **KPI Hero Cards**: Thống kê doanh thu, đơn hàng, khách hàng và danh mục sản phẩm dưới dạng các thẻ kính mờ (glassmorphic) nổi bật.
*   **Interactive Charts (Chart.js)**:
    *   *Doanh thu 30 ngày*: Đồ thị diện tích (Area Line Chart) với đường cong mềm mại và dải màu sáng vàng đồng sang trọng.
    *   *Doanh thu 12 tháng*: Đồ thị cột đứng (Rounded Bar Chart) bo tròn các góc tinh tế.
*   **Ranking Medals**: Bảng xếp hạng Top 8 sản phẩm bán chạy nhất kèm huy chương mạ vàng, mạ bạc và đồng lấp lánh.
*   **Stock Warning & Progress Bar**: Giám sát tồn kho dưới dạng thanh tiến trình trực quan, cảnh báo khẩn cấp các sản phẩm sắp hết hàng.

### 🧑‍💼 4. Quản lý Hồ sơ & Tải Ảnh Đại Diện AJAX Tức Thì
*   Chức năng cập nhật avatar khách hàng được tối ưu hóa bằng **AJAX**. Hình ảnh được tải lên tức thì khi người dùng chọn tệp mà không cần tải lại trang.
*   Đồng bộ hóa ảnh đại diện ngay lập tức lên Navbar khách hàng và Topbar Admin trong thời gian thực.
*   Cơ chế Fallback thông minh: Tự động tạo biểu tượng hình đại diện bằng chữ cái đầu (ví dụ: "TA") trên nền màu đồng lộng lẫy khi người dùng chưa thiết lập avatar.

---

## 📁 Cấu trúc Thư mục Dự án

```text
f:\ECommerceMVC\
│
├── ECommerceMVC/                        # Thư mục chứa mã nguồn chính của ứng dụng ASP.NET MVC
│   ├── Controllers/                     # Thư mục chứa các Controllers xử lý logic nghiệp vụ
│   │   ├── AdminController.cs           # Quản lý tất cả hoạt động phía Admin (Sản phẩm, Đơn hàng, Biểu đồ)
│   │   ├── CartController.cs            # Xử lý giỏ hàng, đặt hàng, voucher, và tích hợp thanh toán Momo, VNPay
│   │   ├── HangHoaController.cs         # Cửa hàng duyệt sản phẩm, phân trang, lọc tìm kiếm phía khách hàng
│   │   ├── KhachHangController.cs       # Đăng nhập, đăng ký, quên mật khẩu, cập nhật hồ sơ AJAX
│   │   └── ShowroomController.cs        # Điều hướng và hiển thị showroom nội thất 3D
│   │
│   ├── Data/                            # Entity Framework DbContext và các thực thể Database
│   │   └── Hshop2023Context.cs          # Quản lý kết nối SQL Server và ánh xạ các bảng CSDL
│   │
│   ├── Services/                        # Các lớp xử lý dịch vụ nghiệp vụ (Business Service Layer)
│   │   ├── IEmailService.cs             # Dịch vụ gửi email thông báo đơn hàng & khôi phục mật khẩu
│   │   ├── IStockService.cs             # Kiểm soát số lượng hàng tồn kho và cảnh báo ngưỡng an toàn
│   │   ├── IVoucherService.cs           # Kiểm tra và áp dụng mã giảm giá hóa đơn
│   │   └── IVnPayService.cs             # Tạo chuỗi thanh toán và xác thực chữ ký số VNPay IPN
│   │
│   ├── ViewModels/                      # Các đối tượng DTO và ViewModel phục vụ hiển thị dữ liệu
│   │
│   ├── Views/                           # Các tệp giao diện Razor View (.cshtml)
│   │   ├── Admin/                       # Giao diện Bảng điều khiển, Sản phẩm, Đơn hàng, Khách hàng phía Admin
│   │   ├── Shared/                      # Các Layout dùng chung (_LayoutCustomer, _AdminLayout) và Partials
│   │   └── Showroom/                    # Tệp View hiển thị showroom mô phỏng 3D
│   │
│   └── wwwroot/                         # Tài nguyên tĩnh được tải lên Web Server
│       ├── css/                         # Thư mục chứa style chủ đạo (modern-theme.css, modern-animations.css)
│       ├── Hinh/                        # Thư mục lưu trữ hình ảnh sản phẩm và avatar khách hàng tải lên
│       └── showroom3d/                  # Tệp mã nguồn HTML5, mô hình GLTF/GLB của không gian 3D
│
├── .env                                 # Tệp cấu hình biến môi trường cục bộ (Database, SMTP, VNPay, Momo)
└── ECommerceMVC.sln                     # Tệp Solution chính của Visual Studio
```

---

## ⚙️ Hướng dẫn Cài đặt & Chạy Dự án

Để chạy ứng dụng trên máy local của bạn, vui lòng làm theo các bước hướng dẫn chi tiết sau đây:

### Bước 1: Khởi tạo Cơ sở Dữ liệu SQL Server
1.  Đảm bảo máy tính của bạn đã cài đặt **SQL Server** (LocalDB hoặc Express) và **SQL Server Management Studio (SSMS)**.
2.  Tiến hành khôi phục CSDL hoặc tạo cơ sở dữ liệu mới với tên `HShop2023` dựa trên cấu trúc các thực thể dữ liệu trong mã nguồn.

### Bước 2: Thiết lập Tệp cấu hình `.env`
1.  Sao chép tệp cấu hình mẫu `.env.example` thành tệp `.env` nằm ở thư mục gốc của dự án (`f:\ECommerceMVC\ECommerceMVC\.env`).
2.  Mở tệp `.env` và điền đầy đủ các thông tin kết nối và API Key của bạn:

```env
# Chuỗi kết nối CSDL SQL Server của bạn
DB_CONNECTION_STRING=Server=YOUR_SERVER_NAME;Database=HShop2023;Trusted_Connection=True;TrustServerCertificate=True

# Thiết lập tài khoản gửi Email SMTP (Ví dụ: Gmail App Password)
EMAIL_SMTP_HOST=smtp.gmail.com
EMAIL_SMTP_PORT=587
EMAIL_SMTP_USER=your_email@gmail.com
EMAIL_SMTP_PASSWORD=your_gmail_app_password
EMAIL_FROM=your_email@gmail.com
EMAIL_FROM_NAME=DEEPSEARCH Luxury Interior

# Cấu hình cổng thanh toán thử nghiệm VNPAY
VNPAY_TMNCODE=YOUR_VNPAY_TMNCODE
VNPAY_HASH_SECRET=YOUR_VNPAY_HASH_SECRET
VNPAY_CALLBACK_URL=https://localhost:7148/Cart/PaymentCallBackVnPay

# Mã bảo mật đặc quyền cấp quyền quản trị
ADMIN_SECRET_CODE=LuxuryDeepsearchAdminPasscode2026
```

### Bước 3: Build và Chạy Dự án bằng Command Line
Mở PowerShell hoặc Command Prompt tại thư mục chứa file Solution (`f:\ECommerceMVC`), thực thi chuỗi lệnh sau:

```bash
# 1. Khôi phục các gói thư viện NuGet phụ thuộc
dotnet restore

# 2. Biên dịch toàn bộ dự án
dotnet build

# 3. Di chuyển vào thư mục dự án web và khởi chạy ứng dụng
cd ECommerceMVC
dotnet run
```

Sau khi ứng dụng khởi chạy thành công, trình điều khiển console sẽ hiển thị địa chỉ local host. Thông thường là:
*   🌐 **Địa chỉ truy cập**: [https://localhost:7148](https://localhost:7148)

---

## 💡 Các Giải pháp Kỹ thuật & Tối ưu hóa nổi bật

Trong quá trình phát triển dự án, nhiều bài toán kỹ thuật phức tạp đã được xử lý một cách thông minh:

### 1. Giải pháp Tải ảnh đại diện tức thì không dính Lỗi ModelState Validation
*   **Thách thức**: Khi người dùng tải ảnh đại diện trực tiếp trên biểu mẫu Hồ sơ cá nhân (`EditProfile.cshtml`), cơ chế ASP.NET MVC tự động kiểm tra `ModelState` của toàn bộ các trường thông tin trong form (như mật khẩu, số điện thoại, địa chỉ). Nếu các trường đó chưa được điền đầy đủ, quá trình upload avatar sẽ bị từ chối.
*   **Giải pháp**: Thiết kế một biểu mẫu AJAX ẩn độc lập dành riêng cho trường tải tệp tin ảnh đại diện. Khi tệp được chọn, một đoạn script Javascript thuần sẽ đóng gói tệp bằng `FormData` và gửi đến một Action đặc thù `UploadAvatarJson`. Cách tiếp cận này loại bỏ hoàn toàn ràng buộc validate của form chính, lưu tệp thành công, cập nhật ngay lập tức ảnh trên giao diện Navbar khách hàng và Topbar Admin mà không cần tải lại toàn bộ trang.

### 2. Giải pháp Render Biểu đồ Chart.js tốc độ cực nhanh
*   **Thách thức**: Gọi API liên tục để vẽ biểu đồ thống kê phía Client sẽ làm giảm hiệu năng tải trang Admin Dashboard và gây trễ trải nghiệm.
*   **Giải pháp**: Dữ liệu thống kê được nạp trực tiếp vào View Model `AdminDashboardVM` từ tầng Service. Tại Razor View, dữ liệu C# List được tuần tự hóa trực tiếp sang cấu trúc JSON Array an toàn bằng cú pháp `@Html.Raw(Json.Serialize(Model.DailyRevenue.Select(...)))`. Thư viện Chart.js phía Client-side sẽ nhận được dữ liệu thô ngay lập tức khi trang vừa render xong, vẽ biểu đồ mượt mà dưới dạng các nét gradient quyến rũ mà không phải chờ đợi phản hồi của bất kỳ API bất đồng bộ nào.

### 3. Tối ưu hóa Tốc độ Tải Showroom 3D
*   Mô hình 3D (.gltf/.glb) được tối ưu hóa nén kết cấu (texture mapping) và tải bất đồng bộ (`loading="lazy"`) thông qua thẻ `<iframe>`, giúp trang web chính vẫn tải nhanh chóng mà không bị block bởi dung lượng tệp đồ họa lớn của showroom.

---

Chúc bạn có một trải nghiệm mua sắm và quản trị sang trọng và mượt mà cùng **DEEPSEARCH**! 🛋️✨
