## Flows to show

### a) OutputCache in PieController
1. Do not log.
2. Click in Shop drop down.
3. Click in All Pies.

### b) OutputCache in PieController
1. Log as an admin user.
2. Click in Pies drop down.
3. Click in View All.
4. In Action column, click in Details.

### c) OutputCache in CategoryRepository
1. Log as an admin user.
2. Click in Categories drop down.
3. Click View All.

### d) DistributedCache in OrderRepository
1. Log as an admin user.
2. Click in Orders to display the list.

---

## Running the App with .NET Aspire

Follow these steps to run the application using .NET Aspire:

1. **Install .NET Aspire Workload**
   - Make sure you have the latest .NET 9 SDK installed.
   - Install the Aspire workload if you haven't already: 
 ```
 dotnet workload install aspire
 ```
2. **Restore Dependencies**
   - Restore all NuGet packages:
```
     dotnet restore
```
3. **Run the Aspire AppHost Project**
   - The entry point for Aspire orchestration is the `PieShop.AppHost` project. Run it with: ```sh
 dotnet run --project PieShop.AppHost/PieShop.AppHost.csproj
    - This will start all required services, including the UI, database, and any background workers, as defined in the Aspire AppHost.

4. **Access the Application**
   - Once started, the Aspire dashboard will provide links to the running services (such as the UI and APIs). Follow the provided URLs to access the PieShop app.

5. **Stopping the App**
   - Press `Ctrl+C` in the terminal to stop all services.

---

For more details, see the [Microsoft Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/).

## Sample Users

| Role              | Email                 | Password       |
|-------------------|-----------------------|----------------|
| Customer          | customer@gmail.com    | Customer@12345 |
| Administrator     | admin_user@gmail.com  | Admin@12345    |

---

# Redis Commander

KEYS *

GET "aspnetcore-outputcache:209b0e81-29f7-5662-c713-0d64fbc6b88e"

---

# Connect to Microsoft SQL Server
1. **Server type**
   - Database Engine

2. **Server name**
   - Option 1: 127.0.0.1,56043
   - Option 2: localhost,56043
   
3. **Authentication**
   - SQL Server Authentication

4. **Login**
   - sa

5. **Password**
   - P@ssword1

---

## Aspire Docs
- [Microsoft Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)

## EF Database Factory
- [Using a DbContext Factory](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#use-a-dbcontext-factory)

## Deployment
- [Deploy to Azure Container Apps using azd](https://learn.microsoft.com/en-us/dotnet/aspire/deployment/azure/aca-deployment)
- [Deploy to Azure Container Apps with azd (In-depth)](https://learn.microsoft.com/en-us/dotnet/aspire/deployment/azure/aca-deployment-azd-in-depth?tabs=windows)

## Azure Developer CLI (azd)
- [Azure Developer CLI Commands](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/reference)
- [Feature Stages: Alpha, Beta, and Stable](https://github.com/Azure/azure-dev/blob/main/cli/azd/docs/feature-stages.md)

## .NET Aspire Repository
- [GitHub Repository](https://github.com/dotnet/aspire/pulls)

## Blog Post
- [Introducing the Azure Developer CLI (azd): A Faster Way to Build Apps for the Cloud](https://devblogs.microsoft.com/azure-sdk/introducing-the-azure-developer-cli-a-faster-way-to-build-apps-for-the-cloud/?ocid=AID754288&wt.mc_id=azfr-c9-scottha,CFID0730)

---