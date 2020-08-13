using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace Vecc.HealthCheckExample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddHealthChecks()
                .AddCheck("ping", () => new HealthCheckResult(HealthStatus.Healthy, "pong"), new string[] { "ping" })
                .AddCheck("remote1", () => new HealthCheckResult(HealthStatus.Healthy, "always healthy"), new string[] { "remote" })
                .AddCheck("remote2", () => new HealthCheckResult(HealthStatus.Degraded, "always degraded"), new string[] { "remote" })
                .AddCheck("remote3", () => new HealthCheckResult(HealthStatus.Unhealthy, "always unhealthy"), new string[] { "remote" });

            services.AddHealthChecksUI((options)=>
            {
                options.AddHealthCheckEndpoint("ping", "/hc/ping");
                options.AddHealthCheckEndpoint("remote", "/hc/remote");
            })
                .AddInMemoryStorage();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/hc/ping", new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("ping"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecks("/hc/remote", new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("remote"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecksUI();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
