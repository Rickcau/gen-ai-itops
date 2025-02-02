# Azure AI Search Index Schema for IT Operations Capabilities

This document outlines the design and schema for an Azure AI Search Index that supports hybrid search capabilities. The schema is tailored for IT operations use cases, such as executing runbooks, triggering GitHub actions, and querying Azure Resource Graph.

---

## Schema Design

### **Index Name**
`capabilities-index`

### **Fields**

| Field Name       | Type                       | Key  | Searchable | Filterable | Sortable | Facetable |
|------------------|----------------------------|------|------------|------------|----------|-----------|
| `id`            | `Edm.String`              | Yes  | No         | Yes        | Yes      | No        |
| `name`          | `Edm.String`              | No   | Yes        | Yes        | Yes      | No        |
| `description`   | `Edm.String`              | No   | Yes        | No         | No       | No        |
| `tags`          | `Collection(Edm.String)`  | No   | Yes        | Yes        | No       | Yes       |
| `parameters`    | `Edm.ComplexType`         | No   | No         | Partial    | No       | No        |
| `executionMethod`| `Edm.ComplexType`        | No   | No         | Partial    | No       | No        |
| `embedding`     | `Collection(Edm.Single)`  | No   | No         | No         | No       | No        |
| `createdAt`     | `Edm.DateTimeOffset`      | No   | No         | Yes        | Yes      | No        |
| `updatedAt`     | `Edm.DateTimeOffset`      | No   | No         | Yes        | Yes      | No        |

### **Complex Fields**

#### **Parameters**
Defines the parameters required to execute a capability.

```json
{
  "parameters": [
    { "name": "vmName", "type": "string" },
    { "name": "storageAccount", "type": "string" }
  ]
}
```

#### **Execution Method**
Describes how a capability is executed.

```json
{
  "executionMethod": {
    "type": "AutomationRunbook",
    "details": "RunbookName: BackupVM"
  }
}
```

---

## How the Schema Supports Hybrid Search

### **Semantic Search**
- The `embedding` field stores vector embeddings for semantic similarity searches using Azure OpenAI models.

### **Metadata Filtering**
- **Filterable Fields**: `tags`, `parameters.type`, `executionMethod.type`, `createdAt`, and `updatedAt`.
- **Faceted Search**: The `tags` field supports faceted search for easier filtering.

### **Exact Match Search**
- The `name` field allows users to search for specific capabilities directly.

---

## Example Document in the Index

```json
{
  "id": "capability_001",
  "name": "BackupVMRunbook",
  "description": "Backs up virtual machines to Azure Storage.",
  "tags": ["backup", "virtual machines", "Azure"],
  "parameters": [
    { "name": "vmName", "type": "string" },
    { "name": "storageAccount", "type": "string" }
  ],
  "executionMethod": {
    "type": "AutomationRunbook",
    "details": "RunbookName: BackupVM"
  },
  "embedding": [0.012, 0.345, ..., 0.876],
  "createdAt": "2025-01-26T10:00:00Z",
  "updatedAt": "2025-01-26T10:00:00Z"
}
```

---

## Workflow for Adding and Searching Capabilities

### **Adding a Capability**
1. Admin registers a new capability with:
   - Name
   - Description
   - Tags
   - Parameters
   - Execution method details
2. Metadata is stored in CosmosDB.
3. Embeddings are generated using Azure OpenAI and added to the index.

### **Searching for Capabilities**
1. User submits a query (e.g., "How do I back up my VMs?").
2. Perform a **hybrid search**:
   - **Semantic Search**: Use the `embedding` field for relevance ranking.
   - **Metadata Filtering**: Filter results by tags or execution type.
3. Return the top results to the user.

### **Executing a Capability**
1. Retrieve execution metadata from the index.
2. Invoke the corresponding execution method (e.g., Automation Runbook, GitHub Action, Azure Resource Graph query).

---

## Next Steps

1. **Index Creation**:
   - Use Azure Cognitive Search’s REST API or Azure Portal to create the index.
2. **Populate the Index**:
   - Sync data from CosmosDB and generate embeddings for semantic search.
3. **Implement Hybrid Search Logic**:
   - Combine semantic similarity with metadata filtering.
4. **Enhance Admin Interface**:
   - Provide tools for adding, updating, and managing capabilities.

---

This schema provides a scalable and extensible foundation for managing IT operations capabilities using Azure AI Search. Let me know if additional details are needed!
