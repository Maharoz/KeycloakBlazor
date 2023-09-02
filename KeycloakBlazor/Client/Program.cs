global using Microsoft.AspNetCore.Components.Authorization;
using KeycloakBlazor.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace KeycloakBlazor.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            //builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            var services = builder.Services;
           RegisterHttpClient(builder, services);
           
            builder.Services.AddOidcAuthentication(options =>
            {
                
                options.ProviderOptions.MetadataUrl = "http://localhost:8080/realms/Medium/.well-known/openid-configuration";
                options.ProviderOptions.Authority = "http://localhost:8080/realms/Medium";
                options.ProviderOptions.ClientId = "medium";
                options.ProviderOptions.ResponseType = "id_token token";
                
              //  options.ProviderOptions.AdditionalProviderParameters.SaveToken = true;

            });
          //  builder.Services.AddAuthorizationCore();
            await builder.Build().RunAsync();

            static void RegisterHttpClient(
    WebAssemblyHostBuilder builder,
    IServiceCollection services)
            {
                var httpClientName = "Default";

                services.AddHttpClient(httpClientName,
                    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                        .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

                services.AddScoped(
                    sp => sp.GetRequiredService<IHttpClientFactory>()
                        .CreateClient(httpClientName));
            }
        }
    }
}