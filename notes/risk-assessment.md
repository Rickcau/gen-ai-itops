# Risk Assessment: AI-Assisted IT Operations Management Approaches

**Author: Rick Caudle**  
**Date: March 24, 2025**

## Executive Summary

This document assesses the risks and benefits of different approaches to implementing AI-assisted IT Operations management for cloud resources, with a particular focus on Azure environments. We compare capability-based approaches that utilize registered functions and APIs against screen/browser control approaches (like OpenAI's Operator, Anthropic's Computer Use, and Google's Project Mariner). Our findings indicate that capability-based approaches offer superior security, auditability, and reliability for enterprise IT operations environments.

## Approach Comparison

### Capability-Based Approach

**Implementation Example**: Gen-AI-ITOps repository approach

**Key Characteristics**:
- Registers specific operations (e.g., VM start/stop) with defined parameters
- Uses vector search to match natural language queries to capabilities
- Executes operations through official APIs with specific permissions
- Provides a controlled sandbox for AI to operate within

**Risk Assessment**:

| Risk Category | Risk Level | Notes |
|---------------|------------|-------|
| Unintended Operations | Low | Limited to pre-registered capabilities with defined parameters |
| Audit Trail | Low | All operations can be logged with clear provenance |
| Authorization Control | Low | Can integrate with existing RBAC systems |
| Maintainability | Medium | Requires registering new capabilities as needs evolve |
| Implementation Complexity | Medium | Requires initial setup of capability registry and vector database |

### Screen/Browser Control Approach

**Implementation Examples**: OpenAI Operator, Anthropic Computer Use, Google Project Mariner

**Key Characteristics**:
- AI agent controls a browser or computer interface
- Navigates portal UIs to perform operations
- Uses visual understanding to interpret screens
- Executes actions based on interpreting UI elements

**Risk Assessment**:

| Risk Category | Risk Level | Notes |
|---------------|------------|-------|
| Unintended Operations | High | Potential to navigate to unintended areas or misinterpret instructions |
| Audit Trail | High | Difficult to track specific actions and changes |
| Authorization Control | High | Uses same permissions as logged-in user with limited granularity |
| Maintainability | High | UI changes can break functionality without warning |
| Implementation Complexity | Low | Less initial setup but higher ongoing maintenance |

## Detailed Findings

### Security Considerations

**Capability-Based Approach**:
- Provides principle of least privilege through well-defined operation boundaries
- Cannot perform operations outside registered capabilities
- Can implement approval workflows for sensitive operations
- Easy to integrate with existing security frameworks

**Screen Control Approach**:
- Has full access to whatever the screen user has access to
- Difficult to prevent access to sensitive areas of portals
- Potential for credential exposure in browser contexts
- Limited ability to implement fine-grained permissions

### Reliability and Maintainability

**Capability-Based Approach**:
- Resilient to UI changes in portals
- Direct API integration ensures stable operations
- Clear error handling and validation
- Consistent behavior across operations

**Screen Control Approach**:
- Highly susceptible to UI redesigns breaking functionality
- Dependent on visual interpretation which can be error-prone
- Unpredictable performance with complex portal interfaces
- Increased latency due to visual processing requirements

### Compliance and Governance

**Capability-Based Approach**:
- Clear audit trails of all performed operations
- Ability to enforce approval workflows
- Easy integration with compliance monitoring tools
- Controlled parameter validation prevents malicious inputs

**Screen Control Approach**:
- Limited visibility into exact actions performed
- Difficult to integrate with compliance frameworks
- Challenging to implement consistent governance policies
- Higher risk of human-like errors that bypass governance controls

## Recommendations

1. **Preferred Approach**: The capability-based approach represents the safest and most reliable method for implementing AI-assisted IT operations, particularly in enterprise environments.

2. **When to Consider Screen Control**: Screen control approaches may be appropriate for:
   - Personal productivity use cases
   - Non-critical environments
   - Scenarios where APIs are unavailable
   - Rapid prototyping before implementing API-based solutions

3. **Hybrid Considerations**: For certain use cases, a hybrid approach might be warranted where:
   - Critical operations use capability-based execution
   - Informational queries might leverage screen reading
   - Initial discovery leverages UI but execution uses APIs

4. **Implementation Safeguards**: If screen control must be used, implement:
   - Restricted environments with limited permissions
   - Continuous monitoring of activities
   - Regular validation of workflows as UIs change
   - Clear audit capture tools that record screen activities

## Conclusion

While screen control approaches like OpenAI Operator, Anthropic Computer Use, and Google Project Mariner offer enticing simplicity and flexibility, they introduce significant risks when applied to IT operations management. The capability-based approach demonstrated in the Gen-AI-ITOps repository provides a more robust, secure, and auditable framework that aligns with enterprise security requirements and best practices.

For organizations seeking to implement AI-assisted IT operations, we strongly recommend focusing on capability-based implementations that leverage official APIs and well-defined operation boundaries rather than screen control mechanisms.
