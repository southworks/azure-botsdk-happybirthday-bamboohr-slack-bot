// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private const string WelcomeMessage = "Welcome to the Proactive Bot sample.  Navigate to http://localhost:3978/api/notify to proactively message everyone who has previously messaged this bot.";

        // Dependency injected dictionary for storing ConversationReference objects used in NotifyController to proactively message users
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly IOStore _oStore;
        public Birthday_Bot(IOStore ostore, ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            _oStore = ostore ?? throw new ArgumentNullException(nameof(ostore));
            _conversationReferences = conversationReferences;
        }

        private async void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();

            // Analyze why ServiceUrl must be null
            conversationReference.ServiceUrl = "null";
            // Here we set thread_ts in JSON to null, so we avoid to continue on a Thread
            var oldConversation = conversationReference.Conversation;
            conversationReference.Conversation = new ConversationAccount(oldConversation.IsGroup, oldConversation.ConversationType,
                oldConversation.Id, oldConversation.Name, oldConversation.AadObjectId, oldConversation.Role, oldConversation.TenantId);

            //Console.WriteLine(conversationReference.Conversation.Id);
            if (conversationReference.User.Id != null) // When conversationReference is still in memory
            {
                
                _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
            }
            else
            {
                conversationReference.User.Id = "12345"; 
                conversationReference.User.Name = "BirthdayBot";
            }
            await SaveState(conversationReference);
        }

        private async Task SaveState(ConversationReference conversationState)
        {
            // Analyze if possible to save the ConversationState with ConversationID as a Key
            var success = await _oStore.SaveAsync(JsonConvert.SerializeObject(conversationState));
        }

        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);

            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return;
            //foreach (var member in membersAdded)
            //{
            //    // Greet anyone that was not the target (recipient) of this message.
            //    if (member.Id != turnContext.Activity.Recipient.Id)
            //    {
            //        await turnContext.SendActivityAsync(MessageFactory.Text(WelcomeMessage), cancellationToken);
            //    }
            //}
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Every message on the channel where the bot is added
            AddConversationReference(turnContext.Activity as Activity);
            return;
            // Echo back what the user said
            //await turnContext.SendActivityAsync(MessageFactory.Text($"You sent '{turnContext.Activity.Text}'"), cancellationToken);
        }


    }
}
