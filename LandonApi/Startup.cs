using LandonApi.Filters;
using LandonApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using LandonApi.Services;
using LandonApi.Infrastructure;

namespace LandonApi
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

            services.Configure<HotelInfo>(Configuration.GetSection("Info"));
            services.AddScoped<IRoomService, DefaultRoomService>();

            // Use in-memory database for quick dev and testing
            //TODO: Swap out for a read database in production
            var builder = new DbContextOptionsBuilder<HotelApiDbContext>();
            builder.UseInMemoryDatabase("landondb");
            services.AddDbContext<HotelApiDbContext>(options => options = builder);

            services.AddControllers();
            
            services
                .AddMvc(options =>
                {
                    options.EnableEndpointRouting = false;
                    options.Filters.Add<JsonExceptionFilter>();
                    options.Filters.Add<RequireHttpsOrCloseAttribute>();
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "LandonApi", Version = "v1" });
            });

            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ApiVersionReader = new MediaTypeApiVersionReader();
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
            });

            // remove after testing, this allows any origin to be able to connect to the APi
            services.AddCors(options =>
            {
                options.AddPolicy("Allow My App", policy => policy.AllowAnyOrigin());
            });

            services.AddAutoMapper(
                options => options.AddProfile<MappingProfile>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "LandonApi v1"));
            }
            else
            {
                app.UseHsts();
            }
            app.UseCors("Allow My App");
            app.UseMvc();
        }
    }
}
