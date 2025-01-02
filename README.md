# Generative AI Chat Application with .NET, Azure OpenAI, and Angular

## Overview

This repository contains a Generative AI-driven chat application built with the following technologies:

- **Backend**: .NET 9 API integrated with Azure OpenAI, Azure Form Recognizer, and Azure Blob Storage.
- **Frontend**: Angular (latest version), using Angular Material for UI components and `ngx-markdown` for rendering AI-generated responses.

The application allows users to chat, upload documents, and receive real-time responses from Azure OpenAI. It is designed to highlight Azure's AI capabilities and the power of integrating cloud-based document processing and file storage.

## Features

- **Real-Time AI Chat**: Communicate with Azure OpenAI via streaming responses.
- **File Upload and Analysis**: Upload documents (PDFs and Word files) and extract relevant content using Azure Form Recognizer.
- **Document Comparison**: Upload multiple documents for detailed comparison using Azure's AI models.

## Upcoming Feature
- [ ] Maintaining history context for a conversation

## Prerequisites

- .NET 9 SDK
- Node.js (with npm) - Angular CLI requires Node.js.
- Angular CLI (latest version)
- Azure Subscription (for using Azure OpenAI, Blob Storage, and Form Recognizer services)
- Visual Studio or VS Code (recommended for .NET and Angular development)

## Getting Started

### 1. Clone the Repository

```sh
git clone https://github.com/your-username/genai-chat.git
cd genai-chat
```

### 2. Backend Setup (API)

Navigate to the backend project directory and restore dependencies:

```sh
cd api
# Restore dependencies
dotnet restore
```

#### Add Configuration

Update value in `appsettings.Development.json` file in the `GenAI.Api` project root :

```json
  "ConnectionStrings": { 
    "AzureStorage": "<your-blob-storage-connection-string>" 
    },
  "DocumentIntelligence": {
    "Endpoint": "<your-document-intelligence-endpoint>",
    "ApiKey": "<your-document-intelligence-key>",
    "ModelId": "<your-document-intelligence-modelId>"
  },
  "OpenAI": {
    "Endpoint": "<your-openai-endpoint>",
    "ApiKey": "<your-openai-api-key>"
  }
```

Update these placeholders with the correct credentials from your Azure subscription.

#### Run the API

To start the API locally, run the following command:

```sh
dotnet run
```

The API will be available at `https://localhost:7173;http://localhost:5187` by default.

### 3. Frontend Setup (Angular)

Navigate to the Angular project directory and install dependencies:

```sh
cd ../frontend
# Install dependencies
npm install
```

#### Run the Angular Application

To start the Angular frontend, use the following command:

```sh
ng serve
```

The application will be available at `http://localhost:4200` by default.

### 4. Running the Full Application

Once both the backend and frontend are running, you can interact with the AI-driven chat interface in your browser at `http://localhost:4200`. Make sure the backend is running to provide data to the Angular client.

## Docker Support

**Note**: Dockerization for this project is currently in progress. We plan to provide a complete Docker setup soon, allowing you to run the entire application (API and frontend) in Docker containers for easier deployment.

## Coming Soon Updates

- **Chat History Support**: Store and retrieve chat history, allowing users to maintain conversations over time.

- **Multiple Chat Sessions**: Support multiple chat windows for different contexts, enabling users to work on various topics simultaneously.

- **Chat History with CosmosDB**: Maintain chat history using Azure CosmosDB for scalable and reliable storage.

- **Azure SSO Integration**: Implement Azure Single Sign-On (SSO) to provide a seamless and secure authentication experience.


- **Docker Setup**: Finalize and add Docker configuration for both backend and frontend.

## Contributing

We welcome contributions! Please feel free to submit pull requests, and report issues or enhancements you would like to see in the project.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.

## Contact

For support or questions, please contact Nitin Singh at nitin27may@gmail.com.

