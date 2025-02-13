# Reset AppServiecPlan to Premium V3
# Just another example of an IT Operation.
# .\Reset-App-Service-Plan.ps1 -ResourceGroupName "MyRG" -AppServicePlanName "AppService1" -SubscriptionId "dkjdlkfjdlk53456345"

param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$AppServicePlanName,
    
    [Parameter(Mandatory=$true)]
    [string]$SubscriptionId
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
    # Authenticate using managed identity
    Write-LogMessage "Connecting to Azure using Managed Identity..."
    Connect-AzAccount -Identity | Out-Null
    
    # Set the subscription context
    Write-LogMessage "Setting subscription context to: $SubscriptionId"
    Set-AzContext -SubscriptionId $SubscriptionId | Out-Null
    
    # Get the current App Service Plan
    Write-LogMessage "Retrieving App Service Plan: $AppServicePlanName in resource group: $ResourceGroupName"
    $asp = Get-AzAppServicePlan -ResourceGroupName $ResourceGroupName -Name $AppServicePlanName
    
    if (!$asp) {
        Write-LogMessage "App Service Plan not found" "Error"
        throw "App Service Plan '$AppServicePlanName' not found in resource group '$ResourceGroupName'"
    }

    Write-LogMessage "Current App Service Plan tier: $($asp.Sku.Tier), Size: $($asp.Sku.Name)"
    
    # Check if current tier is lower than Premium V3
    $needsUpgrade = $false
    
    if ($asp.Sku.Tier -eq "Free" -or 
        $asp.Sku.Tier -eq "Shared" -or 
        $asp.Sku.Tier -eq "Basic" -or 
        $asp.Sku.Tier -eq "Standard" -or 
        ($asp.Sku.Tier -eq "Premium" -and $asp.Sku.Name -notlike "*V3*")) {
        $needsUpgrade = $true
    }
    
    if ($needsUpgrade) {
        Write-LogMessage "Current tier ($($asp.Sku.Tier)) is lower than Premium V3, initiating upgrade..."
        
        Write-LogMessage "Upgrading App Service Plan to PremiumV3 tier..."
        # Set new App Service Plan size
        Set-AzAppServicePlan -ResourceGroupName $ResourceGroupName `
                            -Name $AppServicePlanName `
                            -Tier "PremiumV3" `
                            -WorkerSize "Small" `
                            -NumberofWorkers $asp.Sku.Capacity | Out-Null
        
        Write-LogMessage "Successfully upgraded App Service Plan to Premium V3"
    } else {
        Write-LogMessage "App Service Plan is already at Premium V3 or higher tier. No upgrade needed."
    }

    # Verify the upgrade
    $updatedAsp = Get-AzAppServicePlan -ResourceGroupName $ResourceGroupName -Name $AppServicePlanName
    Write-LogMessage "Final App Service Plan configuration - Tier: $($updatedAsp.Sku.Tier), Size: $($updatedAsp.Sku.Name)"
}
catch {
    Write-LogMessage "An error occurred: $_" "Error"
    Write-LogMessage "Stack Trace: $($_.ScriptStackTrace)" "Error"
    throw
}
