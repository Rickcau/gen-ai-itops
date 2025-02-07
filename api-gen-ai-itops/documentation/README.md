# IT Ops Chatbot API Documentation

This document provides detailed information about the IT Ops Chatbot API endpoints, resources, and methods.

## Base URL

The API endpoints are directly accessible at the root path of the server. For example:

- Development: `https://localhost:7049` or `http://localhost:5205`
- Production: Your deployed API URL

No version prefix is required in the URL path.

## Resources

### 1. Users

Manages user accounts and their data in the system.

#### Endpoints

##### Get All Users
- **GET** `/users`
- **Description**: Returns a list of all users in the system
- **Response**: 200 OK
  - Array of User objects
- **Error Responses**:
  - 500 Internal Server Error

##### Get User
- **GET** `/users/{id}`
- **Description**: Returns a specific user by ID
- **Response**: 200 OK
  - User object
- **Error Responses**:
  - 404 Not Found
  - 500 Internal Server Error

##### Create User
- **POST** `/users`
- **Description**: Creates a new user
- **Request Body**: User object
- **Response**: 201 Created
  - Created User object
- **Error Responses**:
  - 409 Conflict (User already exists)
  - 500 Internal Server Error

##### Update User
- **PUT** `/users/{id}`
- **Description**: Updates an existing user
- **Request Body**: User object
- **Response**: 200 OK
  - Updated User object
- **Error Responses**:
  - 404 Not Found
  - 500 Internal Server Error

##### Delete User
- **DELETE** `/users/{id}`
- **Description**: Deletes a user
- **Response**: 204 No Content
- **Error Responses**:
  - 404 Not Found
  - 500 Internal Server Error

##### Purge User History
- **DELETE** `/users/{id}/history`
- **Description**: Deletes all chat sessions and messages for a user
- **Response**: 204 No Content
- **Error Responses**:
  - 404 Not Found
  - 500 Internal Server Error

### 2. Chat

Handles chat interactions with the AI system.

#### Endpoints

##### Send Message
- **POST** `/chat`
- **Description**: Sends a user prompt and gets a response from the AI agents
- **Request Body**:
  ```json
  {
    "userId": "string",
    "sessionId": "string",
    "tenantId": "string",
    "chatName": "string",
    "prompt": "string"
  }
  ```
- **Response**: 201 Created
  - Chat response object with assistant and specialist responses
- **Error Responses**:
  - 400 Bad Request
  - 500 Internal Server Error

### 3. Sessions

Manages chat sessions and their messages.

#### Endpoints

##### Get All Sessions
- **GET** `/sessions?userId={userId}`
- **Description**: Returns all sessions, optionally filtered by userId
- **Query Parameters**:
  - userId (optional): Filter sessions by user
- **Response**: 200 OK
  - Array of Session objects
- **Error Responses**:
  - 500 Internal Server Error

##### Get Session
- **GET** `/sessions/{sessionId}`
- **Description**: Returns details of a specific session
- **Response**: 200 OK
  - Session object
- **Error Responses**:
  - 404 Not Found
  - 500 Internal Server Error

##### Get Session Messages
- **GET** `/sessions/{sessionId}/messages`
- **Description**: Returns all messages in a session
- **Response**: 200 OK
  - Array of Message objects
- **Error Responses**:
  - 404 Not Found
  - 500 Internal Server Error

##### Create Session
- **POST** `/sessions`
- **Description**: Creates a new chat session
- **Request Body**: Session object
- **Response**: 201 Created
  - Created Session object
- **Error Responses**:
  - 400 Bad Request
  - 500 Internal Server Error

##### Add Message
- **POST** `/sessions/{sessionId}/messages`
- **Description**: Adds a new message to a session
- **Request Body**: Message object
- **Response**: 201 Created
  - Created Message object
- **Error Responses**:
  - 404 Not Found
  - 500 Internal Server Error

##### Update Session
- **PUT** `/sessions/{sessionId}`
- **Description**: Updates session details
- **Request Body**: Session object
- **Response**: 200 OK
  - Updated Session object
- **Error Responses**:
  - 404 Not Found
  - 500 Internal Server Error

##### Delete Session
- **DELETE** `/sessions/{sessionId}`
- **Description**: Deletes a session and all its messages
- **Response**: 204 No Content
- **Error Responses**:
  - 404 Not Found
  - 500 Internal Server Error

### 4. Capabilities

Manages system capabilities and features.

#### Endpoints

##### Get All Capabilities
- **GET** `/capabilities`
- **Description**: Returns all system capabilities
- **Response**: 200 OK
  - Array of Capability objects
- **Error Responses**:
  - 500 Internal Server Error

##### Get Capability
- **GET** `/capabilities/{id}`
- **Description**: Returns a specific capability
- **Response**: 200 OK
  - Capability object
- **Error Responses**:
  - 404 Not Found
  - 500 Internal Server Error

##### Create Capability
- **POST** `/capabilities`
- **Description**: Creates a new capability
- **Request Body**: Capability object
- **Response**: 201 Created
  - Created Capability object
- **Error Responses**:
  - 409 Conflict
  - 500 Internal Server Error

##### Update Capability
- **PUT** `/capabilities/{id}`
- **Description**: Updates an existing capability
- **Request Body**: Capability object
- **Response**: 200 OK
  - Updated Capability object
- **Error Responses**:
  - 404 Not Found
  - 500 Internal Server Error

##### Delete Capability
- **DELETE** `/capabilities/{id}`
- **Description**: Deletes a capability
- **Response**: 204 No Content
- **Error Responses**:
  - 404 Not Found
  - 500 Internal Server Error

## Data Models

### User
```json
{
  "id": "string",
  "userInfo": {
    "email": "string"
  },
  "role": "string",
  "tier": "string",
  "preferences": {
    "theme": "string"
  }
}
```

### Message
```json
{
  "id": "string",
  "type": "string",
  "sender": "string",
  "sessionId": "string",
  "timeStamp": "string",
  "prompt": "string"
}
```

### Session
```json
{
  "id": "string",
  "sessionId": "string",
  "userId": "string",
  "name": "string",
  "type": "string",
  "timestamp": "string"
}
```

### Capability
```json
{
  "id": "string",
  "name": "string",
  "description": "string",
  "enabled": "boolean"
}
```

## Authentication

The API uses Azure Easy Auth for authentication. All endpoints require a valid authentication token.

## Error Handling

All endpoints follow a consistent error response format:

```json
{
  "message": "string",
  "details": "string"
}
```

Common HTTP status codes:
- 200: Success
- 201: Created
- 204: No Content
- 400: Bad Request
- 401: Unauthorized
- 404: Not Found
- 409: Conflict
- 500: Internal Server Error 