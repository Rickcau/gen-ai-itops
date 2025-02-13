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
        Name: VMName
        Description: The name of the Virtual Machine to shut down
        Required: Yes
        Type: string
        Default: None

    .PARAMETER 
        Name: ResourceGroupName
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

$VerbosePreference = "Continue"
$InformationPreference = "Continue"

param(
    [Parameter(Mandatory = $true)]
    [string]$VNName,

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
    # Also use Write-Progress for real-time status
    Write-Progress -Activity "VM Shutdown" -Status "$Message"
}

try {
    Write-LogMessage "Starting VM shutdown script"
    Write-LogMessage "Parameters: VNName=$VNName, ResourceGroupName=$ResourceGroupName"
    
    # Connect using managed identity
    Write-LogMessage "Connecting to Azure..."
    $connection = Connect-AzAccount -Identity
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
        throw "Failed to establish Azure context using Managed Identity"
    }

    Write-LogMessage "Connected to subscription: $($context.Subscription.Name)"

    # Verify VM exists
    Write-LogMessage "Verifying VM '$VNName'"
    $vm = Get-AzVM -ResourceGroupName $ResourceGroupName -Name $VNName -ErrorAction Stop
    Write-LogMessage "VM found: $($vm.Name)"
    
    if (!$vm) {
        throw "VM '$VMName' not found in resource group '$ResourceGroupName'"
    }

    # Get current VM status
    $vmStatus = Get-AzVM -ResourceGroupName $ResourceGroupName -Name $VNName -Status
    $powerState = ($vmStatus.Statuses | Where-Object Code -like "PowerState/*").DisplayStatus
    Write-LogMessage "Current VM power state: $powerState"

    if ($powerState -eq "VM deallocated") {
        Write-LogMessage "VM is already shut down."
        return
    }

    # Initiate shutdown
    Write-LogMessage "Initiating shutdown..."
    $result = Stop-AzVM -ResourceGroupName $ResourceGroupName -Name $VNName -Force
    
    if ($result.Status -eq "Succeeded") {
        Write-LogMessage "VM shutdown completed successfully"
    } else {
        throw "VM shutdown failed. Status: $($result.Status)"
    }

} catch {
    $errorMessage = $_.Exception.Message
    $stackTrace = $_.ScriptStackTrace
    Write-LogMessage "Error occurred: $errorMessage"
    Write-LogMessage "Stack Trace: $stackTrace"
    # Write error directly to error stream
    Write-Error "Failed to shut down VM: $errorMessage"
    throw
}
