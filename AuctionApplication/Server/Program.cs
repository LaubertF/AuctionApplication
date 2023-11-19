using AuctionApplication.Database;
using AuctionApplication.Server.Business;
using AuctionApplication.Server.Hubs;
using AuctionApplication.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<DbContext,Context>();
builder.Services.AddScoped<EfRepository<Auction>>();
builder.Services.AddScoped<EfRepository<User>>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuctionService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, c =>
    {
        c.Authority = $"{builder.Configuration["Auth0:Domain"]}";
        c.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidAudience = builder.Configuration["Auth0:Audience"],
            ValidIssuer = $"{builder.Configuration["Auth0:Domain"]}",
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole",
        policy => policy.RequireClaim("https://my-app.example.com/roles", "Admin"));
});

var app = builder.Build();
app.UseResponseCompression();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapSwagger();
app.UseSwaggerUI();
app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");
app.MapHub<BidHub>("/chathub");
app.Run();