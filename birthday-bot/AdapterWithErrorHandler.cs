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
using Birthday_Bot.DataStorage;

namespace Birthday_Bot
{
    public class AdapterWithErrorHandler : SlackAdapter
    {
        //private List<Birthday> Birthdays;
        private readonly string _slackBotToken;
        private string _blobStorageStringConnection;
        private string _blobStorageDataUserContainer;
        private string _bambooHRUsersFileName;
        private BambooUsersStorageInterop _bambooHRUsersStorage;

        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger)
            : base(configuration, logger)
        {
            _slackBotToken = configuration["SlackBotToken"];
            _blobStorageStringConnection = configuration["BlobStorageStringConnection"];
            _blobStorageDataUserContainer = configuration["BlobStorageDataUsersContainer"];
            _bambooHRUsersFileName = configuration["BambooHRUsersFileName"];
            _bambooHRUsersStorage = new BambooUsersStorageInterop();
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
            };
        }

        public override async Task ContinueConversationAsync(string botId, ConversationReference reference, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            List<Birthday> TodaysBirthdays = new List<Birthday>();
            try
            {
                var _bambooHRBirthdays = _bambooHRUsersStorage.GetTodaysBirthdays(_blobStorageStringConnection, _blobStorageDataUserContainer, _bambooHRUsersFileName);
                if (_bambooHRBirthdays.Any())
                {
                    foreach (var bambooUser in _bambooHRBirthdays)
                    {
                        var slackUser = await SlackInterop.GetSlackUserByEmailAsync(_slackBotToken, bambooUser.Email);
                        if (slackUser != null)
                        {
                            TodaysBirthdays.Add(new Birthday()
                            {
                                bambooUser = bambooUser,
                                slackUser = slackUser,
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            using (var context = new TurnContext(this, reference.GetContinuationActivity()))
            {
                if (TodaysBirthdays.Any())
                {
                    await context.SendActivityAsync("Today's " +
                         string.Join(", ", TodaysBirthdays.Select(
                             r => string.Concat("<@", r.slackUser.Id, ">"))
                             .ToArray()) + " Birthday! Let's greet them (only if they bring some :cake: of course)");
                }
                await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
