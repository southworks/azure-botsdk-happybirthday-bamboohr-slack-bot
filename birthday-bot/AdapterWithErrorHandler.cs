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
        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger, List<Birthday> birthdays)
            : base(configuration, logger)
        {
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
                    var slackUsers = getSlackUserIDFromEmail("");
                    if (slackUsers.Any())
                    {
                        var joined = from bU in bambooUsers
                                     join sU in slackUsers on bU.Email equals sU.Email
                                     select new Birthday()
                                     {
                                         slackUser = sU,
                                         bambooUser = bU
                                     };

                        if (joined.Any())
                        {
                            Birthdays = joined.ToList();
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

        public List<SlackUser> getSlackUserIDFromEmail(string email)
        {
            try
            {
                #region Slack API
                // Here we use the Bot endpoint
                //string botendpoint = "https://slack.com/api/users.list";

                //var client = new HttpClient();
                //var request = new HttpRequestMessage()
                //{
                //    RequestUri = new Uri(botendpoint),
                //    Method = HttpMethod.Post,
                //};
                //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "xoxb-1172066589573-1231858313910-dFClUcBz283AjVTd7ovFbxzr");
                //client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                //HttpResponseMessage responseMessage = await client.SendAsync(request);
                //String response = await responseMessage.Content.ReadAsStringAsync();
                //Debug.WriteLine(response);
                //Console.WriteLine(response);
                #endregion Slack API

                return new List<SlackUser>() {
                    new SlackUser(){ Email = "german.gonzalez@southworks.com", Id = "UD5S2KKGX", Is_Bot = false, Is_Restricted = false },
                    new SlackUser(){ Email = "denise.scollo@southworks.com", Id = "U92C18XNX", Is_Bot = false, Is_Restricted = false },
                    new SlackUser(){ Email = "mariano.stinson@southworks.com", Id = "UE1HFMS04", Is_Bot = false, Is_Restricted = false },
                    new SlackUser(){ Email = "gabriel.antelo@southworks.com", Id = "UHK1D4TK9", Is_Bot = false, Is_Restricted = false },
                    new SlackUser(){ Email = "bernardo.ortiz@southworks.com", Id = "UGHB2U1NK", Is_Bot = false, Is_Restricted = false },
                };
            }
            catch (Exception e)
            {
                return new List<SlackUser>() { };
            }
        }

        public List<BambooHRUser> getTodayBirthdaysBambooUsers()
        {
            try
            {
                #region BambooHR
                #endregion BambooHR

                var users = new List<BambooHRUser>() {
                    new BambooHRUser(){ Email = "german.gonzalez@southworks.com", Birthday = new DateTime(2020,7,23) },
                    new BambooHRUser(){ Email = "denise.scollo@southworks.com", Birthday = new DateTime(2020,7,28) },
                    new BambooHRUser(){ Email = "mariano.stinson@southworks.com", Birthday = new DateTime(2020,7,30) },
                    new BambooHRUser(){ Email = "gabriel.antelo@southworks.com", Birthday = new DateTime(2020,7,26) },
                    new BambooHRUser(){ Email = "bernardo.ortiz@southworks.com", Birthday = new DateTime(2020,7,26) },
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
