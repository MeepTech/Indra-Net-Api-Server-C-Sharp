using Indra.Api.Configuration;
using Indra.Net.Focuses.Actors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;

namespace Indra.Api {
  public class Startup {

    public Startup(IConfiguration configuration, IWebHostEnvironment env) {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services) {
      // Cookies
      services.ConfigureApplicationCookie(options => {
        // Cookie settings
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(5);

        options.LoginPath = "/login";
        options.AccessDeniedPath = "/denied";
        options.SlidingExpiration = true;
      });

      // API
      services.AddControllers()
        .AddNewtonsoftJson();

      // Swagger
      services.AddSwaggerGen(c
        => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Indra.Server", Version = "v1" }));
      services.AddSwaggerGenNewtonsoftSupport();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DbContext dbContext) {
      if (env.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(options => {
          options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
          options.RoutePrefix = "swagger";
          options.EnableDeepLinking();
        });
      }

      app.UseHttpsRedirection();

      app.UseRouting();

      app.UseEndpoints(endpoints => {
        endpoints.MapControllers();
      });

      Server.Initialize("Test-Server-A", "localhost:8080", dbContext);
    }
  }
}
