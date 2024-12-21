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