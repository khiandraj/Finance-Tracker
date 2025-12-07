#  Finance Tracker API

A .NET-powered financial management system that supports user creation, balance tracking, recurring subscription handling, and automated transaction generation.  
This project uses **ASP.NET Web API**, **MongoDB**, **Swagger**, and **xUnit/Moq** for automated testing.

---

##  Features

###  User Management
- Create users  
- Validate login credentials  
- Store user data securely in MongoDB  

###  Balance Management
- Create new balance records automatically  
- Credit and debit user accounts  
- Track `LastUpdated` timestamps  
- Delete balance records  

###  Subscription Management
- Create recurring subscriptions  
- Frequency options: Monthly, Weekly, Daily, Custom  
- Automatically generate transactions when payment dates are due  
- Cancel subscriptions  

###  Automated Transactions
- Generated whenever subscriptions renew  
- Recorded using `ITransactionService`  
- Saves transaction amount, date, description, and currency  

###  API Documentation (Swagger)
Interactive API UI:  https://localhost:8080/swagger/index.html



---

##  Tech Stack

| **Layer** | **Technology** |
|----------|----------------|
| Backend | .NET 9 Web API |
| Database | MongoDB |
| Testing | xUnit + Moq |
| Documentation | Swagger / XML Docs |
| CI/CD | GitHub Actions |

---

## 📁 Project Structure


FinanceTracker/
│
├── FinanceTracker.Api
│ ├── Controllers
│ ├── Services
│ ├── Models
│ ├── Interfaces
│ ├── Helpers
│ └── FinanceTracker.Api.csproj
│
├── FinanceTracker.Tests
│ ├── BalanceServiceTests.cs
│ ├── SubscriptionServiceTests.cs
│ ├── TransactionServiceTests.cs
│ └── FinanceTracker.Tests.csproj
│
└── README.md



---

##  How to Run the API

### 1. Run the Web API
```bash
dotnet run --project FinanceTracker.Api
```
### 2. Open Swagger

```bash
https://localhost:8080/swagger
```

## Running Tests
```bash
dotnet test
```
## Example API Calls

### Create Subscription

POST /api/subscriptions
Content-Type: application/json

{
  "userId": "67331d448111e41dbd936ca2",
  "name": "Apple Music",
  "amount": 10.99,
  "currency": "USD",
  "frequency": 3,
  "nextPaymentUtc": "2025-12-10T12:00:00Z",
  "isActive": true,
  "notes": "Student plan"
}
