using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Backend.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Backend.Controllers;
using Backend.Services;
using Microsoft.AspNetCore.Cors;

namespace Backend
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
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                };
            });
            services.AddCors(options =>
            {
                options.AddPolicy(name: "_myAllowSpecificOrigins",
                                  policy =>
                                  {
                                      policy.WithOrigins("*");
                                  });
            });
            services.AddMvc();
            services.AddDbContext<BackendContext>(options => options.UseSqlServer("Server=localhost;Database=Licenta;Trusted_Connection=True;"));
            services.AddControllersWithViews();
            services.AddControllers().AddNewtonsoftJson(options =>
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );
            services.AddControllers();
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IListingService, ListingService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IRepositoryManager, RepositoryManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAuthentication();
            
            if (env.IsDevelopment())
            {
                //app.UseCors("CorsPolicy");
                app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            // app.UseCors("CorsPolicy");
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
