using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters.Slack;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Birthday_Bot.Controllers
{
    [Route("api/notify")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly string _specificChannelName;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly IOStore _oStore;
        private readonly string _slackBotToken;
        private readonly string _blobStorageStringConnection;
        private readonly string _blobStorageDataUserContainer;
        private readonly string _bambooHRUsersFileName;
        private string happyBirthdayMessage;

        public NotifyController(SlackAdapter adapter, IConfiguration configuration,
            ConcurrentDictionary<string, ConversationReference> conversationReferences,
            IOStore ostore)
        {
            _adapter = adapter;
            _conversationReferences = conversationReferences;
            _appId = configuration["MicrosoftAppId"];
            _specificChannelName = configuration["SpecificChannelName"];
            _slackBotToken = configuration["SlackBotToken"];
            _oStore = ostore;
            _blobStorageStringConnection = configuration["BlobStorageStringConnection"];
            _blobStorageDataUserContainer = configuration["BlobStorageDataUsersContainer"];
            _bambooHRUsersFileName = configuration["BambooHRUsersFileName"];
            // If the channel is the Emulator, and authentication is not in use,
            // the AppId will be null.  We generate a random AppId for this case only.
            // This is not required for production, since the AppId will have a value.
            if (string.IsNullOrEmpty(_appId))
            {
                _appId = Guid.NewGuid().ToString(); //if no AppId, use a random Guid
            }
        }

        public async Task<IActionResult> Get()
        {
            BirthdaysHelper birthdaysHelper = new BirthdaysHelper(_blobStorageStringConnection, _blobStorageDataUserContainer,
                _bambooHRUsersFileName, _slackBotToken);
            happyBirthdayMessage = await birthdaysHelper.GetBirthdayMessage();
            if (!string.IsNullOrEmpty(happyBirthdayMessage))
            {
                var _specificChannelID = await SlackInterop.GetChannelIdByName(_specificChannelName, _slackBotToken);
                if (_conversationReferences.Values.Count == 0)
                {
                    var storedConversationReferenciesJson = await _oStore.LoadAsync(); // _store.LoadAsync();
                    if (storedConversationReferenciesJson != null && !string.IsNullOrEmpty(storedConversationReferenciesJson.ToString()))
                    {
                        try
                        {
                            var storedConversationReferencesList = JsonConvert.DeserializeObject<List<ConversationReference>>(storedConversationReferenciesJson.ToString());
                            foreach (var conversationRef in storedConversationReferencesList)
                            {
                                conversationRef.ServiceUrl = "null";
                                conversationRef.Conversation = new ConversationAccount(conversationRef.Conversation.IsGroup, conversationRef.Conversation.ConversationType,
                                    conversationRef.Conversation.Id, conversationRef.Conversation.Name, conversationRef.Conversation.AadObjectId, conversationRef.Conversation.Role,
                                    conversationRef.Conversation.TenantId);
                                _conversationReferences.AddOrUpdate(conversationRef.Conversation.Id, conversationRef, (key, newValue) => conversationRef);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }

                // Send message, only, to specific channel
                if (_conversationReferences.Values.Any(r => r.Conversation.Id == _specificChannelID))
                {
                    var _specificChannelConversationRef = _conversationReferences.Values.First(r => r.Conversation.Id == _specificChannelID);
                    await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, _specificChannelConversationRef, BotCallback, default(CancellationToken));
                }
            }
            // Let the caller know proactive messages have been sent
            return new ContentResult()
            {
                Content = "<html><body><h1>Proactive messages have been sent: Happy Birthday!.</h1></body></html>",
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
            };
        }

        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(happyBirthdayMessage);
        }
    }
}
