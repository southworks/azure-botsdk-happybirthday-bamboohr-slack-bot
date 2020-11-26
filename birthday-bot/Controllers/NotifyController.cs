using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Birthday_Bot.Events;
using Birthday_Bot.Queue;
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
        private const string SLACK = "Slack";
        private const string QUEUE = "Queue";

        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly IOStore _oStore;
        private readonly IQueueProducer _queueProducer;
        private readonly string _appId;
        private readonly string _specificChannelName;
        private readonly string _slackBotToken;
        private readonly string _blobStorageStringConnection;
        private readonly string _blobStorageDataUserContainer;
        private readonly string _bambooHRUsersFileName;
        private readonly string _storageMethod;
        private readonly IEnumerable<string> _enabledNotifications;

        private string happyBirthdayMessage;

        public NotifyController(
            SlackAdapter adapter,
            IConfiguration configuration,
            ConcurrentDictionary<string, ConversationReference> conversationReferences,
            IOStore ostore,
            IQueueProducer queueProducer)
        {
            _adapter = adapter;
            _conversationReferences = conversationReferences;
            _oStore = ostore;
            _queueProducer = queueProducer;
            _appId = configuration["MicrosoftAppId"];
            _specificChannelName = configuration["SpecificChannelName"];
            _slackBotToken = configuration["SlackBotToken"];
            _blobStorageStringConnection = configuration["BlobStorageStringConnection"];
            _blobStorageDataUserContainer = configuration["BlobStorageDataUsersContainer"];
            _bambooHRUsersFileName = configuration["BambooHRUsersFileName"];
            _enabledNotifications = configuration.GetSection("EnabledNotifications").Get<List<string>>();
            if (string.IsNullOrEmpty(configuration["StorageMethod"]))
            {
                _storageMethod = "JSON";
            }
            else
            {
                _storageMethod = configuration["StorageMethod"];
            }
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
                _bambooHRUsersFileName, _slackBotToken, _storageMethod);

            happyBirthdayMessage = await birthdaysHelper.GetBirthdayMessageAsync();

            if (!string.IsNullOrWhiteSpace(happyBirthdayMessage))
            {
                if (_enabledNotifications.Contains(SLACK))
                {
                    await SendBirthdayMessagesToSlack();
                }
                if (_enabledNotifications.Contains(QUEUE))
                {
                    await SendBirthdayMessageToQueue();
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
        private async Task SendBirthdayMessageToQueue()
        {
            await _queueProducer.SendMessageAsync(happyBirthdayMessage);
        }
        private async Task SendBirthdayMessagesToSlack()
        {
            var _specificChannelID = await SlackInterop.GetChannelIdByNameAsync(_specificChannelName, _slackBotToken);
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
                        // This console log only works locally.
                        // TODO: add AppInsights, maybe?
                        Console.WriteLine(ex.ToString());
                    }
                }
            }

            // Send message only to specific channel
            if (_conversationReferences.Values.Any(r => r.Conversation.Id == _specificChannelID))
            {
                var _specificChannelConversationRef = _conversationReferences.Values.First(r => r.Conversation.Id == _specificChannelID);
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, _specificChannelConversationRef, BotCallback, default(CancellationToken));
            }
        }

        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(happyBirthdayMessage);
        }
    }
}
