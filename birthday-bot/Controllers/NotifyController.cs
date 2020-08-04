using System;
using System.Collections.Concurrent;
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
        private readonly string _specificChannelID;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly IOStore _oStore;
        // IStore store,
        public NotifyController(SlackAdapter adapter, IConfiguration configuration,
            ConcurrentDictionary<string, ConversationReference> conversationReferences,
            IOStore ostore)
        {
            _adapter = adapter;
            _conversationReferences = conversationReferences;
            _appId = configuration["MicrosoftAppId"];
            _specificChannelID = configuration["SpecificChannelID"];
            _oStore = ostore;
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
            if (_conversationReferences.Values.Count == 0)
            {
                // Analyze if possible to load the ConversationState with ConversationID as a Key
                // Check how much conversations are saved in oStore (related to channels where the bots is added)
                // and foreach channel send the message (channels in the same Workspace?)
                var storedConvState = await _oStore.LoadAsync(); // _store.LoadAsync();
                if (storedConvState != null && !string.IsNullOrEmpty(storedConvState.ToString()))
                {
                    try
                    {
                        var des = JsonConvert.DeserializeObject<ConversationReference>(storedConvState.ToString());
                        var oldConversation = des.Conversation;
                        des.ServiceUrl = "null";
                        des.Conversation = new ConversationAccount(oldConversation.IsGroup, oldConversation.ConversationType,
                            oldConversation.Id, oldConversation.Name, oldConversation.AadObjectId, oldConversation.Role, oldConversation.TenantId);
                        _conversationReferences.AddOrUpdate(des.User.Id, des, (key, newValue) => des);
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
            // If you encounter permission-related errors when sending this message, see
            // https://aka.ms/BotTrustServiceUrl
            //await turnContext.SendActivityAsync("Happy Birthday! <@U015G20UGHF>");

            //foreach(var item in Birthdays)
            //{
            //    await turnContext.SendActivityAsync($"Happy Birthday! <@{item.slackUser.Id}>");
            //}
        }


    }
}
