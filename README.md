# CareNote – Smart Documentation for Healthcare

CareNote is a web application built with **ASP.NET Core MVC (.NET 8)** to support healthcare professionals with documentation, reflection, and knowledge management.

The system uses **Firebase Firestore** for authentication and data storage, as well as **GroqAIService** to provide AI-generated responses and reflections.

---

## Table of Contents

1. [Features](#features)
2. [Project Structure](#project-structure)
3. [System Requirements](#system-requirements)
4. [Installation and Configuration](#installation-and-configuration)
5. [Running the Application](#running-the-application)
6. [Authentication](#authentication)
7. [Testing](#testing)
8. [Technical Overview](#technical-overview)
9. [Images](#images)

---

## Features

| Module             | Description                                                                                               |
| ------------------ | --------------------------------------------------------------------------------------------------------- |
| **AI Chat**        | Chat functionality that provides AI support for healthcare documentation and procedures.                  |
| **Reflection**     | A reflection mode that generates questions and insights to support professional development.              |
| **Knowledge Base** | Search functionality for personal files and external sources such as 1177, WHO, SBU, and Socialstyrelsen. |
| **Notes**          | Create, edit, and manage personal notes.                                                                  |
| **Authentication** | Sign in with Google (restricted to `@care.se` addresses) or with email and password.                      |

---

## Project Structure

```text
CareNote/
│
├── Controllers/
│   ├── AIChatController.cs
│   ├── AuthController.cs
│   ├── HomeController.cs
│   ├── NotesController.cs
│
├── Models/
│   ├── Note.cs
│   ├── User.cs
│   ├── KnowledgeFile.cs
│   ├── NoteModel.cs
│   ├── ErrorViewModel.cs
│
├── Services/
│   ├── FirebaseAuthService.cs
│   ├── GroqAIService.cs
│   ├── KnowledgeService.cs
│   ├── NoteRepository.cs
│   ├── Interfaces/
│       ├── INoteRepository.cs
│       ├── IKnowledgeService.cs
│
├── Views/
│   ├── Auth/
│   ├── Home/
│   ├── AIChat/
│   ├── Notes/
│
├── wwwroot/
│   ├── css/
│   ├── js/
│
├── Program.cs
├── appsettings.Development.json
└── firebase-service-account.json (local file, not version controlled)

CareNoteTest/
├── CareNoteTest.csproj
├── UnitTest1.cs
└── FirebaseAuthServiceTests.cs
```

---

## System Requirements

Before running the application, make sure you have:

* .NET 8 SDK
* Visual Studio 2022, JetBrains Rider, or Visual Studio Code
* A Google Cloud project with **Firebase Firestore**
* A valid **Service Account file** (`firebase-service-account.json`)
* A valid **Groq API key**

---

## Installation and Configuration

### 1. Navigate to the project

```bash
cd CareNote
```

---

## Running the Application

### Build the project

```bash
dotnet build
```

### Run the application

```bash
dotnet run
```

---

## Authentication

Only users with email addresses ending in `@care.se` can register and sign in.

The application supports:

* Google Sign-In through Firebase
* Email and password authentication
* Cookie-based session management

---

## Testing

Tests are located in a separate project called **CareNoteTest**.

### Structure

```text
CareNoteTest/
├── UnitTest1.cs
├── FirebaseAuthServiceTests.cs
└── CareNoteTest.csproj
```

### Purpose

* `UnitTest1.cs` tests the `Note` model.
* `FirebaseAuthServiceTests.cs` tests logging and error handling in `FirebaseAuthService`.

### Run the Tests

```bash
cd CareNoteTest
dotnet test
```

Example test results:

```text
✓ Note_Should_Create_Correctly [PASSED]
✓ Constructor_MissingServiceAccountPath_LogsWarning [PASSED]
✓ Constructor_FileNotFound_LogsErrorAndWarning [PASSED]
✓ Constructor_ValidServiceAccountPath_LogsInitializationAttempt [PASSED]
```

---

## Technical Overview

| Component          | Technology                                 |
| ------------------ | ------------------------------------------ |
| **Backend**        | ASP.NET Core MVC (.NET 8)                  |
| **Database**       | Firebase Firestore                         |
| **AI Service**     | GroqAIService (e.g. LLaMA model)           |
| **Authentication** | CookieAuthentication + FirebaseAuthService |
| **Testing**        | xUnit + Moq                                |
| **Build System**   | .NET CLI                                   |

---

## Images

<img width="904" height="1354" alt="CareNote application" src="https://github.com/user-attachments/assets/c4ff7373-a763-4f83-99d8-9f9ff8532687" />

<img width="2862" height="1542" alt="CareNote application" src="https://github.com/user-attachments/assets/b63afeec-e36e-4b46-9c75-01ba42f8aed7" />

<img width="2636" height="1476" alt="CareNote application" src="https://github.com/user-attachments/assets/2129e856-7375-4718-a8f1-07cc697f1b83" />

<img width="2570" height="1526" alt="CareNote application" src="https://github.com/user-attachments/assets/74cb558a-7e17-4eae-81c6-070e9b5b8bc0" />

<img width="2576" height="1552" alt="CareNote application" src="https://github.com/user-attachments/assets/db953b62-7810-4ed7-8f30-321c112d4eeb" />

<img width="2476" height="1538" alt="CareNote application" src="https://github.com/user-attachments/assets/bb74f0bd-021b-4b13-827f-f1a30f06a5b0" />

<img width="2550" height="1546" alt="CareNote application" src="https://github.com/user-attachments/assets/6cd2fece-ad58-47e3-8a6f-28a5ffe89006" />

<img width="2534" height="1554" alt="CareNote application" src="https://github.com/user-attachments/assets/15fe6b9c-9f05-40e8-8e5d-f13766bc22b7" />



