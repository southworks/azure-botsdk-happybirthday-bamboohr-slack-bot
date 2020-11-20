# Deploy to Azure
This document explains how to deploy the bot. In which it can be done through 2 methods:
- Deploy with Visual Studio.
- Deploy via CLI.

## Deploy with Visual Studio
1. In Solution Explorer, right-click in the project node and choose **Publish** (or use the **Build** > **Publish** menu item).
1. In the **Publish** dialog, select **Azure**.
1. Select **Azure App Service (Windows)** and **Next**.
1. Sign in with you Azure account, if necessary. Select **Create a new Azure App Service...**
1. In the **Create Azure App Service (Windows)** dialog, the **App Name, Resource Group**, and **App Service Plan** entry fields are populated. Select **Create**. When ready, select **Finish**.

![Deploy to Azure](images/deploy.png)

## Deploy via CLI

### Prerequisites

* Install the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest)
* An **App Service** deployed in the [Azure Portal](https://portal.azure.com/)

### Steps

1. Clone the repository into your file system:

    ```shell
    git clone https://github.com/southworks/azure-botsdk-happybirthday-bamboohr-slack-bot.git
    ```

1. Open the **Windows PowerShell**.

1. Navigate into the the folder where file `.csproj` is located.

1. First of all, execute the following command to log into the Azure Portal. This command will prompt you a browser screen in order to enter your azure credential:

    ````shell
    az login
    ````

1. Execute the following command in order to add the configuration files into the root of your local source code directory and be able to publish using `az webapp deployment`:

    ````shell
    az bot prepare-deploy --lang Csharp --proj-file-path ./
    ````
    > **More information:** [az bot prepare-deploy](https://docs.microsoft.com/en-us/cli/azure/bot?view=azure-cli-latest#az-bot-prepare-deploy)

1. Execute the following command to publish the application and its dependencies to a folder for deployment:

    ````shell
    dotnet publish -o <FOLDER-NAME>/
    ````
    > **NOTE:** Before executing this command, the `folder-name` path must exist. The `-o` command option indicates the output directory.

1. Run the following command to compress the publish files generated in the previous step:

    ````shell
    Get-ChildItem -Path "<FOLDER-NAME>" | Compress-Archive -DestinationPath <ZIP-FILE-NAME.zip> -Force
    ````
    > **NOTE:** The `Get-ChildItem` command displays the elements and children of one or more specified locations.

1. Run the following command to deploy the bot source code into the **Azure Portal** using the zip file compressed in the previous step:

    ````shell
    az webapp deployment source config-zip --resource-group "<YOUR-RESOURCE-GROUP-NAME>" --name "<YOUR-APP-SERVICE-BOT-NAME>" --src "<ZIP-FILE-NAME.zip>"
    ````
### Next Steps

* [Create Slack App](SlackApp.md#create-slack-app)
* [Configure blobstorage & container](Blobstorage&Container.md#configure-blob-storage--container)
* [Configure appsetting.json](ConfigureAppsettings.md#configure-appsettingsjson)
* [Republish Bot](RepublishBot.md#republish-bot)
* [Create Azure Function](AzureFunction.md#create-azure-function)

[← Back to Table of contents](README.md#table-of-contents)