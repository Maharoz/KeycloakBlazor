
using Microsoft.AspNetCore.ResponseCompression;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace KeycloakBlazor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            // Load the X.509 certificate from a file or any other source
            X509Certificate2 certificate = new X509Certificate2("KeyCloakRealm.Public.crt");

            // Get the RSA public key from the certificate (RSACng)
            RSACng rsaPublicKey = (RSACng)certificate.PublicKey.Key;

            builder.Services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = false;
                //  options.MetadataAddress = "http://localhost:8080/realms/Medium/.well-known/openid-configuration";
                options.Authority = "http://localhost:8080/realms/Medium";
                options.IncludeErrorDetails = true;
                options.Audience = "account";
                options.SaveToken =true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = "http://localhost:8080/realms/Medium",
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = new RsaSecurityKey(rsaPublicKey),
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json; charset=utf-8";
                        var message = context.Exception.ToString(); // Environment.development ?  : "An error occurred processing your authentication.";
                        var result = JsonConvert.SerializeObject(new { message });
                        return context.Response.WriteAsync(result);
                    }
                };
            });

            var app = builder.Build();

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
            app.MapRazorPages();
            app.MapControllers();
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}