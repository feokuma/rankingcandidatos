using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using rankingcandidatos.API.Infra.Dados;

namespace rankingcandidatos.API
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
            services.Configure<RankingCandidatosDatabaseSettings>(
                Configuration.GetSection(nameof(RankingCandidatosDatabaseSettings))
            );
            services.AddSingleton<IRankingCandidatosDatabaseSettings>(sp => 
                sp.GetRequiredService<IOptions<RankingCandidatosDatabaseSettings>>().Value
            );

            services.AddSingleton<ICandidatoRepositório, CandidatoRepositório>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
