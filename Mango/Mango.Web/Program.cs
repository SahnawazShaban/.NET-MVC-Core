using Mango.Web.Service.IService;
using Mango.Web.Service;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;

// Creates a WebApplicationBuilder to configure and set up the app.
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Adds controllers and views support for MVC.
builder.Services.AddControllersWithViews();

// Adds the HttpContextAccessor service to access the current HTTP context.
builder.Services.AddHttpContextAccessor();

// Registers HttpClient service to enable making HTTP requests from the app.
builder.Services.AddHttpClient();

// Registers typed HttpClient for ProductService to handle product-related HTTP requests.
builder.Services.AddHttpClient<IProductService, ProductService>();

// Registers typed HttpClient for CouponService to handle coupon-related HTTP requests.
builder.Services.AddHttpClient<ICouponService, CouponService>();

// Registers typed HttpClient for CartService to handle cart-related HTTP requests.
builder.Services.AddHttpClient<ICartService, CartService>();

// Registers typed HttpClient for AuthService to handle authentication-related HTTP requests.
builder.Services.AddHttpClient<IAuthService, AuthService>();

// Registers base URLs for various APIs used within the application, retrieved from configuration settings.
SD.CouponAPIBase = builder.Configuration["ServiceUrls:CouponAPI"];
SD.OrderAPIBase = builder.Configuration["ServiceUrls:OrderAPI"];
SD.ShoppingCartAPIBase = builder.Configuration["ServiceUrls:ShoppingCartAPI"];
SD.AuthAPIBase = builder.Configuration["ServiceUrls:AuthAPI"];
SD.ProductAPIBase = builder.Configuration["ServiceUrls:ProductAPI"];

// Registers TokenProvider service with scoped lifetime for handling token-related functionality.
builder.Services.AddScoped<ITokenProvider, TokenProvider>();

// Registers BaseService with scoped lifetime to manage general operations, such as common API calls.
builder.Services.AddScoped<IBaseService, BaseService>();

// The registration for OrderService is commented out, indicating it’s either not needed now or under development.
// builder.Services.AddScoped<IOrderService, OrderService>();

// Registers ProductService with scoped lifetime to manage product-related operations.
builder.Services.AddScoped<IProductService, ProductService>();

// Registers CartService with scoped lifetime to manage shopping cart operations.
builder.Services.AddScoped<ICartService, CartService>();

// Registers AuthService with scoped lifetime to manage authentication operations.
builder.Services.AddScoped<IAuthService, AuthService>();

// Registers CouponService with scoped lifetime to manage coupon-related operations.
builder.Services.AddScoped<ICouponService, CouponService>();

// Configures cookie-based authentication using the CookieAuthenticationDefaults scheme.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Sets the expiration time for the authentication cookie to 10 hours.
        options.ExpireTimeSpan = TimeSpan.FromHours(10);

        // Specifies the login page URL where the user will be redirected if not authenticated.
        options.LoginPath = "/Auth/Login";

        // Specifies the URL to which the user is redirected if access is denied due to insufficient permissions.
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });

// Build the app.
var app = builder.Build();

// Configure the HTTP request pipeline.
// If the app is not in development mode, it configures exception handling to redirect users to a custom error page.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    // Enforces strict transport security for production, ensuring secure connections.
    app.UseHsts();
}

// Redirects all HTTP requests to HTTPS.
app.UseHttpsRedirection();

// Enables serving of static files like CSS, JavaScript, and images.
app.UseStaticFiles();

// Configures the app to use routing middleware to map incoming requests to the appropriate controllers/actions.
app.UseRouting();

// Enables authentication middleware to manage user identity.
app.UseAuthentication();

// Enables authorization middleware to enforce access control based on user roles or claims.
app.UseAuthorization();

// Maps the default controller route pattern, which means the app will direct requests to the Home controller's Index action if no other routes match.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Runs the application, starting the web server.
app.Run();
