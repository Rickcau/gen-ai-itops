<#
.SYNOPSIS
   Exports Azure Automation PowerShell or PowerShell72 runbook details to a JSON file.

.DESCRIPTION
   - Connects to Azure Automation and retrieves metadata about
     each runbook in a specified Automation Account.
   - Filters out non-PowerShell runbooks or runbooks that aren't published.
   - Attempts to parse the runbook's PowerShell code for a `param()` block.
   - Outputs all metadata to a JSON file.

.NOTES
   - This script assumes you're running the latest version of the Az PowerShell module
     and have sufficient privileges (Contributor or Owner) on the Automation Account.

.PARAMETER SubscriptionId
   The target Azure Subscription ID.

.PARAMETER ResourceGroupName
   The resource group containing the Automation Account.

.PARAMETER AutomationAccountName
   The name of the Automation Account.

.PARAMETER OutputPath
   Optional path to write the resulting JSON file. Defaults to .\runbooks.json.

.EXAMPLE
   .\EnumRunbooks.ps1 -SubscriptionId "00000000-0000-0000-0000-000000000000" `
       -ResourceGroupName "MyResourceGroup" `
       -AutomationAccountName "MyAutomationAccount" `
       -OutputPath "C:\Temp\runbooks.json"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$SubscriptionId,

    [Parameter(Mandatory = $true)]
    [string]$ResourceGroupName,

    [Parameter(Mandatory = $true)]
    [string]$AutomationAccountName,

    [Parameter(Mandatory = $false)]
    [string]$OutputPath = ".\runbooks.json"
)

# Function to extract comment-based help
# Function to extract comment-based help
function Get-ScriptCommentBasedHelp([string]$ScriptContent) {    
    $helpInfo = @{
        Description = ""
        Synopsis = ""
        Notes = ""
        Example = ""
        Metadata = @{
            Author = ""
            LastEdit = ""
            Version = ""
            Category = ""
            Tags = @()
            Dependencies = @()
        }
    }

    # Match the entire comment-based help block
    $helpMatch = [Regex]::Match($ScriptContent, '<#(.*?)#>', [System.Text.RegularExpressions.RegexOptions]::Singleline)
    if ($helpMatch.Success) {
        $helpBlock = $helpMatch.Groups[1].Value

        # Extract Description
        $descMatch = [Regex]::Match($helpBlock, '\.DESCRIPTION\s*(.*?)(\r?\n\s*\.|$)', [System.Text.RegularExpressions.RegexOptions]::Singleline)
        if ($descMatch.Success) {
            $helpInfo.Description = $descMatch.Groups[1].Value.Trim()
        }

        # Extract Synopsis
        $synopsisMatch = [Regex]::Match($helpBlock, '\.SYNOPSIS\s*(.*?)(\r?\n\s*\.|$)', [System.Text.RegularExpressions.RegexOptions]::Singleline)
        if ($synopsisMatch.Success) {
            $helpInfo.Synopsis = $synopsisMatch.Groups[1].Value.Trim()
        }

        # Extract Notes
        $notesMatch = [Regex]::Match($helpBlock, '\.NOTES\s*(.*?)(\r?\n\s*\.|$)', [System.Text.RegularExpressions.RegexOptions]::Singleline)
        if ($notesMatch.Success) {
            $helpInfo.Notes = $notesMatch.Groups[1].Value.Trim()
        }

        # Extract Example
        $exampleMatch = [Regex]::Match($helpBlock, '\.EXAMPLE\s*(.*?)(\r?\n\s*\.|$)', [System.Text.RegularExpressions.RegexOptions]::Singleline)
        if ($exampleMatch.Success) {
            $helpInfo.Example = $exampleMatch.Groups[1].Value.Trim()
        }

        # Extract Metadata
        $metadataMatch = [Regex]::Match($helpBlock, '\.METADATA\s*(.*?)(\r?\n\s*\.|$)', [System.Text.RegularExpressions.RegexOptions]::Singleline)
        if ($metadataMatch.Success) {
            $metadataBlock = $metadataMatch.Groups[1].Value

            # Extract individual metadata fields
            $authorMatch = [Regex]::Match($metadataBlock, 'AUTHOR:\s*(.*?)(\r?\n|$)')
            if ($authorMatch.Success) {
                $helpInfo.Metadata.Author = $authorMatch.Groups[1].Value.Trim()
            }

            $lastEditMatch = [Regex]::Match($metadataBlock, 'LASTEDIT:\s*(.*?)(\r?\n|$)')
            if ($lastEditMatch.Success) {
                $helpInfo.Metadata.LastEdit = $lastEditMatch.Groups[1].Value.Trim()
            }

            $versionMatch = [Regex]::Match($metadataBlock, 'VERSION:\s*(.*?)(\r?\n|$)')
            if ($versionMatch.Success) {
                $helpInfo.Metadata.Version = $versionMatch.Groups[1].Value.Trim()
            }

            $categoryMatch = [Regex]::Match($metadataBlock, 'CATEGORY:\s*(.*?)(\r?\n|$)')
            if ($categoryMatch.Success) {
                $helpInfo.Metadata.Category = $categoryMatch.Groups[1].Value.Trim()
            }

            $tagsMatch = [Regex]::Match($metadataBlock, 'TAGS:\s*(.*?)(\r?\n|$)')
            if ($tagsMatch.Success) {
                $helpInfo.Metadata.Tags = $tagsMatch.Groups[1].Value.Split(',').ForEach({ $_.Trim() })
            }

            $dependenciesMatch = [Regex]::Match($metadataBlock, 'DEPENDENCIES:\s*(.*?)(\r?\n|$)')
            if ($dependenciesMatch.Success) {
                $helpInfo.Metadata.Dependencies = $dependenciesMatch.Groups[1].Value.Split(',').ForEach({ $_.Trim() })
            }
        }
    }

    return $helpInfo
}

# Function to extract parameters from script
function Get-ScriptParameters([string]$ScriptContent) {
    $paramArray = @()

    # Find all .PARAMETER sections in the comment block
    $paramPattern = '\.PARAMETER\s*(.*?)(?=\r?\n\s*\.|$)'
    $paramMatches = [regex]::Matches($ScriptContent, $paramPattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)

    foreach ($match in $paramMatches) {
        $paramContent = $match.Groups[1].Value.Trim()
        
        # Extract the structured parameter metadata
        $nameMatch = [regex]::Match($paramContent, 'Name:\s*(\w+)')
        $descMatch = [regex]::Match($paramContent, 'Description:\s*([^\r\n]+)')
        $requiredMatch = [regex]::Match($paramContent, 'Required:\s*(Yes|No)', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        $typeMatch = [regex]::Match($paramContent, 'Type:\s*(\w+)')
        $defaultMatch = [regex]::Match($paramContent, 'Default:\s*([^\r\n]+)')

        if ($nameMatch.Success) {
            $paramObj = @{
                name = $nameMatch.Groups[1].Value
                type = if ($typeMatch.Success) { $typeMatch.Groups[1].Value } else { "string" }
                required = if ($requiredMatch.Success) { $requiredMatch.Groups[1].Value -eq "Yes" } else { $true }
                description = if ($descMatch.Success) { $descMatch.Groups[1].Value.Trim() } else { "" }
                default = if ($defaultMatch.Success -and $defaultMatch.Groups[1].Value.Trim() -ne "None") { 
                    $defaultMatch.Groups[1].Value.Trim() 
                } else { 
                    $null 
                }
            }

            $paramArray += $paramObj
        }
    }

    return $paramArray
}



# Import required modules
Import-Module Az.Accounts
Import-Module Az.Automation

try {
    Write-Host "Switching to subscription: $SubscriptionId"
    Set-AzContext -SubscriptionId $SubscriptionId

    Write-Host "Retrieving runbooks from Automation Account: $AutomationAccountName in RG: $ResourceGroupName"
    $runbooks = Get-AzAutomationRunbook `
        -ResourceGroupName $ResourceGroupName `
        -AutomationAccountName $AutomationAccountName

    if (!$runbooks -or $runbooks.Count -eq 0) {
        Write-Warning "No runbooks found in Automation Account."
        return
    }

    # We'll store all runbook metadata in this array
    $runbookArray = @()

    foreach ($runbook in $runbooks) {
        Write-Host "Processing runbook: $($runbook.Name)"

        # 1. Check if it's a PowerShell runbook
        if ($runbook.RunbookType -notin @("PowerShell", "PowerShell72")) {
            Write-Warning "Skipping non-PowerShell runbook: $($runbook.Name) (type=$($runbook.RunbookType))"
            continue
        }

        # 2. Check if the runbook is published
        if ($runbook.State -ne "Published") {
            Write-Warning "Skipping runbook not in published state: $($runbook.Name) (state=$($runbook.State))"
            continue
        }

        $runbookContent = ""
        try {
            # Create a temporary file to store the runbook content
            $tempFolder = [System.IO.Path]::GetTempPath()
            $tempFile = Join-Path $tempFolder "$($runbook.Name).ps1"
            
            Write-Host "Exporting runbook to: $tempFile"
            
            # Export the runbook
            Export-AzAutomationRunbook `
                -ResourceGroupName $ResourceGroupName `
                -AutomationAccountName $AutomationAccountName `
                -Name $runbook.Name `
                -OutputFolder $tempFolder `
                -Slot "Published"
            
            # Verify file exists and read content
            if (Test-Path $tempFile) {
                $runbookContent = Get-Content -Path $tempFile -Raw
                Write-Host "Successfully retrieved content for $($runbook.Name) (Length: $($runbookContent.Length) characters)"
                
                # Get comment-based help
                $helpInfo = Get-ScriptCommentBasedHelp -ScriptContent $runbookContent
                Write-Host "Description found: $($helpInfo.Description -ne $null)"
                
                # Get parameters
                $parameters = Get-ScriptParameters -ScriptContent $runbookContent
                Write-Host "Parameters found: $($parameters.Count)"
                
                # Clean up
                Remove-Item -Path $tempFile -Force
            } else {
                Write-Warning "Export succeeded but file not found at: $tempFile"
            }
        }
        catch {
            Write-Warning "Could not retrieve runbook content for $($runbook.Name): $_"
            $runbookContent = ""
        }

        # Build the final object for this runbook
        $runbookObj = @{
            name = $runbook.Name
            description = if ($helpInfo.Description) { $helpInfo.Description } else { $runbook.Description }
            synopsis = if ($helpInfo.Synopsis) { $helpInfo.Synopsis } else { "" }
            notes = $helpInfo.Notes
            example = $helpInfo.Example
            version = if ($helpInfo.Metadata.Version) { $helpInfo.Metadata.Version } else { "1.0.0" }
            author = $helpInfo.Metadata.Author
            lastEdit = $helpInfo.Metadata.LastEdit
            category = if ($helpInfo.Metadata.Category) { $helpInfo.Metadata.Category } else { $runbook.RunbookType }
            tags = if ($helpInfo.Metadata.Tags.Count -gt 0) { $helpInfo.Metadata.Tags } else { @() }
            dependencies = if ($helpInfo.Metadata.Dependencies.Count -gt 0) { $helpInfo.Metadata.Dependencies } else { @() }
            parameters = $parameters
        }

        $runbookArray += $runbookObj
    }

    # Convert the array to JSON
    $runbooksJson = $runbookArray | ConvertTo-Json -Depth 5

    # Save to file
    Write-Host "Writing runbook metadata to $OutputPath"
    Set-Content -Path $OutputPath -Value $runbooksJson

    Write-Host "Export complete. Runbooks metadata saved to $OutputPath"
}
catch {
    Write-Error $_.Exception.Message
}