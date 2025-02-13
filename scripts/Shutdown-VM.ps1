<#
    .DESCRIPTION
        This runbook safely stops a specified Azure Virtual Machine in a given resource group.
        It uses the Automation Account's Managed Identity for authentication and includes
        validation and error handling. The script checks the VM's existence and current
        power state before attempting shutdown.
    .EXAMPLE
        .\ShutDown-VM.ps1 -VMName "MyVM" -ResourceGroupName "MyRG"
#>

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
    Write-Output "[$timestamp][$Level] $Message"
}

try {
    Write-LogMessage "Starting VM shutdown script"
    Write-LogMessage "Parameters: VMName=$VMName, ResourceGroupName=$ResourceGroupName"
    
    # Connect using managed identity
    Write-LogMessage "Connecting to Azure..."
    $connection = Connect-AzAccount -Identity | Out-Null
    Write-LogMessage "Connection attempt completed"

    # Get and log context information
    $context = Get-AzContext
    Write-LogMessage "Identity Type: $($context.Account.Type)"
    Write-LogMessage "Identity Used: $($context.Account.Id)"
    Write-LogMessage "Checking permissions..."
    
    Get-AzRoleAssignment | ForEach-Object {
        Write-LogMessage "Role: $($_.RoleDefinitionName) Scope: $($_.Scope)"
    }

    # Verify we have a valid connection
    if (!$context) {
        Write-LogMessage "Failed to establish Azure context using Managed Identity" "Error"
        throw "Failed to establish Azure context using Managed Identity"
    }

    Write-LogMessage "Connected to subscription: $($context.Subscription.Name)"

    # Verify VM exists
    Write-LogMessage "Verifying VM '$VMName'"
    $vm = Get-AzVM -ResourceGroupName $ResourceGroupName -Name $VMName -ErrorAction Stop
    
    if (!$vm) {
        Write-LogMessage "VM '$VMName' not found in resource group '$ResourceGroupName'" "Error"
        throw "VM '$VMName' not found in resource group '$ResourceGroupName'"
    }

    Write-LogMessage "VM found: $($vm.Name)"
    
    # Get current VM status
    $vmStatus = Get-AzVM -ResourceGroupName $ResourceGroupName -Name $VMName -Status
    $powerState = ($vmStatus.Statuses | Where-Object Code -like "PowerState/*").DisplayStatus
    Write-LogMessage "Current VM power state: $powerState"

    if ($powerState -eq "VM deallocated") {
        Write-LogMessage "VM is already shut down"
        return
    }

    # Initiate shutdown
    Write-LogMessage "Initiating shutdown..."
    $result = Stop-AzVM -ResourceGroupName $ResourceGroupName -Name $VMName -Force
    
    if ($result.Status -eq "Succeeded") {
        Write-LogMessage "VM shutdown completed successfully"
    } else {
        Write-LogMessage "VM shutdown failed. Status: $($result.Status)" "Error"
        throw "VM shutdown failed. Status: $($result.Status)"
    }

} catch {
    $errorMessage = $_.Exception.Message
    $stackTrace = $_.ScriptStackTrace
    Write-LogMessage "Error occurred: $errorMessage" "Error"
    Write-LogMessage "Stack Trace: $stackTrace" "Error"
    throw $errorMessage
}
