using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.Services;
using Microsoft.EntityFrameworkCore;

DotEnvLoader.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

var builder = WebApplication.CreateBuilder(args);

var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
	?? builder.Configuration.GetConnectionString("HShop");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<Hshop2023Context>(options => {
	options.UseSqlServer(dbConnectionString);
});
builder.Services.Configure<SmtpSettings>(options =>
{
	builder.Configuration.GetSection("Smtp").Bind(options);
	options.Host = Environment.GetEnvironmentVariable("EMAIL_SMTP_HOST") ?? options.Host;
	if (int.TryParse(Environment.GetEnvironmentVariable("EMAIL_SMTP_PORT"), out var smtpPort))
	{
		options.Port = smtpPort;
	}
	options.UserName = Environment.GetEnvironmentVariable("EMAIL_SMTP_USER") ?? options.UserName;
	options.Password = Environment.GetEnvironmentVariable("EMAIL_SMTP_PASSWORD") ?? options.Password;
	options.FromEmail = Environment.GetEnvironmentVariable("EMAIL_FROM") ?? options.FromEmail;
	options.FromName = Environment.GetEnvironmentVariable("EMAIL_FROM_NAME") ?? options.FromName;
});
builder.Services.Configure<PaymentGatewaySettings>(options =>
{
	builder.Configuration.GetSection("Payments").Bind(options);
	options.VnPay.TmnCode = Environment.GetEnvironmentVariable("VNPAY_TMNCODE") ?? options.VnPay.TmnCode;
	options.VnPay.HashSecret = Environment.GetEnvironmentVariable("VNPAY_HASH_SECRET") ?? options.VnPay.HashSecret;
	options.VnPay.PaymentUrl = Environment.GetEnvironmentVariable("VNPAY_PAYMENT_URL") ?? options.VnPay.PaymentUrl;
	options.VnPay.ReturnUrl = Environment.GetEnvironmentVariable("VNPAY_RETURN_URL") ?? options.VnPay.ReturnUrl;
	options.VnPay.IpnUrl = Environment.GetEnvironmentVariable("VNPAY_IPN_URL") ?? options.VnPay.IpnUrl;
	options.MoMo.PartnerCode = Environment.GetEnvironmentVariable("MOMO_PARTNER_CODE") ?? options.MoMo.PartnerCode;
	options.MoMo.AccessKey = Environment.GetEnvironmentVariable("MOMO_ACCESS_KEY") ?? options.MoMo.AccessKey;
	options.MoMo.SecretKey = Environment.GetEnvironmentVariable("MOMO_SECRET_KEY") ?? options.MoMo.SecretKey;
	options.MoMo.ReturnUrl = Environment.GetEnvironmentVariable("MOMO_RETURN_URL") ?? options.MoMo.ReturnUrl;
});
builder.Services.Configure<AdminSecuritySettings>(options =>
{
	builder.Configuration.GetSection("AdminSecurity").Bind(options);
	options.SecretCode = Environment.GetEnvironmentVariable("ADMIN_SECRET_CODE") ?? options.SecretCode;
});
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<ICatalogQueryService, CatalogQueryService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<IShippingFeeService, ShippingFeeService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IPaymentSandboxService, PaymentSandboxService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(10);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

// https://docs.automapper.org/en/stable/Dependency-injection.html
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
