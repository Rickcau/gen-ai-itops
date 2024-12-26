# ConsoleApp Azure Check Permissions
This Console App demonstrates how we can use Easy Authentication to authenticate a user and how to retrieve the roles/permissions/claims that the user has access to using the API App Registration.

It's important to understand these concepts because they will be used for the frontend and the backend API.  

In the **ConsoleApp-Az-Automation** we demonstrated how to run a **Runbook**.  In this example, we are demonstrating how to use RBAC for permissions and verify what Roles / Permissions the user has for the credentials you are logged in with.  At the README.md file in the root of the repo, I explain how to setup the Roles, we created a **VM User** and a **VM Administrator**.  Now that we can see how to leverage the App Registration and check the claims of the user, we can easily determine of they have permissions to perform an operation.

## Our Next Steps
Next, you need to understand how to build a customer index for this use case, which we will demostrate this in the **ConsoleApp-Build-Ai-Index**.  In the **ConsoleApp-Build-Ai-Index** it takes several arguments which allow you to **create**, **delete**, **load** and **search** the index.


