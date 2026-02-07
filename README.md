# 📚 Bokhandel (Service-Oriented Inventory System)

![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![EF Core](https://img.shields.io/badge/Entity_Framework_Core-512BD4?style=for-the-badge)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)

**Bokhandel** is a robust console-based application for managing bookstore inventory and sales. It is built with **Entity Framework Core** and demonstrates a **Layered Architecture** by separating business logic into dedicated services.

## 🏗 Architecture

The solution uses a clean separation of concerns:

### 1. 🧠 Bokhandel.Services (Business Logic Layer)
Contains the core logic for processing data.
- **AuthorService:** Manages author details and associations.
- **BookService:** Handles book creation, updates, and deletion.
- **InventoryService:** Manages stock levels across different `Store` locations.
- **Service Pattern:** Decouples the user interface from the data access layer.

### 2. 🗄 Bokhandel (Data Access Layer)
Defines the database schema and context.
- **Entity Framework Core:** Uses `BokhandelContext` to manage SQL Server connections.
- **Rich Domain Model:** Includes entities for `Books`, `Authors`, `Publishers`, `Orders`, and `Inventory`.

## 🗄 Database Schema
The application models a complex relational database:
- **Many-to-Many:** Between `Books` and `Authors` (a book can have multiple authors).
- **Inventory System:** `Inventory` entity links `Stores` and `Books` with quantity tracking.
- **Sales:** `Order` and `OrderItem` entities to track transaction history.


## 🚀 How to Run

Since this project was built using a **Database First** approach, you need to initialize the database schema using the provided SQL script.

1.  **Clone the repository**
    ```bash
    git clone https://github.com/QQQQQQQQQian/Bokhandel.git
    ```

2.  **Set up the Database**
    - Locate the `script.sql` file in the root directory.
    - Open **SQL Server Management Studio (SSMS)**.
    - Create a new database named `Bokhandel`.
    - Run the script `script.sql` against this new database to create all tables (Books, Authors, Orders, etc.).

3.  **Configure Connection**
    - Open `Bokhandel/Models/BokhandelContext.cs`.
    - Locate the `OnConfiguring` method.
    - **Update the Connection String** to match your local SQL Server instance:
      ```csharp
      optionsBuilder.UseSqlServer("Server=localhost;Database=Bokhandel;Trusted_Connection=True;...");
      ```

4.  **Run the Application**
    - Open the solution in Visual Studio.
    - Press `F5` to start the Console Application.

---
*Note: This project uses Entity Framework Core (Database First) scaffolding.*
