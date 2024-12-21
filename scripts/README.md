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
