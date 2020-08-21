# Birthday Bot
## How to configure & deploy  
### Pre-requisites

1. [Azure Portal](https://portal.azure.com) account
1. [Slack Workspace](https://slack.com/get-started#/create)
1. The bot in this repository

### Create Slack App

* [Connect deployed bot with Slack Channel](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-channel-connect-slack?view=azure-bot-service-4.0&tabs=abs)
* Add Slack-App to a channel
![Add Slack-App to a channel](images/add-app.png)
Select your channel and click on "Add an app"

### Configure blobstorage & container  
- In your browser, navigate to the [Azure Portal](https://portal.azure.com).
  
- Add new Storage account into your Azure Portal

    ![Storage account](images/create-storage-account.png)

- We select our Resource group. We write a name for the storage account. Leave the default settings.
Finally, we select Review + Create to review your storage account settings and create the account.  
     ![Storage account](images/form_storage_account.png)

- Once created the storage account, add a new Container. Enter a name, select in Public access level Private, and then click create.  

     ![Storage account](images/add-container.png)

- With the container created, we will upload the corresponding JSON files. We select the file and then click Upload. We repeat the process to upload the second file.  

     ![Storage account](images/upload-json-file.png)

### Configure appsetting.json
**Set Up Specific Channel**

Regarding the main funcionality of the bot, **Send Happy Birthday Message**, you may set up the related **specific channel name** in the **appsettings.json** file variable **SpecificChannelName**. The channel ID is obtanined by its name to more easily specify the channel on which the bot should send the message. This channel name must be added under the SpecificChannelName parameter without the # character.

![Storage account](images/specificChannelName.png)  

**Set Up Slack App**

As for the connection to the slack app, the parameters (**SlackVerificationToken, SlackClientSigningSecret and SlackBotToken**) of the **appsetting.json** file of our project have to be modified.
- Enter the page of [Slack Api](https://api.slack.com/apps)
    - Select the app you want to connect to  
    ![Select Slack App](images/select_slack_app.png)

    - Set parameter **SlackVerificationToken** in **appsetting.json file**:
    ![Set verification token](images/set_verification_token.png)

    - Set parameter **SlackClientSigningSecret** in **appsetting.json file**:
    ![Set verification token](images/clientSigninSecret.png)

    - Set parameter **SlackBotToken** in **appsetting.json file**:
    ![Set verification token](images/slack_bot_token.png)

**Set up Azure Blob Storage**
 
As part of the configuration of the bot, we need to configure several parameters related to the **Blob Storage** in the **appsettings.json** file. The first thing would be to enter our **Storage Account** from the [Azure Portal](https://portal.azure.com/). To modify the following parameters:
 
- **Connection string** into variable **BlobStorageStringConnection**, it is a connection string to the Azure Blob Storage.
![](images/blob_storage_string_connection.png)
- **Data User Container** into variable **BlobStorageDataUsersContainer**, it is an url to the Azure Blob Storage Container.
![](images/data_user_container.png)
- **Conversation Container** into variable **BlobStorageConversationContainer**, it is an url to the conversation data.
![](images/conversation_container.png)

### Deploy on Azure Portal 

![Deploy on azure portal](images/deploy.png)
### Create Azure Function  
* Add **Function** to **Azure Function** on **Azure Portal**, **Run/Test**

    - Add new Function App into your Azure Portal Resource Group

    ![Create Function App](images/create-function-app.png)

    - Select the Subscription & Resource Group, **Function App Name**, Runtime stack, Version and Region, and then an Storage account with Windows Operating System
    
    ![Create Function App Wizard](images/create-function-app-wizard.png)

    - Once created the **function-app**, add a **new Function**, based on **Timer trigger Template**

    ![Add Function to Function App](images/add-function-to-function-app.png)
    **Schedule** could be configure as the classic cronjob on linux.

    - After created the **function** with the template, edit and save the code to execute every tick of the timer

    ![Function Code](images/function-code.png)

    - Then **Test/Run** the code and review the log to confirm it runs without problems.

    ![Test Run Log](images/test-run-log.png)

 ## [Integration](../../integrations#README.md)
