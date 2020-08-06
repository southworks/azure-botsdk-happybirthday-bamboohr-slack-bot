// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
        // Message to send to users when the bot receives a Conversation Update event
        //private const string WelcomeMessage = "Welcome to the Proactive Bot sample.  Navigate to http://localhost:3978/api/notify to proactively message everyone who has previously messaged this bot.";

        // Dependency injected dictionary for storing ConversationReference objects used in NotifyController to proactively message users
        private readonly IOStore _oStore;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReference;
        public Birthday_Bot(IOStore ostore, ConcurrentDictionary<string, ConversationReference> conversationReference)
        {
            _oStore = ostore ?? throw new ArgumentNullException(nameof(ostore));
            _conversationReference = conversationReference;
        }

        private async void AddConversationReference(Activity activity)
        {
            List<ConversationReference> storedConvRef = new List<ConversationReference>();
            var storedObjects = await _oStore.LoadAsync(); // Stored Conversations
            if (storedObjects != null && !string.IsNullOrEmpty(storedObjects.ToString()))
            {
                try
                {
                    storedConvRef = JsonConvert.DeserializeObject<List<ConversationReference>>(storedObjects.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            var conversationReference = activity.GetConversationReference();
            conversationReference.ServiceUrl = "null";
            // Here we set thread_ts in JSON to null, so we avoid to continue on a Thread
            var oldConversation = conversationReference.Conversation;
            conversationReference.Conversation = new ConversationAccount(oldConversation.IsGroup, oldConversation.ConversationType,
                oldConversation.Id, oldConversation.Name, oldConversation.AadObjectId, oldConversation.Role, oldConversation.TenantId);

            if (conversationReference.Conversation.Id == null) // When conversationReference is still in memory
            {
                
                return;
            }

            var savedData = storedConvRef.ToDictionary(r => r.Conversation.Id, r => r);
            var concurrentSaveData = new ConcurrentDictionary<string, ConversationReference>(savedData);

            concurrentSaveData.AddOrUpdate(conversationReference.Conversation.Id, conversationReference, (key, newValue) => conversationReference);
            await SaveState(concurrentSaveData.Values.ToList());
        }

        private async Task SaveState(List<ConversationReference> conversationState)
        {
            // Analyze if possible to save the ConversationState with ConversationID as a Key
            await _oStore.SaveAsync(JsonConvert.SerializeObject(conversationState));
        }

        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);

            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Every message on the channel where the bot is added
            AddConversationReference(turnContext.Activity as Activity);
            return;
        }


    }
}
