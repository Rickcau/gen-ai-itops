## Setup Azure Automation
[Please review the instructions in the this document](./RunBooks.md).

## Rename the App.config.bak file to App.config
1. Make sure that the App.config file has all the variables set for your environment.
2. Save the file as it will be used to load the configration information at runtime.
3. The same App.config concept will be used for all the examples / solutions in this repo.

## The setup process is:

1. Create app roles in App Registration
2. Assign users to those roles in Enterprise Application

**This split exists because:**

- App Registration: Defines the application and its roles
- Enterprise Application: Manages who can access the application and what roles they have

## Application Registerations
We will create one App Registerations in Entra ID, for this solution, which will be for the API.  We will create the API App Registration below the API is actually deployed.

### Create the App Registration
#### API App Registeration
1. Log into the Azure Portal and navigate to  Entra ID > App Registrations
2. Click on New Registration and use the following example to create a registeration for the frontend:
```
    Name: genai-itops-api | Expose an API
    Who can use this application or access this API?
    X - Accounts in this organization directory only
```
3. Click Register

Make note of the Client ID, Tenant ID

4. Navigate to Manage > Expose an API and click on Application ID URI > Add
It should look somehting like this:
```
   api://XXXXXXX-430e-42c4-XXXX-e26a16c7faa0
```
This URI will be used as the base for your API's identity.
Click on > Save.

For this scenario (where roles are what we care about), you don't need to add any scopes.

When the console app or any client requests a token with .default scope (like in the console app), it will get all the roles the user has been assigned without needing any additional scope definitions here.

#### Authentication for the App Registration
Make sure that you have done the following otherwise the Console App will not work propertly.
1. Go to the App Registeration for "genai-itops-api" click on > Manage > Authentication
2. Click on > Add a Platform and for Redirect URLs select the following:

```
   X https://login.microsoftonline.com/common/oauth2/nativeclient
```

3. Click on **Configure** to save the changes.
1. 
#### Add Roles
1. Go to "App roles" in the left menu
2. Create your roles here (VMAdmin, VMViewer, etc.)
3. Then assign users to these roles

Are are examples of how you can defind a custom role for VM Viewer and VM Administrator.

```
    {
        "displayName": "VM Viewer",
        "description": "Can list and view VM status",
        "value": "VMViewer",
        "allowedMemberTypes": ["User"]
    }

    {
        "displayName": "VM Administrator",
        "description": "Full access to manage VMs including shutdown",
        "value": "VMAdmin",
        "allowedMemberTypes": ["User"]
    }
```

# Testing
- Run the Test Console App
- When prompted to select an account in the Browser Popup, select the account you want to test
- The Console App will print the permissions the logged in user has


# Frontend Workflow
1. User logs into frontend using Easy Auth
2. Frontend gets access token
3. Frontend includes the token in API requests
4. API validates token and checks roles
5. API executes permitted operations

![frontend-workflow](/images/easy-auth-flow-console-app.jpg)

