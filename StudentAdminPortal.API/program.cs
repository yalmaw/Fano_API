
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using StudentAdminPortal.API.DataModels;
using StudentAdminPortal.API.Repositories;
using System.IO;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Http; // For IFormFile

// Create a WebApplicationBuilder instance.
// This is the entry point for configuring and building the host in .NET 7+.
var builder = WebApplication.CreateBuilder(args);

// Add services to the container (equivalent to ConfigureServices in Startup.cs)

// Configure CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("angularApplication", policyBuilder =>
    {
        policyBuilder.WithOrigins("http://localhost:4200") // Allow requests from this origin
                     .AllowAnyHeader()                     // Allow any headers
                     .WithMethods("GET", "POST", "PUT", "DELETE") // Allow specified HTTP methods
                     .WithExposedHeaders("*");              // Expose all headers
    });
});

// Add controllers to the service collection
builder.Services.AddControllers();

// Add FluentValidation for request validation
// Registers validators from the assembly containing 'Program' (which replaces 'Startup')
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Program>());

// Configure SQL Server database context
builder.Services.AddDbContext<StudentAdminContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StudentAdminPortalDb")));

// Register repositories for dependency injection
builder.Services.AddScoped<IStudentRepository, SqlStudentRepository>();
builder.Services.AddScoped<IImageRepository, LocalStorageImageRepository>();
//builder.Services.AddScoped<IFileRepository, FileRepository>();   

// Configure Swagger/OpenAPI for API documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "StudentAdminPortal.API", Version = "v1" });
});

// Add AutoMapper for object mapping
// Scans the assembly containing 'Program' for AutoMapper profiles
builder.Services.AddAutoMapper(typeof(Program).Assembly);


// Build the WebApplication instance
var app = builder.Build();

// Configure the HTTP request pipeline (equivalent to Configure in Startup.cs)

// Check if the application is running in Development environment
if (app.Environment.IsDevelopment())
{
    // Use developer exception page for detailed error information
    app.UseDeveloperExceptionPage();
    // Enable Swagger UI for API testing and documentation
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "StudentAdminPortal.API v1"));
}

// Enforce HTTPS redirection
app.UseHttpsRedirection();

// Serve static files from the "Resources" folder
// This allows access to uploaded images, for example.
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "Resources")),
    RequestPath = "/Resources" // The URL path to access these static files (e.g., /Resources/image.jpg)
});

// Enable routing for incoming requests
app.UseRouting();

// Apply the configured CORS policy named "angularApplication"
app.UseCors("angularApplication");

// Enable authorization middleware
app.UseAuthorization();

// Map controller endpoints
// In .NET 7+, MapControllers() is typically called directly on the app instance.
app.MapControllers();

// Run the application
app.Run();

