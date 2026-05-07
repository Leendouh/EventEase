# EventEase - Venue & Event Booking Management System

<div align="center">

![EventEase Logo](https://via.placeholder.com/200x80/0d6efd/ffffff?text=EventEase)

[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-blue.svg)](https://docs.microsoft.com/aspnet/core/)
[![Azure](https://img.shields.io/badge/Azure-Integrate-blue.svg)](https://azure.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

*A comprehensive venue and event booking management system built with ASP.NET Core MVC*

</div>

## 📋 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Technology Stack](#-technology-stack)
- [Prerequisites](#-prerequisites)
- [Installation](#-installation)
- [Configuration](#-configuration)
- [Usage](#-usage)
- [Project Structure](#-project-structure)
- [API Documentation](#-api-documentation)
- [Contributing](#-contributing)
- [License](#-license)

## 🎯 Overview

EventEase is a modern web application for managing venue bookings and events. It provides a complete solution for businesses to manage their venues, events, and bookings with advanced features like conflict prevention, image management, and comprehensive search capabilities.

### Key Benefits

- 🏢 **Venue Management**: Complete CRUD operations with image support
- 📅 **Event Management**: Schedule and organize events with venue associations
- 🎫 **Booking System**: Advanced booking with conflict prevention and validation
- 🔍 **Search & Filter**: Powerful search capabilities across all entities
- 👥 **User Management**: Role-based access control with authentication
- ☁️ **Cloud Integration**: Azure Storage for image management
- 📱 **Responsive Design**: Mobile-friendly Bootstrap 5 interface

## ✨ Features

### 🏢 Venue Management
- Create, read, update, and delete venues
- Image upload and management via Azure Storage
- Capacity and availability tracking
- Advanced filtering and search
- Deletion protection when linked to bookings

### 📅 Event Management
- Complete event lifecycle management
- Venue association and confirmation
- Image upload for events
- Date-based filtering and search
- Deletion protection when linked to bookings

### 🎫 Booking System
- Comprehensive booking management
- Double-booking prevention
- Customer information tracking
- Status management (Pending, Confirmed, Cancelled, Completed)
- Enhanced booking views with event and venue details
- Advanced search by Booking ID, Event Name, Customer details

### 🔍 Search & Filter
- Real-time search across all entities
- Multi-criteria filtering
- Date range filtering
- Status-based filtering
- Pagination support for large datasets

### 👥 Authentication & Authorization
- ASP.NET Core Identity integration
- Role-based access control (SuperAdmin, BookingSpecialist)
- First-time password change requirement
- Profile management system
- Secure login with proper authorization

### 🎨 User Interface
- Modern Bootstrap 5 design
- Responsive mobile-friendly layout
- Interactive elements with transitions
- Status indicators and badges
- Enhanced header with admin designation
- Fixed navigation for better UX

## 🛠 Technology Stack

### Backend
- **.NET 8.0** - Latest .NET framework
- **ASP.NET Core MVC** - Web framework
- **Entity Framework Core** - ORM and database management
- **ASP.NET Core Identity** - Authentication and authorization

### Frontend
- **Bootstrap 5** - CSS framework
- **Razor Views** - Server-side templating
- **JavaScript** - Client-side interactions
- **HTML5 & CSS3** - Modern web standards

### Database
- **SQL Server** - Primary database (Local/Azure)
- **Azure SQL Database** - Cloud database option
- **Entity Framework Migrations** - Database versioning

### Cloud Services
- **Azure Blob Storage** - Image storage and management
- **Azure Web Apps** - Hosting platform
- **Azure SQL Database** - Managed database service

## 📋 Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server** (Local Express or full version)
- **Visual Studio 2022** or **VS Code** with C# extension
- **Git** for version control

### Optional (for Azure deployment)
- **Azure Account** - [Create free account](https://azure.microsoft.com/free/)
- **Azure CLI** - [Install Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)

## 🚀 Installation

### 1. Clone the Repository

```bash
git clone https://github.com/Leendouh/EventEase.git
cd EventEase
```

### 2. Configure Database

#### Option A: Local SQL Server
1. Ensure SQL Server Express is running
2. The application is configured to use `EventEaseDB` database
3. Run database migrations (see Configuration section)

#### Option B: Azure SQL Database
1. Create an Azure SQL Database
2. Update connection string in `appsettings.json`
3. Run database migrations

### 3. Install Dependencies

```bash
cd EventEase
dotnet restore
```

### 4. Run Database Migrations

```bash
dotnet ef database update
```

### 5. Run the Application

```bash
dotnet run
```

The application will be available at `http://localhost:5000`

## ⚙️ Configuration

### Database Configuration

Edit `appsettings.json` to configure your database connection:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=EventEaseDB;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### Azure Storage Configuration (Optional)

For image upload functionality, configure Azure Storage:

```json
{
  "AzureStorage": {
    "ConnectionString": "Your-Azure-Storage-Connection-String",
    "ContainerName": "venue-images"
  }
}
```

### Logging Configuration

Configure logging levels as needed:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## 📖 Usage

### Getting Started

1. **Run the application** - Navigate to `http://localhost:5000`
2. **Create Admin Account** - Register as a new user
3. **Login** - Use your credentials to access the dashboard
4. **Set Up Venues** - Add venues with images and details
5. **Create Events** - Schedule events and associate with venues
6. **Manage Bookings** - Create and manage bookings with conflict prevention

### Key Workflows

#### Venue Management
1. Navigate to **Venues** → **Create New**
2. Fill in venue details (name, location, capacity)
3. Upload venue image (optional)
4. Set availability status
5. Save venue

#### Event Management
1. Navigate to **Events** → **Create New**
2. Enter event details (name, date, description)
3. Select venue (optional)
4. Upload event image (optional)
5. Save event

#### Booking Management
1. Navigate to **Bookings** → **Create New**
2. Select event and venue
3. Enter customer details
4. Set booking dates
5. System automatically prevents double-booking
6. Save booking

## 📁 Project Structure

```
EventEase/
├── Controllers/          # MVC Controllers
│   ├── AuthController.cs
│   ├── BookingController.cs
│   ├── EventController.cs
│   ├── HomeController.cs
│   └── VenueController.cs
├── Models/              # Data Models
│   ├── Booking.cs
│   ├── Event.cs
│   ├── Venue.cs
│   └── ViewModels/      # View Models
├── Views/               # Razor Views
│   ├── Auth/
│   ├── Booking/
│   ├── Event/
│   ├── Home/
│   ├── Shared/
│   └── Venue/
├── Services/            # Business Logic Services
│   └── AzureStorageService.cs
├── Data/               # Database Context
│   └── ApplicationDbContext.cs
├── Migrations/         # Database Migrations
├── wwwroot/           # Static Files
│   ├── css/
│   ├── js/
│   └── images/
└── Program.cs         # Application Entry Point
```

## 🔧 API Endpoints

### Authentication
- `POST /Auth/Login` - User login
- `POST /Auth/Register` - User registration
- `POST /Auth/Logout` - User logout

### Venues
- `GET /Venues` - List all venues
- `GET /Venues/{id}` - Get venue details
- `POST /Venues` - Create new venue
- `PUT /Venues/{id}` - Update venue
- `DELETE /Venues/{id}` - Delete venue

### Events
- `GET /Events` - List all events
- `GET /Events/{id}` - Get event details
- `POST /Events` - Create new event
- `PUT /Events/{id}` - Update event
- `DELETE /Events/{id}` - Delete event

### Bookings
- `GET /Bookings` - List all bookings
- `GET /Bookings/{id}` - Get booking details
- `POST /Bookings` - Create new booking
- `PUT /Bookings/{id}` - Update booking
- `DELETE /Bookings/{id}` - Delete booking
- `GET /Bookings/EnhancedView` - Enhanced booking view with filters

## 🤝 Contributing

We welcome contributions! Please follow these steps:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/AmazingFeature`)
3. **Commit** your changes (`git commit -m 'Add some AmazingFeature'`)
4. **Push** to the branch (`git push origin feature/AmazingFeature`)
5. **Open** a Pull Request

### Development Guidelines

- Follow C# coding conventions
- Write unit tests for new features
- Update documentation as needed
- Ensure code builds without errors
- Follow the existing project structure

## 📝 Documentation

For detailed setup and configuration instructions, see:

- [Development Setup Guide](README-DEVELOPMENT.md)
- [Database Setup Guide](README-Database-Setup.md)

## 🐛 Troubleshooting

### Common Issues

#### Database Connection Issues
- Ensure SQL Server is running
- Check connection string in `appsettings.json`
- Verify database permissions

#### Image Upload Not Working
- Configure Azure Storage connection string
- Check storage account permissions
- Verify container exists in Azure

#### Build Errors
- Run `dotnet clean` and `dotnet restore`
- Ensure .NET 8.0 SDK is installed
- Check for missing NuGet packages

### Getting Help

- Check the documentation files
- Review the error messages in the application logs
- Create an issue in the repository

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [ASP.NET Core](https://docs.microsoft.com/aspnet/core/) - Web framework
- [Bootstrap](https://getbootstrap.com/) - CSS framework
- [Entity Framework Core](https://docs.microsoft.com/ef/core/) - ORM
- [Azure Services](https://azure.microsoft.com/) - Cloud platform

---

<div align="center">

**Built with ❤️ using .NET 8.0 and ASP.NET Core**

[🔝 Back to Top](#eventease---venue--event-booking-management-system)

</div>
