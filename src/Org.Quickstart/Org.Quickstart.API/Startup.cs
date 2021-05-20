using Couchbase.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Org.Quickstart.API.Models;
using Org.Quickstart.API.Services;

namespace Org.Quickstart.API
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
	        services.Configure<CouchbaseConfig>(Configuration.GetSection("Couchbase"));
	        services.AddCouchbase(Configuration.GetSection("Couchbase"));
	        services.AddHttpClient();
            services.AddTransient<DatabaseService>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { 
		            Title = "Couchbase Quickstart API", 
		            Version = "v1" 
		        });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

		        //setup swagger for debugging and testing APIs
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint(
		            "/swagger/v1/swagger.json", 
		            "Couchbase Quickstart API v1"
		        ));
            }

	        //setup the database once everything is setup and running
	        appLifetime.ApplicationStarted.Register(async () => {
		        var db = app.ApplicationServices.GetService<DatabaseService>();
		        await db.SetupDatabase();
	        });

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
