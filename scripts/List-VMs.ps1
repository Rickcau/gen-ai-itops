<#
    .SYNOPSIS
        Lists all Azure VMs in the current subscription using Managed Identity authentication.

    .DESCRIPTION
        This runbook connects to Azure using the Automation Account's Managed Identity,
        displays the connection context information, and then retrieves and displays
        a formatted list of all Virtual Machines in the subscription, including their
        power state and status. It includes error handling and detailed output formatting
        for better readability.

    .METADATA
        AUTHOR: Rick Caudle
        LASTEDIT: December 29, 2024
        VERSION: 1.1.0
        CATEGORY: Infrastructure
        TAGS: VM, Inventory, ManagedIdentity, Status
        DEPENDENCIES: Az.Accounts, Az.Compute

    .NOTES
        - Requires a Managed Identity configured on the Automation Account
        - Requires the Managed Identity to have appropriate RBAC permissions to list VMs
        - Does not require any parameters as it uses the context of the Managed Identity
        - Provides formatted output suitable for both human reading and log analysis

    .EXAMPLE
        The script requires no parameters and can be run directly in Azure Automation:
        .\List-VMs.ps1
#>

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
        # Get all VMs with their status
        $vms = Get-AzVM -Status | Select-Object Name, ResourceGroupName, PowerState, @{
            Name='ProvisioningState';
            Expression={$_.Statuses[-1].DisplayStatus}
        }, Location, @{
            Name='OS';
            Expression={if ($_.OsProfile.WindowsConfiguration) {'Windows'} else {'Linux'}}
        }

        foreach ($vm in $vms) {
            Write-Output "VM Name: $($vm.Name)"
            Write-Output "Resource Group: $($vm.ResourceGroupName)"
            Write-Output "Power State: $($vm.PowerState)"
            Write-Output "Provisioning State: $($vm.ProvisioningState)"
            Write-Output "Location: $($vm.Location)"
            Write-Output "Operating System: $($vm.OS)"
            Write-Output "----------------------------------------"
        }

        # Add summary statistics
        Write-Output "`n============= Summary Statistics ============="
        $runningVMs = ($vms | Where-Object {$_.PowerState -eq "VM running"}).Count
        $deallocatedVMs = ($vms | Where-Object {$_.PowerState -eq "VM deallocated"}).Count
        $otherStateVMs = ($vms | Where-Object {$_.PowerState -notmatch "VM running|VM deallocated"}).Count
        
        Write-Output "Total VMs: $($vms.Count)"
        Write-Output "Running VMs: $runningVMs"
        Write-Output "Deallocated VMs: $deallocatedVMs"
        Write-Output "Other States: $otherStateVMs"
    } else {
        Write-Error "No Azure context found after connection attempt"
    }
} catch {
    Write-Error "Detailed Error: $_"
    Write-Error "Stack Trace: $($_.ScriptStackTrace)"
}
