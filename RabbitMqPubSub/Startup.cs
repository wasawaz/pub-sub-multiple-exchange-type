using System;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using RabbitMqPubSub.Commons.Dispatchers;
using RabbitMqPubSub.Commons.RabbitMq;
using RabbitMqPubSub.Commons.Swaggers;
using RabbitMqPubSub.Messages;
using RabbitMqPubSub.Messages.Events;
using RabbitMqPubSub.Messages.Events.Subscribe;
using RawRabbit.Configuration.Exchange;

namespace RabbitMqPubSub
{
    public class Startup
    {
        public IContainer Container { get; private set; }

        private readonly IHostingEnvironment env;

        public Startup(IHostingEnvironment env)
        {
            this.env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            if (env.IsDevelopment())
            {
                services.AddAppSwagger();
            }

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var builder = new ContainerBuilder();
            builder.RegisterAssemblyTypes(Assembly.GetEntryAssembly())
                .AsImplementedInterfaces();
            builder.Populate(services);

            builder.AddRabbitMq();
            builder.AddDispatchers();

            Container = builder.Build();
            return new AutofacServiceProvider(Container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseAppSwagger();
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            var rabbitPrefixNamespace = Environment.GetEnvironmentVariable("RABBITMQ_PREFIX_NAMESPACE");
            app.UseRabbitMq()
                .SubscribeEvent<UserCreated>(@namespace: "users", prefixQueueName: rabbitPrefixNamespace,
                    exchangeType: ExchangeType.Fanout)
                .SubscribeEvent<ProjectCreated>(@namespace: "projects");
        }
    }
}