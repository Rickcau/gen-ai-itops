# Get-AppServicePlans.ps1 
# Lists all App Service Plans in the subscription with their details
# .\Get-AppServicePlans.ps1

function Write-LogMessage {
    param(
        [string]$Message,
        [string]$Level = "Information"
    )
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Output "[$timestamp][$Level] $Message"
}

try {
    # Authenticate using managed identity
    Write-LogMessage "Connecting to Azure using Managed Identity..."
    Connect-AzAccount -Identity | Out-Null
    
    # Get current context and subscription
    $context = Get-AzContext
    Write-LogMessage "Connected to subscription: $($context.Subscription.Name)"
    
    # Get all resource groups
    Write-LogMessage "Retrieving all resource groups..."
    $resourceGroups = Get-AzResourceGroup
    
    Write-LogMessage "Found $($resourceGroups.Count) resource groups"
    
    # Initialize array to store App Service Plan details
    $allAppServicePlans = @()
    
    # Loop through each resource group
    foreach ($rg in $resourceGroups) {
        Write-LogMessage "Checking resource group: $($rg.ResourceGroupName)"
        
        # Get App Service Plans in this resource group
        $plans = Get-AzAppServicePlan -ResourceGroupName $rg.ResourceGroupName
        
        if ($plans) {
            Write-LogMessage "Found $($plans.Count) App Service Plan(s) in $($rg.ResourceGroupName)"
            
            foreach ($plan in $plans) {
                $planDetails = [PSCustomObject]@{
                    Name = $plan.Name
                    ResourceGroup = $rg.ResourceGroupName
                    Location = $plan.Location
                    Tier = $plan.Sku.Tier
                    Size = $plan.Sku.Name
                    Capacity = $plan.Sku.Capacity
                    Status = $plan.Status
                }
                
                $allAppServicePlans += $planDetails
                
                Write-LogMessage "  - Plan: $($plan.Name)"
                Write-LogMessage "    Tier: $($plan.Sku.Tier)"
                Write-LogMessage "    Size: $($plan.Sku.Name)"
                Write-LogMessage "    Capacity: $($plan.Sku.Capacity)"
                Write-LogMessage "    Status: $($plan.Status)"
            }
        } else {
            Write-LogMessage "No App Service Plans found in $($rg.ResourceGroupName)"
        }
    }
    
    # Output summary
    Write-LogMessage "Summary: Found total of $($allAppServicePlans.Count) App Service Plans"
    
    # Return the array of App Service Plans
    return $allAppServicePlans

} catch {
    Write-LogMessage "An error occurred: $_" "Error"
    Write-LogMessage "Stack Trace: $($_.ScriptStackTrace)" "Error"
    throw
}
