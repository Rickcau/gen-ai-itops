# Gen-AI-ITOps: Generative AI-Powered IT Operations Accelerator
[![Dotnet CI](https://github.com/Rickcau/gen-ai-itops/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/Rickcau/gen-ai-itops/actions/workflows/dotnet-ci.yml) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

[![Next.js CI](https://github.com/Rickcau/gen-ai-itops/actions/workflows/nextjs-ci.yml/badge.svg)](https://github.com/Rickcau/gen-ai-itops/actions/workflows/nextjs-ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
## Overview
This repository contains a comprehensive solution for automating IT operations using Generative AI, Azure Automation, and modern web technologies. The platform enables users to interact with IT infrastructure through a natural language interface while maintaining proper security controls and role-based access.

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
- `/ConsoleApp-Chat-Bot/`: CLI interface for the chatbot
- `/ConsoleApp-Build-Ai-Index/`: Tools for building AI search indexes
- `/ConsoleApp-Az-Automation/`: Azure Automation management utilities
- `/ConsoleApp-Az-Check-Permissions/`: Permission verification tools

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
- Important Notes in [`/ImportantNotes.md/`](./notes/ImportantNotes.md)
- Just a few ideas / thoughts in [`/Thoughts.md/`](./notes/Thoughts.md)
- Setup Details in [`/Setup.md/`](./notes/Setup.md)
- Runbook documentation in [`RunBooks.md`](RunBooks.md)
- Architecture specifications in [`/architecture/`](./architecture)
- [`API documentation`](./api-gen-ai-itops) available in respective service directories

## License
This project is licensed under the MIT License - see the LICENSE file for details. 
