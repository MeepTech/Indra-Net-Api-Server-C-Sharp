using Indra.Data;
using Meep.Tech.Data;
using Meep.Tech.Data.Configuration;
using Meep.Tech.Data.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Indra.Server {
  public class Startup {

    public Startup(IConfiguration configuration, IWebHostEnvironment env) {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services) {
      // XBam
      _initXbam();

      // Cookies
      services.ConfigureApplicationCookie(options => {
        // Cookie settings
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(5);

        options.LoginPath = "/api/user/login";
        options.AccessDeniedPath = "/";
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

    static void _initXbam() {
      Loader loader = new(new() {
        PreLoadAssemblies = new List<Assembly> {
          typeof(Place).Assembly
        }
      });
      Universe universe = new(loader, "Indra.Net");
      universe.SetExtraContext(new ModPorterContext(universe, AppDomain.CurrentDomain.BaseDirectory));
      loader.Initialize();
      JsonConvert.DefaultSettings = () => loader.Options.ModelSerializerOptions.JsonSerializerSettings;
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
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
    }
  }
}
