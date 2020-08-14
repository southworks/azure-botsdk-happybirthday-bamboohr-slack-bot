﻿using Birthday_Bot.Models;
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
        //private List<Birthday> Birthdays;
        private readonly string _slackBotToken;
        private string _blobStorageStringConnection;
        private string _blobStorageContainer;
        private string _bambooFileName;
        private BambooUsersStorage bambooUsersStorage;

        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger)
            : base(configuration, logger)
        {
            _slackBotToken = configuration["SlackBotToken"];
            _blobStorageStringConnection = configuration["BlobStorageStringConnection"];
            _blobStorageContainer = configuration["BlobStorageContainer"];
            _bambooFileName = configuration["BambooFileName"];
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
            List<Birthday> Birthdays = new List<Birthday>();
            try
            {
                var _bambooUsers = bambooUsersStorage.GetBambooUsers(_blobStorageStringConnection, _blobStorageContainer, _bambooFileName);
                if (_bambooUsers.Any())
                {
                    foreach (var bambooUser in _bambooUsers)
                    {
                        var slackUser = await SlackInterop.GetSlackUserByEmailAsync(_slackBotToken, bambooUser.Email);
                        if (slackUser != null)
                        {
                            Birthdays.Add(new Birthday()
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
                string fullPath = Path.Combine(".", "Resources", "Phrases.lg");
                Templates birthdayPhrasesTemplate = Templates.ParseFile(fullPath);

                if (Birthdays.Any())
                {
                    var slackUsersIds = string.Join(", ", Birthdays.Select(
                             r => $"<@{r.slackUser.Id}>")
                             .ToArray());
                    string phrase = birthdayPhrasesTemplate.Evaluate("RandomPhrases", new
                    {
                        users = slackUsersIds
                    }).ToString();
                    await context.SendActivityAsync(phrase);
                }
                await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
