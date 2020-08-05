using Birthday_Bot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters.Slack;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Birthday_Bot
{
    public class AdapterWithErrorHandler : SlackAdapter
    {
        //private List<Birthday> Birthdays;
        private readonly IConfiguration _configuration;
        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger, List<Birthday> birthdays)
            : base(configuration, logger)
        {
            _configuration = configuration;
            //Birthdays = birthdays;
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

                // Send a message to the user
                //await turnContext.SendActivityAsync("The bot encountered an error or bug.");
                //await turnContext.SendActivityAsync("To continue to run this bot, please fix the bot source code.");

                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
            };
        }

        public override async Task ContinueConversationAsync(string botId, ConversationReference reference, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            List<Birthday> Birthdays = new List<Birthday>();
            try
            {
                var bambooUsers = getTodayBirthdaysBambooUsers();
                if (bambooUsers.Any())
                {
                    foreach(var bambooUser in bambooUsers)
                    {
                        var slackUser = await SlackInterop.GetSlackUserByEmailAsync(_configuration["SlackBotToken"], bambooUser.Email);
                        if (slackUser != null)
                        {
                            Birthdays.Add(new Birthday() { 
                            bambooUser = bambooUser,
                            slackUser = slackUser,
                            });
                        }
                    }
                }
            }
            catch { 
            
            }
            using (var context = new TurnContext(this, reference.GetContinuationActivity()))
            {

                if (Birthdays.Any())
                {
                    await context.SendActivityAsync("Happy Birthday " +
                         string.Join(", ", Birthdays.Select(
                             r => string.Concat("<@", r.slackUser.Id, ">"))
                             .ToArray()));
                }
                // *** Added for testing purpose ***
                //else
                //{
                //    await context.SendActivityAsync("There's no birthday for today!");
                //}
                await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);
                //context.TurnState.Add<IIdentity>(BotIdentityKey, claimsIdentity);
                //context.TurnState.Add<BotCallbackHandler>(callback);
            }
        }

        public List<BambooHRUser> getTodayBirthdaysBambooUsers()
        {
            try
            {
                #region BambooHR
                #endregion BambooHR

                var users = new List<BambooHRUser>() {
                    new BambooHRUser(){ Email = "rodrigo.funes@southworks.com", Birthday = new DateTime(2020,8,5) },
                    new BambooHRUser(){ Email = "melisa.onofri@southworks.com", Birthday = new DateTime(2020,8,5) },
                    new BambooHRUser(){ Email = "lazaro.ansaldi@southworks.com", Birthday = new DateTime(2020,8,7) },
                    new BambooHRUser(){ Email = "adrian.juri@southworks.com", Birthday = new DateTime(2020,8,8) },
                    new BambooHRUser(){ Email = "alejandro.daza@southworks.com", Birthday = new DateTime(2020,8,9) },
                };
                return users.Where(r => r.Birthday.Date.CompareTo(DateTime.Now.Date) == 0).ToList();
            }
            catch (Exception e)
            {
                return new List<BambooHRUser>() { };
            }
        }
    }
}
