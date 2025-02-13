# Start-VM Runbook
# This runbook starts a specified Azure Virtual Machine in a given resource group. 
# It uses the Automation Account's Managed Identity for authentication and includes validation and error handling. 
# The script checks the VM's existence and current power state before attempting to start it.
# Streams the results to the output window.

param(
    [Parameter(Mandatory = $true)]
    [string]$VMName,

    [Parameter(Mandatory = $true)]
    [string]$ResourceGroupName
)

function Write-LogMessage {
    param(
        [string]$Message,
        [string]$Level = "Information"
    )
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    # Use Write-Output for immediate visibility
    Write-Output "[$timestamp][$Level] $Message"
}

try {
    # Connect using managed identity
    Write-LogMessage "Connecting to Azure using Managed Identity..."
    Connect-AzAccount -Identity | Out-Null

    # Verify we have a valid connection
    $context = Get-AzContext
    if (!$context) {
        Write-LogMessage "Failed to establish Azure context using Managed Identity" "Error"
        throw "Failed to establish Azure context using Managed Identity"
    }

    Write-LogMessage "Connected successfully to subscription: $($context.Subscription.Name)"

    # Verify VM exists
    Write-LogMessage "Verifying VM '$VMName' in resource group '$ResourceGroupName'..."
    $vm = Get-AzVM -ResourceGroupName $ResourceGroupName -Name $VMName -ErrorAction Stop
    
    if (!$vm) {
        Write-LogMessage "VM '$VMName' not found in resource group '$ResourceGroupName'" "Error"
        throw "VM '$VMName' not found in resource group '$ResourceGroupName'"
    }

    # Get current VM status
    $vmStatus = Get-AzVM -ResourceGroupName $ResourceGroupName -Name $VMName -Status
    $powerState = ($vmStatus.Statuses | Where-Object Code -like "PowerState/*").DisplayStatus

    Write-LogMessage "Current VM power state: $powerState"

    if ($powerState -eq "VM running") {
        Write-LogMessage "VM is already running."
        return
    }

    # Initiate startup
    Write-LogMessage "Initiating startup of VM '$VMName'..."
    $result = Start-AzVM -ResourceGroupName $ResourceGroupName -Name $VMName
    
    if ($result.Status -eq "Succeeded") {
        Write-LogMessage "VM startup completed successfully"
    } else {
        Write-LogMessage "VM startup failed. Status: $($result.Status)" "Error"
        throw "VM startup failed. Status: $($result.Status)"
    }

} catch {
    Write-LogMessage "Error starting VM: $_" "Error"
    Write-LogMessage "Stack Trace: $($_.ScriptStackTrace)" "Error"
    throw
}
