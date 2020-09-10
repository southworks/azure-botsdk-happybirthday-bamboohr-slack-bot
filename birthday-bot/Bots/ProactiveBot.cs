using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Birthday_Bot
{
    public class Birthday_Bot : ActivityHandler
    {
        // Dependency injected dictionary for storing ConversationReference objects used in NotifyController to proactively message users
        private readonly IOStore _oStore;
        public Birthday_Bot(IOStore ostore)
        {
            _oStore = ostore ?? throw new ArgumentNullException(nameof(ostore));
        }

        private async Task AddConversationReference(Activity activity)
        {
            List<ConversationReference> storedConversationReferences = new List<ConversationReference>();
            var storedConversationsJSON = await _oStore.LoadAsync(); // Stored Conversations
            if (storedConversationsJSON != null && !string.IsNullOrEmpty(storedConversationsJSON.ToString()))
            {
                try
                {
                    storedConversationReferences = JsonConvert.DeserializeObject<List<ConversationReference>>(storedConversationsJSON.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            var currentActivityConversationReference = activity.GetConversationReference();
            currentActivityConversationReference.ServiceUrl = "null";
            // Here we set thread_ts in JSON to null, so we avoid to continue on a Thread
            //var oldConversation = currentActivityConversationReference.Conversation;
            currentActivityConversationReference.Conversation = new ConversationAccount(currentActivityConversationReference.Conversation.IsGroup, 
                currentActivityConversationReference.Conversation.ConversationType,
                currentActivityConversationReference.Conversation.Id,
                currentActivityConversationReference.Conversation.Name, 
                currentActivityConversationReference.Conversation.AadObjectId, 
                currentActivityConversationReference.Conversation.Role, 
                currentActivityConversationReference.Conversation.TenantId);

            var concurrentConversationReferences = new ConcurrentDictionary<string, ConversationReference>(storedConversationReferences.ToDictionary(r => r.Conversation.Id, r => r));

            concurrentConversationReferences.AddOrUpdate(currentActivityConversationReference.Conversation.Id, currentActivityConversationReference, (key, newValue) => currentActivityConversationReference);
            await SaveState(concurrentConversationReferences.Values.ToList());
        }

        private async Task SaveState(List<ConversationReference> conversationState)
        {
            await _oStore.SaveAsync(JsonConvert.SerializeObject(conversationState));
        }

        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);
            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Every message on the channel where the bot is added
            await AddConversationReference(turnContext.Activity as Activity);
            return;
        }


    }
}
