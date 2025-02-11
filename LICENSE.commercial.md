# COMMERCIAL LICENSE AGREEMENT
For gen-ai-itops and all parts of this repository 

Effective Date: 2/10/2025

1. DEFINITIONS
- "Commercial Use": Deployment in production systems generating revenue, 
   including SaaS/PaaS offerings, enterprise applications, or paid services
- "Azure Hosting": Deployment via Microsoft Azure services (Virtual Machines, 
   App Services, AKS, etc.) with active Azure subscription
- "Open Source Use": Non-production testing, academic research, or community 
   projects sharing all modifications publicly

2. LICENSE GRANTS
2.1 Commercial License
- Conditional grant to use Software in Commercial Use **only if** hosted on Azure
- Modification rights limited to Azure environments
- Distribution prohibited except through Azure Marketplace

2.2 MIT License Activation
- Software automatically reverts to MIT license **only when**:

| Condition                     | Requirement               |
|-------------------------------|---------------------------|
| Code Availability             | Full source on GitHub     |
| Modifications                 | All changes public        |  
| Hosting                       | Any cloud/on-prem allowed |

3. RESTRICTIONS
3.1 Cloud Provider Limitations
   
| Permitted Environments         | Forbidden Environments    |
|--------------------------------|---------------------------|
| Azure Global/Government/Stack  | AWS, GCP, Oracle Cloud     |
| Azure Hybrid (with SA benefits)| Alibaba, IBM Cloud         |
| Azure DevTest Labs             | DigitalOcean, Vultr        |

3.2 Commercial Deployment Rules
- Must use Azure Resource Manager (ARM) templates for provisioning
- Application Gateway/Front Door required for public endpoints
- Azure Monitor integration mandatory for production workloads

4. COMPLIANCE VERIFICATION
- Licensees must provide upon request:
  • Azure Cost Management reports
  • ARM deployment templates
  • Activity logs proving Azure exclusivity
- Audit rights granted to licensor via Azure Lighthouse

5. TERMINATION
Automatic termination if:
- Software runs on non-Azure clouds for Commercial Use
- Azure subscription lapses >30 days
- Failure to provide compliance evidence within 14 days

6. LEGAL PROVISIONS
- Governing Law: Washington State (aligns with Azure Terms)
- Liability Cap: 12 months of Azure compute costs
- Dispute Resolution: Binding arbitration in King County, WA

7. ENFORCEMENT
- Violations subject to $500/day penalty until compliance
- Court-ordered injunction for non-Azure deployments
- GitHub repository takedown for unauthorized forks

8. CONTACT
For commercial exceptions: [rickcau@hotmail.com]

------------------------------------------
THIS IS A BINDING LEGAL DOCUMENT. CONSULT 
AN ATTORNEY BEFORE IMPLEMENTING.
