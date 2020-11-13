# Republis Bot
Once the parameters of the **appsettings.json** file of our Bot are complete, it is necessary to republish these changes. This document explains how to republish these changes from **Visual Studio**.
### Prerequisites
* Have the parameters of the **appsettings.json** file configured.

### Steps

1. In Solution Explorer, right-click in the project node and choose **Publish** (or use the **Build** > **Publish** menu item).

    ![Republish Bot](images/republish-bot.png)

1. Next, a panel with project properties will be shown, verify they are correct and select **Publish**.
    ![Republish Bot](images/confirm-republish-bot.png)

Once the deployment is complete, we will have updated the parameters of the **appsettings.json** file in Azure.

### Next Steps

* [Create Azure Function](AzureFunction.md#create-azure-function)


[← Back to Table of contents](README.md#table-of-contents)