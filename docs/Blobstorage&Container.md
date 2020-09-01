# Configure Blob storage & container
Here’s how to create an Azure Storage Account Container and a container, assuming you already have an Azure subscription and a Resource group created.
  
- In your browser, navigate to the [Azure Portal](https://portal.azure.com).
  
- Add new Storage Account into your Azure Portal

    ![Storage account](images/create-storage-account.png)

- We select our Resource group. We write a name for the Storage Account. Leave the default settings.
Finally, we select Review + Create to review your Storage Account settings and create the account.  
     ![Storage account](images/form_storage_account.png)

- Once created the Storage Account, add a new Container. Enter a name, select in Public access level Private, and then click create.  

     ![Storage account](images/add-container.png)

### Next Steps

* [Configure appsetting.json](ConfigureAppsettings.md#configure-appsetting.json)
* [Deploy on Azure](DeployAzurePortal.md#deploy-on-azure-portal )
* [Create Azure Function](AzureFunction.md#create-azure-function)
* [BambooHR Integration](BambooHR.md#bambooHR-integration)

[← Back to Table of contents](README.md#table-of-contents)