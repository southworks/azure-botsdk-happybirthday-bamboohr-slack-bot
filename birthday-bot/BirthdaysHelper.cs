using Birthday_Bot.DataStorage;
using Birthday_Bot.Models;
using Microsoft.Bot.Builder.LanguageGeneration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Birthday_Bot
{
    public class BirthdaysHelper
    {
        private readonly string _slackBotToken;
        private readonly string _blobStorageStringConnection;
        private readonly string _blobStorageDataUserContainer;
        private readonly string _bambooHRUsersFileName;

        public BirthdaysHelper(string blobStorageStringConnection, string blobStorageDataUserContainer, string bambooHRUsersFileName,
            string slackBotToken)
        {
            _slackBotToken = slackBotToken;
            _blobStorageStringConnection = blobStorageStringConnection;
            _blobStorageDataUserContainer = blobStorageDataUserContainer;
            _bambooHRUsersFileName = bambooHRUsersFileName;
        }

        public async Task<string> GetBirthdayMessageAsync()
        {
            List<Birthday> TodaysBirthdays = await GetBirthdaysAsync();
            string happyBirthdayMsg = "";
            if (TodaysBirthdays.Any()){
                happyBirthdayMsg = GetHappyBirthdayMsg(TodaysBirthdays);
            }
            return happyBirthdayMsg;
        }

        private async Task<List<Birthday>> GetBirthdaysAsync()
        {
            List<Birthday> Birthdays = new List<Birthday>();
            try
            {
                var _bambooHRBirthdays = BambooUsersStorageInterop.GetTodaysBirthdays(_blobStorageStringConnection, _blobStorageDataUserContainer, _bambooHRUsersFileName);
                if (_bambooHRBirthdays.Any())
                {
                    foreach (var bambooUser in _bambooHRBirthdays)
                    {
                        var slackUser = await SlackInterop.GetSlackUserByEmailAsync(_slackBotToken, bambooUser.Email);
                        if (slackUser != null)
                        {
                            Birthdays.Add(new Birthday()
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
            return Birthdays;
        }

        private string GetHappyBirthdayMsg(List<Birthday> birthdays)
        {
            string fullPath = Path.Combine(".", "Resources", "Phrases.lg");
            var slackUsersIds = string.Join(", ", birthdays.Select(
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
            return phrase;
        }
    }
}
