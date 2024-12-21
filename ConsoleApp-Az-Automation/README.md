# Console App Azure Automation
This Console App demostrates the use of Easy Authentication to authenticate a user and to Start an Azure Automation Runbook.

**Important Note**
We are not using the API Application Registration for this.  

## At the Subscription Level do the following
1. Navigate to the Subscription and click on > Add > Add role assingment
2. Add the following Roles for the Account you plan to use with this Console App.

```
  Automation Contributor
  Automation Job Operator
  Automation Operator
```

Keep in mind that we are not using the App Registeration we will do that in the **ConsoleApp-Az-Check-Permissions** example.

If you do not do this and you run the Console App, and you authenticate with a user that does not have these Roles at the Subscription level you will not
be able to start the Runbook.

