
# CareNote -- Smart Documentation for Healthcare

CareNote är en webbapplikation byggd med **ASP.NET Core MVC (.NET 8)**
för att stödja vårdpersonal i dokumentation, reflektion och
kunskapshantering.\
Systemet använder **Firebase Firestore** för autentisering och
datalagring samt **GroqAIService** för AI-genererade svar och
reflektioner.

------------------------------------------------------------------------

## Innehållsförteckning

1.  [Funktioner](#funktioner)
2.  [Projektstruktur](#projektstruktur)
3.  [Systemkrav](#systemkrav)
4.  [Installation och konfiguration](#installation-och-konfiguration)
5.  [Köra applikationen](#köra-applikationen)
6.  [Testning](#testning)
7.  [Teknisk översikt](#teknisk-översikt)
8.  [Licens](#licens)

------------------------------------------------------------------------

## Funktioner

  -----------------------------------------------------------------------
Modul                       Beskrivning
  --------------------------- -------------------------------------------
AI Chat                     Chatfunktion som erbjuder AI-stöd i
vårddokumentation och rutiner.

Reflektion                  Ett reflektionsläge som genererar frågor
och insikter för professionell utveckling.

Kunskapsbas                 Sökning i egna filer och externa källor som
1177, WHO, SBU och Socialstyrelsen.

Anteckningar                Skapa, redigera och hantera egna
anteckningar.

Autentisering               Inloggning via Google (endast
@care.se-adresser) eller e-post och
lösenord.
  -----------------------------------------------------------------------

------------------------------------------------------------------------

## Projektstruktur

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
    └── firebase-service-account.json (lokal fil, ej versionshanterad)

    CareNoteTest/
    ├── CareNoteTest.csproj
    ├── UnitTest1.cs
    └── FirebaseAuthServiceTests.cs

------------------------------------------------------------------------

## Systemkrav

-   [.NET 8 SDK](https://dotnet.microsoft.com/download)
-   Visual Studio 2022, JetBrains Rider eller Visual Studio Code
-   Ett Google Cloud-projekt med **Firebase Firestore**
-   En giltig **Service Account-fil** (`firebase-service-account.json`)
-   En giltig **Groq API-nyckel**

------------------------------------------------------------------------

## Installation och konfiguration

### 1. Navigera till projektet

``` bash
cd CareNote
```



## Köra applikationen

### Bygg projektet

``` bash
dotnet build
```

### Starta applikationen

``` bash
dotnet run
```



## Autentisering

Endast användare med e-postadresser som slutar på `@care.se` kan
registrera sig och logga in.

Stöd för: - Google-inloggning via Firebase - E-post och lösenord -
Sessioner hanteras via cookies


## Testning

Testerna finns i ett separat projekt: **CareNoteTest**

### Struktur

    CareNoteTest/
    ├── UnitTest1.cs
    ├── FirebaseAuthServiceTests.cs
    └── CareNoteTest.csproj

### Syfte

-   `UnitTest1.cs` testar modellen `Note`
-   `FirebaseAuthServiceTests.cs` testar loggning och felhantering i
    `FirebaseAuthService`

### Kör tester

``` bash
cd CareNoteTest
dotnet test
```

Exempel på testresultat:

    ✓ Note_Should_Create_Correctly [PASSED]
    ✓ Constructor_MissingServiceAccountPath_LogsWarning [PASSED]
    ✓ Constructor_FileNotFound_LogsErrorAndWarning [PASSED]
    ✓ Constructor_ValidServiceAccountPath_LogsInitializationAttempt [PASSED]



## Teknisk översikt

Komponent       Beskrivning

Backend         ASP.NET Core MVC (.NET 8)
Databas         Firebase Firestore
AI-tjänst       GroqAIService (t.ex. LLaMA-modell)
Autentisering   CookieAuthentication + FirebaseAuthService
Tester          xUnit + Moq
Byggsystem      .NET CLI



## Licens

Projektet är en del av **CareNote Project © 2025** Alla rättigheter
reserverade. Utvecklat för användning inom vård och omsorg som stöd för
reflektion och dokumentation.
