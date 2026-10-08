using Polly;
using Microsoft.AspNetCore.RateLimiting; 
using System.Threading.RateLimiting; 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("SearchLimiter", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(10); 
        opt.PermitLimit = 3;                 
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;                   
    });
    options.RejectionStatusCode = 429; 
});

var apiBaseUrl = builder.Configuration["ApiSettings:FlightApiBaseUrl"];

builder.Services.AddHttpClient("FlightApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddTransientHttpErrorPolicy(policyBuilder =>
    policyBuilder.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1))))
.AddTransientHttpErrorPolicy(policyBuilder =>
    policyBuilder.CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseRateLimiter();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();