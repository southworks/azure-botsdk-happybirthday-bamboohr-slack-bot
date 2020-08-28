using Birthday_Bot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters.Slack;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Birthday_Bot.DataStorage;

namespace Birthday_Bot
{
    public class AdapterWithErrorHandler : SlackAdapter
    {
        private readonly string _slackBotToken;
        private readonly string _blobStorageStringConnection;
        private readonly string _blobStorageDataUserContainer;
        private readonly string _bambooHRUsersFileName;
        private readonly string _storageMethod;
        private readonly BambooUsersStorageInterop _bambooHRUsersStorage;

        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger)
            : base(configuration, logger)
        {
            _slackBotToken = configuration["SlackBotToken"];
            _blobStorageStringConnection = configuration["BlobStorageStringConnection"];
            _blobStorageDataUserContainer = configuration["BlobStorageDataUsersContainer"];
            _bambooHRUsersFileName = configuration["BambooHRUsersFileName"];
            _storageMethod = configuration["StorageMethod"];
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
                var _bambooHRBirthdays = _bambooHRUsersStorage.GetTodaysBirthdays(_blobStorageStringConnection, _blobStorageDataUserContainer, _bambooHRUsersFileName, _storageMethod);
                if (_bambooHRBirthdays.Any())
                {
                    foreach (var bambooUser in _bambooHRBirthdays)
                    {
                        var slackUser = await SlackInterop.GetSlackUserByEmailAsync(_slackBotToken, bambooUser.Email);
                        if (slackUser != null)
                        {
                            TodaysBirthdays.Add(new Birthday()
                            {
                                BambooUser = bambooUser,
                                SlackUser = slackUser,
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
                    string fullPath = Path.Combine(".", "Resources", "Phrases.lg");
                    var slackUsersIds = string.Join(", ", TodaysBirthdays.Select(
                             r => $"<@{r.SlackUser.Id}>")
                             .ToArray());

                    Templates birthdayPhrasesTemplate = Templates.ParseFile(fullPath);
                    string phrase = birthdayPhrasesTemplate != null ?
                        birthdayPhrasesTemplate.Evaluate("RandomPhrases", new
                        {
                            users = slackUsersIds
                        }).ToString() : "";

                    if (string.IsNullOrEmpty(phrase))
                    {
                        phrase = "Someone told me that today's " +
                                        slackUsersIds 
                                        + " birthday! Another year older is another year wiser :birthday: :tada: :smile:.";
                    }
                    await context.SendActivityAsync(phrase);
                }
                await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
