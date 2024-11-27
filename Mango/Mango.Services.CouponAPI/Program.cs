using AutoMapper; // Using AutoMapper for object mapping
using Mango.Services.CouponAPI; // Custom namespaces for the project
using Mango.Services.CouponAPI.Data; // Data context of the Coupon API
using Mango.Services.CouponAPI.Extensions; // Custom extension methods
using Microsoft.AspNetCore.Authentication.JwtBearer; // For JWT authentication
using Microsoft.EntityFrameworkCore; // Entity Framework Core for working with databases
using Microsoft.OpenApi.Models; // For configuring Swagger and OpenAPI

var builder = WebApplication.CreateBuilder(args); // Create a builder for configuring the app

// Add services to the container.

// Registering the AppDbContext and configuring it to use SQL Server
builder.Services.AddDbContext<AppDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")); // Connection string from configuration
});

// Register AutoMapper and its configurations
IMapper mapper = MappingConfig.RegisterMaps().CreateMapper(); // Registering mappings
builder.Services.AddSingleton(mapper); // Adding AutoMapper as a singleton service
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies()); // Automatically load AutoMapper profiles from assemblies

builder.Services.AddControllers(); // Adding controller support to handle HTTP requests

// Configure Swagger/OpenAPI to document the API
builder.Services.AddEndpointsApiExplorer(); // Add API explorer for Swagger

builder.Services.AddSwaggerGen(option =>
{
    // Adding JWT Bearer authentication definition in Swagger
    option.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme, securityScheme: new OpenApiSecurityScheme
    {
        Name = "Authorization", // Name of the authorization field
        Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`", // Description in Swagger UI
        In = ParameterLocation.Header, // JWT token will be passed in the header
        Type = SecuritySchemeType.ApiKey, // The type of security is API Key
        Scheme = "Bearer" // The scheme is Bearer
    });

    // Adding security requirements for using JWT Bearer tokens
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference= new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme, // Reference to the security scheme
                    Id=JwtBearerDefaults.AuthenticationScheme // Reference ID for JWT
                }
            }, new string[]{} // No specific scopes required
        }
    });
});

// Custom method to add authentication logic
builder.AddAppAuthetication(); // Register custom authentication method

builder.Services.AddAuthorization(); // Adding authorization service

var app = builder.Build(); // Building the app

// Configure the HTTP request pipeline.
app.UseSwagger(); // Enable Swagger middleware
app.UseSwaggerUI(c =>
{
    // Swagger UI for production environment
    if (!app.Environment.IsDevelopment())
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cart API"); // Set the Swagger endpoint
        c.RoutePrefix = string.Empty; // Set the Swagger UI route to the root
    }
});

// Use Stripe API key from configuration (commented out for now)
//Stripe.StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
app.UseAuthentication(); // Enable authentication middleware
app.UseAuthorization(); // Enable authorization middleware

app.MapControllers(); // Map controller routes

ApplyMigration(); // Apply database migrations if there are pending migrations

app.Run(); // Run the application

// Method to apply any pending database migrations
void ApplyMigration()
{
    using (var scope = app.Services.CreateScope()) // Create a service scope
    {
        var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); // Get the database context

        if (_db.Database.GetPendingMigrations().Count() > 0) // Check if there are any pending migrations
        {
            _db.Database.Migrate(); // Apply the pending migrations
        }
    }
}
