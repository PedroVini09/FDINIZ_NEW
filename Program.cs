using DotNetEnv;
using FDINIZ_Lab.Models;
using FDINIZ_Lab.Services;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Configuração do e-mail
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings")
);

// Serviço de e-mail
builder.Services.AddScoped<IEmailService, EmailService>();

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();