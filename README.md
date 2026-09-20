# 🎓 University Registration Portal (UniPortal)

A robust, full-stack university management and course registration system built with **ASP.NET Core**, featuring a clean separation between a Web frontend and a backend Web API, backed by **SQL Server**.

---

## 🚀 Key Features

* **System Administration Dashboard:** Manage university records, view live statistics (students, instructors, and courses), and handle administrative profiles.
* **Course & Student Management:** Complete CRUD operations for tracking student enrollment, department assignments, and course scheduling.
* **Secure Architecture:** Multi-project solution architecture enforcing clean separation of concerns (`Registration.Web`, `Registration.Api`, `Registration.DataAccess`, and `Registration.Domain`).
* **Optimized Performance:** Tuned database connection routing and network configurations to eliminate lookup bottlenecks and ensure instant page loads.

---

## 🛠️ Tech Stack

* **Backend & Frontend:** ASP.NET Core (C#), MVC, Razor Pages
* **Database:** Microsoft SQL Server, T-SQL (Stored Procedures, Triggers, Custom Views)
* **Version Control:** Git & GitHub

---

## 📂 Project Architecture

```text
University-Registration-Portal/
│
├── Registration.Web/         # Frontend web application (UI & Controllers)
├── Registration.Api/         # RESTful backend API handling business logic
├── Registration.DataAccess/  # Data layer & database operations
├── Registration.Domain/      # Core entities and data models
└── Database/                 # SQL migration scripts, schemas, and stored procedures
