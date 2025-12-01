# Mobile Medical Center - Cross-Platform Mobile Application

A modern cross-platform mobile application for managing medical center operations built with **.NET MAUI** technology.

## Overview

The Mobile Medical Center application is a thin-client healthcare management system that allows users to manage doctors, patients, and appointments. It features offline-first architecture with local SQLite database and Integration Server authentication.

## Features

✅ **Core Features:**
- 🏥 Manage Doctors (add, view, delete)
- 👥 Manage Patients (add, view, delete)
- 📅 Book and manage Appointments
- 📊 Dashboard with statistics and charts
- 🔐 Identity Server authentication
- 🗄️ Local SQLite database for offline support
- 📱 Responsive UI for multiple platforms
- ⏳ Animated loading screens
- 🎨 Dark/Light theme support

## Technology Stack

- **Framework:** .NET MAUI 8.0
- **Database:** Entity Framework Core + SQLite
- **Architecture:** MVVM Pattern
- **Authentication:** Identity Server (JWT)
- **Language:** C# 12

## Project Structure

```
Mobile_Medical_Center/
├── Models/
│   ├── Doctor.cs
│   ├── Patient.cs
│   └── Appointment.cs
├── ViewModels/
│   ├── ViewModelBase.cs
│   ├── AsyncCommand.cs
│   ├── LoginViewModel.cs
│   ├── DoctorsViewModel.cs
│   ├── PatientsViewModel.cs
│   ├── AppointmentsViewModel.cs
│   └── DashboardViewModel.cs
├── Views/
│   └── Pages/
│       ├── LoginPage.xaml
│       ├── DoctorsPage.xaml
│       ├── PatientsPage.xaml
│       ├── AppointmentsPage.xaml
│       ├── DashboardPage.xaml
│       └── AboutPage.xaml
├── Services/
│   ├── AuthService.cs
│   └── DatabaseService.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Converters/
│   ├── InvertedBoolConverter.cs
│   ├── StringToVisibilityConverter.cs
│   ├── IntToWidthConverter.cs
│   └── EmptyCollectionToVisibilityConverter.cs
└── Resources/
    ├── Styles/
    ├── Fonts/
    ├── Images/
    └── Splash/
```

## Data Models

### Doctor
- Id (Primary Key)
- FullName
- Specialization
- ExperienceYears
- Bio
- Rating (decimal)
- CreatedAt

### Patient
- Id (Primary Key)
- FullName
- Email
- PhoneNumber
- DateOfBirth
- Gender
- Address
- EmergencyContact
- CreatedAt

### Appointment
- Id (Primary Key)
- DoctorId (Foreign Key)
- PatientId (Foreign Key)
- AppointmentDate
- Reason
- Status (Scheduled/Completed/Canceled)
- Notes
- CreatedAt

## Key Components

### MVVM Implementation
- **ViewModelBase:** Base class implementing INotifyPropertyChanged
- **AsyncCommand:** Custom implementation for async operations
- **Converters:** Value converters for XAML binding

### Services
- **AuthService:** Handles authentication with Identity Server
- **DatabaseService:** Manages SQLite database operations

### Pages
1. **LoginPage** - User authentication
2. **DashboardPage** - Overview with statistics and charts
3. **DoctorsPage** - Doctor management
4. **PatientsPage** - Patient management
5. **AppointmentsPage** - Appointment booking
6. **AboutPage** - Application information

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 / Visual Studio Code
- (For Android) Android SDK
- (For iOS) Xcode
- (For macOS) Xcode

### Installation

1. **Clone the repository:**
```bash
git clone <repository-url>
cd Mobile_Medical_Center
```

2. **Restore dependencies:**
```bash
dotnet restore
```

3. **Build the project:**
```bash
dotnet build
```

4. **Run on Windows:**
```bash
dotnet run -f net8.0-windows10.0.19041.0
```

### Configuration for Multi-Platform

To enable multi-platform builds, edit `Mobile_Medical_Center.csproj`:

