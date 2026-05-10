using CareNote.Services;
using Microsoft.AspNetCore.Authentication.Cookies;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();


// Registrerar NoteRepository för dependency injection
builder.Services.AddScoped<INoteRepository, NoteRepository>();


// Konfigurerar Cookie-baserad autentisering
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });
builder.Services.AddAuthorization(); // Aktiverar Authorization


// Registrerar HttpClient för KnowledgeService (Typed Client)
builder.Services.AddHttpClient<IKnowledgeService, KnowledgeService>();


// Registrerar andra tjänster
builder.Services.AddSingleton<FirebaseAuthService>();
builder.Services.AddHttpClient<GroqAIService>();
builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddSingleton(sp => // GroqAIService registreras med API-nyckel från konfiguration
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var apiKey = builder.Configuration["Groq:ApiKey"] ?? string.Empty;
    return new GroqAIService(httpClient, apiKey);
});


var app = builder.Build();


// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();