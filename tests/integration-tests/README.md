# Medical Center API Integration Tests

Integration tests for the Medical Center RESTful API using Jest framework.

## Overview

This test suite provides comprehensive integration testing for multiple API controllers:
- **Patients API V1** - Basic patient information endpoints
- **Patients API V2** - Extended patient information with statistics
- **Doctors API** - Doctor management endpoints
- **Appointments API** - Appointment management endpoints

## Features

- **Multi-Database Support**: Tests can run against 4 different database providers:
  - SQL Server
  - PostgreSQL
  - SQLite
  - MySQL

- **API Versioning**: Tests validate both V1 and V2 API compatibility

- **Comprehensive Coverage**:
  - CRUD operations testing
  - Data validation
  - Error handling (404, etc.)
  - Relations and data integrity
  - API versioning compatibility

## Prerequisites

- Node.js (v18 or higher)
- Running Medical Center API instance
- Access to configured databases

## Installation

```bash
cd tests/integration-tests
npm install
```

## Configuration

Create a `.env` file based on `.env.example`:

```bash
cp .env.example .env
```

Edit `.env` with your settings:

```env
API_BASE_URL=https://localhost:7041
API_TIMEOUT=10000
DB_PROVIDER=SqlServer
```

## Running Tests

### Run all tests with default database (SqlServer)

```bash
npm test
```

### Run tests with specific database provider

```bash
# SQL Server
npm run test:sqlserver

# PostgreSQL
npm run test:postgres

# SQLite
npm run test:sqlite

# MySQL
npm run test:mysql
```

### Run tests for all databases sequentially

```bash
npm run test:all-dbs
```

### Run tests in watch mode

```bash
npm run test:watch
```

### Generate coverage report

```bash
npm run test:coverage
```

## Test Structure

```
src/
├── config.ts                 # Configuration and endpoints
├── setup.ts                  # Global test setup
├── types/
│   └── index.ts             # TypeScript type definitions
├── helpers/
│   └── apiClient.ts         # HTTP client wrapper
└── tests/
    ├── patients.v1.test.ts  # Patients API V1 tests
    ├── patients.v2.test.ts  # Patients API V2 tests
    ├── doctors.test.ts      # Doctors API tests
    └── appointments.test.ts # Appointments API tests
```

## Test Scenarios

### Patients V1 API
- Get all patients
- Get patient by ID
- Validate V1 response structure
- Handle non-existent patients (404)

### Patients V2 API
- Get all patients with statistics
- Get patient by ID with statistics
- Validate V2 response structure (includes statistics)
- Validate backward compatibility with V1
- Handle non-existent patients (404)

### Doctors API
- Get all doctors
- Get doctor by ID
- Validate doctor information structure
- Validate experience years range
- Validate rating range
- Validate user relations
- Handle non-existent doctors (404)

### Appointments API
- Get all appointments
- Get appointment by ID
- Validate appointment structure
- Validate appointment status enum
- Validate date format
- Validate patient-doctor relations
- Validate data integrity
- Handle non-existent appointments (404)

## Database Provider Configuration

The API must be configured to use the corresponding database. Set the `DatabaseProvider` in `appsettings.json`:

```json
{
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "SqlServerConnection": "Server=localhost;Database=MedicalCenter;...",
    "PostgresConnection": "Host=localhost;Database=medicalcenter;...",
    "SqliteConnection": "Data Source=medicalcenter.db",
    "MySqlConnection": "Server=localhost;Database=medicalcenter;..."
  }
}
```

## Expected Test Results

All tests should pass if:
1. The API is running and accessible
2. The database is properly configured and contains test data
3. All endpoints are correctly implemented
4. API versioning is properly configured

## Troubleshooting

### SSL Certificate Errors
The test client is configured to accept self-signed certificates for local development. If you encounter SSL errors, ensure your API is running with HTTPS enabled.

### Timeout Errors
Increase the `API_TIMEOUT` value in `.env` if tests are timing out.

### Connection Refused
Ensure the API is running at the configured `API_BASE_URL`.

### 404 Errors on Valid Endpoints
Check API routing configuration and ensure API versioning is properly set up.

## Contributing

When adding new tests:
1. Create a new test file in `src/tests/`
2. Add corresponding types in `src/types/index.ts`
3. Update `src/config.ts` with new endpoints
4. Document the new tests in this README

## Notes

- Tests are run sequentially (`--runInBand`) to avoid database conflicts
- Each test suite logs the database provider being tested
- Tests assume some data already exists in the database
- Empty results are handled gracefully with conditional assertions
