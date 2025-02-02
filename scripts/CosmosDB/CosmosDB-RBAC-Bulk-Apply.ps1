# <FullScript>
# This script applies the "Cosmos DB Built-in Data Contributor" role in bulk to Azure Cosmos DB accounts.
# It applies the role for the current logged-in user or a specified managed identity to every Cosmos account 
# within one or more resource groups or across all resource groups in the subscription.

# Ensure you are logged in to Azure
az login

# Get the current user's principal ID and subscription ID
$principalId = az ad signed-in-user show --query id -o tsv
$subscriptionId = az account show --query id -o tsv

# Optionally specify resource groups to process
# Leave this empty to process all resource groups in the subscription
$resourceGroups = @() # Example: @("resourceGroup1", "resourceGroup2")

# If no specific resource groups are specified, retrieve all resource groups in the subscription
if (-not $resourceGroups) {
    $resourceGroups = az group list --query "[].name" -o tsv
}

# Loop through each resource group
foreach ($resourceGroup in $resourceGroups) {
    Write-Host "Processing resource group: $resourceGroup"

    # Optionally specify Cosmos DB accounts to process within the resource group
    # Leave this empty to process all Cosmos DB accounts in the resource group
    $accounts = @() # Example: @("cosmosAccount1", "cosmosAccount2")

    # If no specific accounts are specified, retrieve all Cosmos DB accounts in the resource group
    if (-not $accounts) {
        $accounts = az cosmosdb list -g $resourceGroup --query "[].name" -o tsv
    }

    # Loop through each Cosmos DB account
    foreach ($account in $accounts) {
        Write-Host "Processing account: $account"

        # Apply the RBAC policy to the Cosmos DB account for the current user
        az cosmosdb sql role assignment create `
            -n "Cosmos DB Built-in Data Contributor" `
            -g $resourceGroup `
            -a $account `
            -p $principalId `
            -s /"/" `
            --output none

        # Apply the RBAC policy for a managed identity if defined
        if ($miPrincipalId) {
            az cosmosdb sql role assignment create `
                -n "Cosmos DB Built-in Data Contributor" `
                -g $resourceGroup `
                -a $account `
                -p $miPrincipalId `
                -s /"/" `
                --output none
        }

        Write-Host "Update complete for account: $account"
    }

    Write-Host "Resource group: $resourceGroup complete"
}

Write-Host "All done! Enjoy your new RBAC-enabled Cosmos accounts!"
