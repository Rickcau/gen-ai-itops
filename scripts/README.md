# System Assigned Identity & Role Assigments
By default Azure Automation uses System Assigned Identity.  By default this identity does not have permissions to control VMs so you need to ensure the System Assigned Identity has the necesscary assignments in order to perform the operations for any give runbook.

## Setting up Azure Automation with the right permissions for VM management
Following the steps below will help ensure you have the necesscary role assignments for VM management and will allow you to test and verify Azure Automation can interact with the VMs.
I am adding the assignment at the Subscription level, but of course you can do this at the Resource Group level as well.

1. First, get your Automation Account's managed identity object ID:

- Go to your Automation Account
- Click "Identity" in the left menu
- Under "System assigned" tab, copy the "Object (principal) ID"


2. Set up role assignments:

- Navigate to the subscription or resource group containing the VMs you want to manage
- Click "Access control (IAM)"
- Click "+ Add" then "Add role assignment"
- Choose "Virtual Machine Contributor" role
- Under "Members" tab, select "Managed Identity"
- Click "Select members"
- Change "Managed Identity" type to "Automation Account"
- Select your automation account from the list
- Click "Select" then "Review + assign"


3. Test the permissions:

- Go back to your Automation Account
- Click "Runbooks"
- Create a new runbook (PowerShell type)
- Paste this simple test script:

## Test Script
```
try {
    # Explicitly connect using managed identity
    Connect-AzAccount -Identity | Out-Null

    $context = Get-AzContext
    if ($context) {
        Write-Output "============= Connection Information ============="
        Write-Output "Successfully connected using managed identity"
        Write-Output "Current context: $($context.Name)"
        Write-Output "Subscription: $($context.Subscription.Name)"
        Write-Output "Tenant: $($context.Tenant.Id)"
        
        Write-Output "`n============= VM Inventory ============="
        # Format the VM output more cleanly
        $vms = Get-AzVM | Select-Object Name, ResourceGroupName
        foreach ($vm in $vms) {
            Write-Output "VM Name: $($vm.Name)"
            Write-Output "Resource Group: $($vm.ResourceGroupName)"
            Write-Output "----------------------------------------"
        }
    } else {
        Write-Error "No Azure context found after connection attempt"
    }
} catch {
    Write-Error "Detailed Error: $_"
    Write-Error "Stack Trace: $($_.ScriptStackTrace)"
}
```
Save and publish the runbook
Click "Start" to run it
Check the output to verify it can list your VMs

## Account Running the Jobs needs the following permissions
- "Automation Job Operator" (minimum for running jobs)
- "Automation Operator" (can start/stop runbooks)
- "Automation Contributor" (full access to manage automation resources)

For testing purposes I normally add this at the subscription level for the account I am using to start the jobs.  Normally, you would likely use a managed identity for this.

# Powershell Script - EnumRunbooks.ps1
This script is designed to help you build a local JSON file that has all your runbook operations which will be exposed to the model for use.

For example purposes we will use a Local JSON file for the Runbook details and this will be injected into LLM so it has knowledge of what is availble.  As you list of Runbooks grow, 

# Simplicity vs. Overengineering:

For small sets of runbooks, a well-structured JSON in version control might be enough. It is simpler and lower cost.
For large sets or advanced discovery (“Find all runbooks that manage Azure VMs in West US 2”), leveraging Azure AI Search to power “natural language” queries is extremely useful.

# Use a consistent metadata pattern for all scripts
By using a consistent metadata pattern in each of the run books, we our PowerShell script will be able to extract the necessary data needed for out LLM to use.  When
you scale up and have hundreds of runbooks, it would be recommended that you leverage something like AI Search to create embeddings for these scripts so they can be 
dynamically retrieved at run time.

If yoru script does not requirement parameters simply don't include the .PARAMETER sections in the metadata.  Using this approach allows us to extract what we need for
indexing purposes or for the building of a JSON file that can be used to inject details about the available operations that can be performed.


```
    <#
        .SYNOPSIS
            Shuts down an Azure Virtual Machine using Managed Identity authentication.

        .DESCRIPTION
            This runbook safely stops a specified Azure Virtual Machine in a given resource group.
            It uses the Automation Account's Managed Identity for authentication and includes
            validation and error handling. The script checks the VM's existence and current
            power state before attempting shutdown.

        .METADATA
            AUTHOR: Rick Caudle
            LASTEDIT: December 19, 2024
            VERSION: 1.0.0
            CATEGORY: Operations
            TAGS: VM, PowerManagement, Shutdown
            DEPENDENCIES: Az.Accounts, Az.Compute

        .PARAMETER 
            Name: VNName
            Description: The name of the Virtual Machine to shut down
            Required: Yes
            Type: string
            Default: None

        .PARAMETER 
            Name: ResourceGroup
            Description: The resource group containing the Virtual Machine
            Required: Yes
            Type: string
            Default: None

        .NOTES
            - Requires a Managed Identity configured on the Automation Account
            - Managed Identity must have Contributor or VM Contributor rights on the VM
            - Performs a graceful shutdown, equivalent to "shutdown" from the OS
            - Waits for confirmation of shutdown before completing

        .EXAMPLE
            .\ShutDown-VM.ps1 -VMName "MyVM" -ResourceGroupName "MyRG"
    #>
```