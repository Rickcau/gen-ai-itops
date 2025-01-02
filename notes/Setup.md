# Simplicity vs. Overengineering:

For small sets of runbooks, a well-structured JSON in version control might be enough. It is simpler and lower cost.
For large sets or advanced discovery (“Find all runbooks that manage Azure VMs in West US 2”), leveraging Azure AI Search to power “natural language” queries is extremely useful.

I will be opting for AI Search even though we will only have 4 PowerShell Scripts deployed in Azure Automation.   I do have a PowerShell Script that can be used to enumerate the details of the Runbooks that are deployed into Azure Automation.

This same concept can be used to perform automation for over services like GitHub Actions, the only caveat is you will need an API to interact with.  The great thing about Azure Automation is that it provides all the scheduling and Job Status capabilities that are needed.  So as long as you can find some way to achieve that with other systems that same concepts apply.   
