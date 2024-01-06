using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    _ = endpoints.MapRazorPages();
});

// Open the default web browser in non-development mode
if (!app.Environment.IsDevelopment())
{
    var url = "http://localhost:5000"; // Change this URL as needed
    Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
}

app.Run();
