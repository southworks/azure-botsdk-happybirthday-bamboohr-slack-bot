# Birthday Bot

## Create Azure Function  
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

### Next Steps

* [BambooHR Integration](BambooHR.md#bambooHR-integration)