using CourseLibrary.API.DbContexts;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using System;

namespace CourseLibrary.API
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
            services.AddControllers(setupAction =>
            {
                setupAction.ReturnHttpNotAcceptable = true;
                //setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
            })
                 .AddNewtonsoftJson(setupAction =>
                 {
                     setupAction.SerializerSettings.ContractResolver =
                     new CamelCasePropertyNamesContractResolver();
                 })
                 .AddXmlDataContractSerializerFormatters()
                 .ConfigureApiBehaviorOptions(setupAction =>
                 {
                     //create a proble details object
                     setupAction.InvalidModelStateResponseFactory = context =>
                     {
                         var problemDetailsFactory = context.HttpContext.RequestServices
                         .GetRequiredService<ProblemDetailsFactory>();
                         var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(
                             context.HttpContext,
                             context.ModelState);

                     //add additional info not added by default
                     problemDetails.Detail = "See the errors fields for details.";
                         problemDetails.Instance = context.HttpContext.Request.Path;

                     //add status code
                     var actionExecutionContext = context as Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext;
                         if ((context.ModelState.ErrorCount > 0) &&
                             (actionExecutionContext?.ActionArguments.Count ==
                             context.ActionDescriptor.Parameters.Count))
                         {
                             problemDetails.Type = "https://courselibrary.com/modelvalidationproblem";
                             problemDetails.Status = StatusCodes.Status422UnprocessableEntity;
                             problemDetails.Title = "One or more validation errors occured.";

                             return new UnprocessableEntityObjectResult(problemDetails)
                             {
                                 ContentTypes = { "application/problem+json" }
                             };
                         };
                         problemDetails.Status = StatusCodes.Status400BadRequest;
                         problemDetails.Title = "One or more validation errors occured.";

                         return new UnprocessableEntityObjectResult(problemDetails)
                         {
                             ContentTypes = { "application/problem+json" }
                         };
                     };
                 });

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddScoped<ICourseLibraryRepository, CourseLibraryRepository>();

            services.AddDbContext<CourseLibraryContext>(options =>
            {
                options.UseSqlServer(
                    @"Server=(localdb)\mssqllocaldb;Database=CourseLibraryDB;Trusted_Connection=True;");
            }); 
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
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("Unexpected falt happened. Try again later.");
    
                    });
                });
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
