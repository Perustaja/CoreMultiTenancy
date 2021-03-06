using System;
using Cmt.Protobuf;
using CoreMultiTenancy.Api.Data;
using CoreMultiTenancy.Api.Grpc;
using CoreMultiTenancy.Api.Interfaces;
using CoreMultiTenancy.Api.Tenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;


namespace CoreMultiTenancy.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Design time connection string for migrations, connection string is overriden later if necessary
            services.AddDbContext<TenantedDbContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("DesignTimeString")));

            services.AddControllers();
            services.AddApiVersioning();
            services.AddHttpContextAccessor();
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", o =>
                {
                    o.Authority = "https://localhost:5100";
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                    };
                });
            services.AddAuthorization(o =>
            {
                o.AddPolicy("ApiScope", p =>
                {
                    p.RequireAuthenticatedUser();
                    p.RequireClaim("scope", "testapi");
                });
            });

            services.AddScoped<ITenantProvider, RouteDataTenantProvider>();
            services.AddScoped<ITenantInfrastructureManager<TenantedDbContext>, TenantInfrastructureManager<TenantedDbContext>>();
            services.AddGrpc();
            services.AddGrpcClients();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/api/error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers()
                    .RequireAuthorization("ApiScope");
                endpoints.MapGrpcInfrastructureServices();
            });
        }
    }
    public static class StartupExtensions
    {
        public static IEndpointRouteBuilder MapGrpcInfrastructureServices(this IEndpointRouteBuilder e)
        {
            e.MapGrpcService<TenantCreationService>();
            e.MapGrpcService<TenantDeletionService>();
            return e;
        }

        public static void AddGrpcClients(this IServiceCollection sc)
        {
            sc.AddGrpcClient<PermissionAuthorize.PermissionAuthorizeClient>(o =>
            {
                o.Address = new Uri("https://localhost:5100");
            });
        }
    }
}
