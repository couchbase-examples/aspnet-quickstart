using Couchbase.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Org.Quickstart.API.Models;

namespace Org.Quickstart.API
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

         /// <summary>
        /// dev origins used to fix CORS for local dev/qa debugging of site
        /// </summary>
        private readonly string _devSpecificOriginsName = "_devAllowSpecificOrigins";

        public Startup(
            IConfiguration configuration, 
            IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            //05/25/2021
            //fix for debugging with DEV and QA environments in GitPod
            //DO NOT APPLY to UAT and PROD environments!!!
            //https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-3.1
            //
            services.AddCors(options =>
            {
                options.AddPolicy(name: _devSpecificOriginsName,
                                  builder =>
                                  {
                                      builder.WithOrigins("https://*.gitpod.io",
                                                          "https://*.github.com",
                                                          "http://localhost:5000",
                                                          "https://localhost:5001")
                                                          .AllowAnyHeader()
                                                          .AllowAnyMethod()
                                                          .AllowCredentials();
                                  });
            });

	        //read in configuration to connect to the database
	        services.Configure<CouchbaseConfig>(Configuration.GetSection("Couchbase"));

	        //register the configuration 
	        services.AddCouchbase(Configuration.GetSection("Couchbase"));
	        services.AddHttpClient();

            services.AddControllers();

	        //customize Swagger UI
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { 
		            Title = "Couchbase Travel Sample API", 
		            Version = "v1" 
		        });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IWebHostEnvironment env, 
            IHostApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                //add cors policy
                 app.UseCors(_devSpecificOriginsName);

		        //setup swagger for debugging and testing APIs
                app.UseSwagger();
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint(
		                "/swagger/v1/swagger.json", 
		                "Couchbase Quickstart API v1"); 
                    c.RoutePrefix = string.Empty;
                    });
            }

	        if (_env.EnvironmentName == "Testing")
            {
                //add cors policy
                 app.UseCors(_devSpecificOriginsName);

                 //assume that bucket, collection, and indexes already exists due to latency in creating and async 
		    } 
            
            //remove couchbase from memory when ASP.NET closes
            appLifetime.ApplicationStopped.Register(() => {
                app.ApplicationServices
                   .GetRequiredService<ICouchbaseLifetimeService>()
                   .Close();
            });

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
