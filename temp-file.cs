using IdentityModel.AspNetCore.OAuth2Introspection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DemoCore31WebApi
{
    public class SomeHelper : Iinitializable
    {
        private readonly ILogger logger;

        public SomeHelper(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<SomeHelper>();
            logger.LogInformation("SomeHelper ctor invoked...");
        }

        public void DoSomething()
        {
            logger.LogInformation("Dosomething invoked...");
        }

        public void Initizlize()
        {
            DoSomething();
        }
    }
    public class ApplicationInitializer : IStartupFilter
    {
        
        private readonly IEnumerable<Iinitializable> iinitializables;

        public ApplicationInitializer(IEnumerable<Iinitializable> iinitializables)
        {
            
            this.iinitializables = iinitializables;
        }
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            foreach( var item in iinitializables)
            {
                item.Initizlize();
            }
            return next;
        }
    }

    public class MyRequirement : IAuthorizationRequirement
    {

    }
    public class MyAuthHandlerHandler : AuthorizationHandler<MyRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MyRequirement requirement)
        {
            var val = context.Resource;
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
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
            services.AddSingleton<IAuthorizationHandler,MyAuthHandlerHandler>();
            services.AddControllers();
            services.AddAuthentication(OAuth2IntrospectionDefaults.AuthenticationScheme)
                .AddOAuth2Introspection(options =>
               {
                   options.IntrospectionEndpoint = "https://demo.com";
                   options.Events.OnTokenValidated = (context) =>
                   {
                       var value = context.Principal;
                       return Task.CompletedTask;
                   };
               });
            services.AddHttpClient(OAuth2IntrospectionDefaults.BackChannelHttpClientName)
                .ConfigurePrimaryHttpMessageHandler( () =>
                {
                    return new MyHttpMessageHandler();
                });
            services.AddAuthorization(options =>
           {
               //// Configure the default policy
               //options.DefaultPolicy = new AuthorizationPolicyBuilder()
               //    .RequireAuthenticatedUser()
               //    .RequireClaim("SomeClaim")
               //    .Build();

               options.AddPolicy("xyz", builder =>
                {
                    builder.AddRequirements(new AssertionRequirement(authContext =>
                  {
                      return true;
                  }));
                    builder.AddRequirements(new AssertionRequirement(authContext =>
                    {
                        return true;
                    }));
                });

               options.AddPolicy("abc", builder =>
               {
                   builder.AddRequirements(new MyRequirement());
                   builder.AddRequirements(new AssertionRequirement(authContext =>
                   {
                       return true;
                   }));
                   builder.AddRequirements(new AssertionRequirement(authContext =>
                   {
                       return true;
                   }));
               });
           });

            services.AddHealthChecks();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireAuthorization("xyz");
                endpoints.MapHealthChecks("/health").RequireAuthorization("abc");
            });
        }
    }
}
