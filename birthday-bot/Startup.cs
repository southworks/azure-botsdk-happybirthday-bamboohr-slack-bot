using System.Collections.Concurrent;
using Birthday_Bot.Events;
using Birthday_Bot.Queue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters.Slack;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Birthday_Bot
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            // Create the Bot Framework Adapter with error handling enabled.
            // Create the Slack Adapter
            services.AddSingleton<SlackAdapter, AdapterWithErrorHandler>();

            // Create a global hashset for our ConversationReferences
            services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();

            // Create the storage where we'll be using for the Conversation Reference.
            services.AddSingleton<IOStore, BlobContainerConversationStore>();

            // Create an event producer to send proactive messages
            services.AddSingleton<IQueueProducer, QueueProducer>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, Birthday_Bot>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
