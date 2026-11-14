Financial Assistant

A mobile application for personal finance tracking built for Android using .NET MAUI.


About

Financial Assistant helps users track their income and expenses, analyze spending patterns, forecast next month expenses, and manage savings goals. All data is stored locally on the device — no internet connection required.


Features

- Income and expense tracking with custom categories
- Analytics for week, month, year or any custom date range
- Dynamic chart with adaptive data grouping
- Category breakdown table with trend analysis (percentage change vs previous period)
- Next month expense forecast using Exponential Time Smoothing algorithm
- Savings goals with progress tracking
- Multi-currency support: BYN, USD, EUR, RUB
- Two interface languages: Russian and English
- Light and dark theme


Technology Stack

| Technology            | Purpose                        |
|-----------------------|--------------------------------|
| C# (.NET 10)          | Programming language           |
| .NET MAUI             | Cross-platform UI framework    |
| SQLite                | Local data storage             |
| MVVM                  | Architectural pattern          |
| CommunityToolkit.Mvvm | MVVM tooling                   |
| Microcharts           | Charts and diagrams            |
| Android SecureStorage | Secure session storage         |


Architecture

The project follows Clean Architecture principles and is divided into four layers:

Core — domain models and service interfaces
Application — business logic (FinancialService, AnalyticsService, MLForecastService, AuthService)
Infrastructure — data access (DatabaseService, GenericRepository, CurrencyService)
UI — user interface using MVVM pattern (Views, ViewModels, Converters)


Requirements

- Android 5.0 (API 21) or higher
- RAM: 2 GB minimum
- Storage: 100 MB free space
- Internet: not required


Getting Started

1. Clone the repository

   git clone https://github.com/gooooseman/khabibullin-diplom-bsuir.git

2. Open FinancialAssistant.sln in Visual Studio 2022

3. Make sure .NET MAUI workload and Android SDK are installed

4. Select a target device (emulator or physical Android device)

5. Press F5 to build and run


[ AUTHOR ]
--------------------------------------------------------------------------------
Vladimir Khabibullin
GitHub : @gooooseman (https://github.com/gooooseman)
================================================================================
