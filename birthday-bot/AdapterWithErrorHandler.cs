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
        private string _blobStorageStringConnection;
        private string _blobStorageContainer;
        private string _bambooFileName;
        private string _slackFileName;
        private List<SlackUser> users;

        private BambooUsersStorage bambooUsersStorage;

        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger)
            : base(configuration, logger)
        {
            _blobStorageStringConnection = configuration["BlobStorageStringConnection"];
            _blobStorageContainer = configuration["BlobStorageContainer"];
            _bambooFileName = configuration["BambooFileName"];
            _slackFileName = configuration["SlackFileName"];
            users = new List<SlackUser>();
            bambooUsersStorage = new BambooUsersStorage();
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
            try
            {
                var _bambooUsers = bambooUsersStorage.GetBambooUsers(_blobStorageStringConnection, _blobStorageContainer, _bambooFileName);
                if (_bambooUsers.Any())
                {
                    SlackUsersStorage slackUsers = new SlackUsersStorage();
                    users = slackUsers.GetSlackUsersBlobFromEmails(_bambooUsers, _blobStorageStringConnection, _blobStorageContainer, _slackFileName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            using (var context = new TurnContext(this, reference.GetContinuationActivity()))
            {
                if (users.Any())
                {
                    await context.SendActivityAsync("Today's " +
                         string.Join(", ", users.Select(
                             r => string.Concat("<@", r.Id, ">"))
                             .ToArray()) + " Birthday! Let's greet them (only if they bring some :cake: of course)");
                }
                await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
