# Gen-AI-ITOps: Generative AI-Powered IT Operations Accelerator
[![Dotnet CI](https://github.com/Rickcau/gen-ai-itops/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/Rickcau/gen-ai-itops/actions/workflows/dotnet-ci.yml) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

[![Next.js CI](https://github.com/Rickcau/gen-ai-itops/actions/workflows/nextjs-ci.yml/badge.svg)](https://github.com/Rickcau/gen-ai-itops/actions/workflows/nextjs-ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

[![Video Title](./images/demo.jpg)](https://www.youtube.com/watch?v=huwIEobrw7w)

[<img src="./images/demo.jpg" alt="Video Title" width="400"/>](https://www.youtube.com/watch?v=huwIEobrw7w)



## Licensing 
This project uses a dual license model:

- **Commercial License**: Required for Azure-hosted production deployments
- **MIT License**: Applies to open source use cases

See [LICENSE.md](LICENSE.md) for details.

## Overview
This repository contains a comprehensive solution for automating IT operations using Generative AI, Azure Automation, GitHub Workflows, and modern web technologies. The platform enables users to interact with IT infrastructure through a natural language interface while maintaining proper security controls and role-based access.

## Key Components

### 1. IT Operations ChatBot
- Located in `/it-ops-chatbot/`
- Next.js-based web application
- Provides natural language interface for IT operations
- Implements secure authentication and authorization

### 2. Azure Automation Integration
- PowerShell-based runbooks for infrastructure operations
- Custom RBAC roles for access control
- Automated task scheduling and execution
- Job status monitoring and reporting

### 3. Console Applications
- `/ConsoleApp-Chat-Bot/`: CLI interface which demostrate ChatBot capabilities that are used in the API Layer
- `/ConsoleApp-Build-Ai-Index/`: Tools for building AI search indexes
- `/ConsoleApp-Az-Automation/`: Azure Automation management utilities / demostrates RBAC with Azure Automation
- `/ConsoleApp-Az-Check-Permissions/`: Permission verification tools / denostrates how custom RBAC roles can be used with the solution.

### 4. API Layer
- `/api-gen-ai-itops/`: Backend API services
- Handles authentication and authorization
- Integrates with Azure services
- Manages communication between frontend and automation components

## Security Features
- Azure Entra ID integration
- Custom RBAC roles (VMAdmin, VMViewer, etc.)
- Secure token-based authentication
- Role-based access control for operations

## Architecture
- Detailed architecture documentation in `/architecture/`
- Implements separation of concerns
- Scalable and maintainable design
- Integration with Azure services

## Development Setup
1. Configure App.config with environment variables
2. Set up Azure Automation account
3. Configure App Registrations in Azure Entra ID
4. Assign appropriate roles to users

## CI/CD
- GitHub Actions workflows for automated testing and deployment
- Continuous integration checks for .NET components
- Automated build and deployment processes

## Documentation
- Implementation details in [`/notes/`](./notes)
- Please read [`Azure Automation VS GitHub`](./notes/AzureAuto-vs-GitHub.md) to help determine which to use
- Important Notes in [`/ImportantNotes.md/`](./notes/ImportantNotes.md)
- Just a few ideas / thoughts in [`/Thoughts.md/`](./notes/Thoughts.md)
- Setup Details in [`/Setup.md/`](./notes/Setup.md)
- Runbook documentation in [`RunBooks.md`](RunBooks.md)
- Architecture specifications in [`/architecture/`](./architecture)
- [`API documentation`](./api-gen-ai-itops) available in respective service directories

## Additional Work that is needed
- Add additional logic to the Console-Build-AI-Index which enumerates the GitHub Workflows that the users have access to
- More work is needed for the GitHub integration as the platform does not provide the robust capabilities that Azure Automation has
- Determining how to lock down access to varioaus GitHub actions would be needed.

