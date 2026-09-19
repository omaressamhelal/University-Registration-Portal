using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// Configure the named HttpClient with your API's Base Address
// ==========================================================
builder.Services.AddHttpClient("RegistrationApi", client =>
{
    // 🌟 FIX: Changed 'localhost' to '127.0.0.1' to eliminate the 21-second IPv6 timeout delay
    client.BaseAddress = new Uri("https://127.0.0.1:7126/");
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // 🌟 THE FIX: Changing the cookie name forces the browser to destroy the old, broken session!
        options.Cookie.Name = "UniPortal_Auth_V2";

        options.LoginPath = "/Login/Index"; // Updated to guarantee it routes to your exact login folder
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/AccessDenied"; // Where to send them if they try to open the wrong portal
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;

        // 🛡️ High-Security Production Cookie Configurations
        options.Cookie.HttpOnly = true;             // Prevents JavaScript from reading the cookie
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Requires HTTPS
        options.Cookie.SameSite = SameSiteMode.Strict;  // Prevents Cross-Site Request Forgery
    });

// 🛡️ Restored Role-Based Security Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("InstructorOnly", policy => policy.RequireRole("Instructor", "Admin"));
    options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));

    // Allows Students, Instructors, and Admins to access student pages
    options.AddPolicy("StudentOrInstructor", policy => policy.RequireRole("Student", "Instructor", "Admin"));
});

builder.Services.AddRazorPages(options =>
{
    // Root redirect to Login page
    options.Conventions.AddPageRoute("/Login/Index", "");

    // 🛡️ Restored Secure Admin Folders
    options.Conventions.AuthorizeFolder("/AdminDashboard", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Students", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Instructors", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Courses", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Departments", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Semesters", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Enrollments", "AdminOnly");

    // 🛡️ Restored Secure Instructor Folders
    options.Conventions.AuthorizeFolder("/InstructorDashboard", "InstructorOnly");
    options.Conventions.AuthorizeFolder("/InstructorCourses", "InstructorOnly");

    // 🛡️ Restored Secure Student Folders
    options.Conventions.AuthorizeFolder("/StudentDashboard", "StudentOrInstructor");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// 🛡️ Restored Security Response Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseRouting();

// 🌟 CRITICAL FIX: You were missing UseAuthentication! It MUST come before UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();