```xml
<!-- Uncomment for multi-platform builds -->
<TargetFrameworks>net8.0-android;net8.0-ios;net8.0-maccatalyst</TargetFrameworks>
<TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('windows'))">$(TargetFrameworks);net8.0-windows10.0.19041.0</TargetFrameworks>
```

Then install required workloads:
```bash
dotnet workload install maui
dotnet workload install android ios maccatalyst
```

## Features in Detail

### Authentication Flow
1. User enters email and password on LoginPage
2. AuthService sends credentials to Identity Server
3. On success, JWT token is stored securely
4. User is navigated to DashboardPage

### Data Management
- All data is stored locally in SQLite database
- Database is automatically created on first app launch
- Seed data includes 3 doctors, 3 patients, and 4 sample appointments

### Dashboard Charts
- Bar chart showing appointments count per doctor
- Summary statistics (Total doctors, patients, appointments, completed)

### Offline Support
- App works completely offline with local database
- Synchronization with backend available when internet is available
- No forced dependency on backend connectivity

## Database Initialization

The database is automatically initialized on app startup:
- Creates SQLite database file in app data directory
- Creates all necessary tables
- Seeds initial data (doctors, patients, appointments)

### Database Location
- **Windows:** `C:\Users\[UserName]\AppData\Local\[AppName]\medical_center.db`
- **Android:** App's internal storage
- **iOS:** App's documents directory

## Authentication Details

### Credentials (Development)
- **Email:** admin@med.local
- **Password:** Admin#2025!

(Note: Requires running backend Medical_center project)

### Backend Integration
The app expects the Identity Server to be running at:
```
https://localhost:7041/api/auth
```

To skip authentication in development mode, modify `App.xaml.cs`.

## UI/UX Features

### Animated Loading Screen
- ActivityIndicator displays during async operations
- IsLoading state managed via ViewModels
- Smooth transitions between states

### Responsive Design
- Grid-based layouts
- Proper spacing and padding
- Touch-friendly controls (min 44x44 points)
- Dark/Light theme support

### Navigation
- Tab-based navigation interface
- Flyout menu for additional options
- Shell-based routing

## Testing

### Unit Test Compatibility
The project uses MVVM pattern making it testable:
- ViewModels can be tested independently
- Services are interface-based and mockable
- Async operations use AsyncCommand

### Manual Testing Checklist
- [ ] Login with valid credentials
- [ ] Add a new doctor
- [ ] Add a new patient
- [ ] Book an appointment
- [ ] Change appointment status
- [ ] View dashboard statistics
- [ ] Test on offline mode
- [ ] Test dark/light theme toggle

## Deployment

### Windows Deployment
```bash
dotnet publish -c Release -f net8.0-windows10.0.19041.0
```

### Android Deployment
```bash
dotnet publish -c Release -f net8.0-android
```

### iOS/macOS Deployment
```bash
dotnet publish -c Release -f net8.0-ios
dotnet publish -c Release -f net8.0-maccatalyst
```

## Known Limitations

1. Authentication requires backend Medical_center project running
2. Multi-platform builds require proper SDK setup
3. Charts are simple bar charts (not advanced visualizations)
4. No real-time synchronization with backend

## Future Enhancements

- [ ] Real-time data synchronization
- [ ] Advanced chart visualizations
- [ ] Patient medical history management
- [ ] Notification system
- [ ] Prescription management
- [ ] Test results integration
- [ ] Video consultation support
- [ ] Multilingual support

## Troubleshooting

### Build Errors
- Ensure .NET 8.0 SDK is installed
- Run `dotnet restore` before building
- Check that all required NuGet packages are available

### Runtime Errors
- Verify database file is not locked
- Check that medical_center backend is running (if authentication required)
- Review application logs for detailed error messages

### Performance Issues
- Clear app data and cache
- Rebuild SQLite indexes
- Profile app using platform-specific profilers

## Contributing

Contributions are welcome! Please follow these guidelines:
1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is part of educational coursework. All rights reserved.

## Contact & Support

For issues, questions, or suggestions, please open an issue on GitHub.

---

**Built with ❤️ using .NET MAUI**

Last Updated: December 2024
Version: 1.0.0
