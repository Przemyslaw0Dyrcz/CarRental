# 🚗 CarRental Platform

## 📌 Overview

CarRental is a secure, full-stack web application for managing vehicle rentals.  
The system supports multiple user roles (**Customer, Dealer, Admin**) and provides end-to-end functionality including car management, reservations, and role-based access control.

The application was designed with a strong focus on **security, scalability, and cloud deployment**.

---

## ⚙️ Tech Stack

### 🖥 Backend
- ASP.NET Core MVC (.NET)
- Entity Framework Core
- Azure SQL Database
- ASP.NET Identity

### 🎨 Frontend
- Razor Views (MVC)
- Bootstrap

### ☁️ Cloud & DevOps
- Microsoft Azure (App Service, SQL Database, Storage)
- GitHub Actions (CI/CD)
- Azure Application Insights (monitoring)

---

## 🔐 Security Features

- Role-Based Access Control (RBAC)
  - **Customer** – browse and reserve cars  
  - **Dealer** – manage vehicles and reservations  
  - **Admin** – full system access  

- ASP.NET Identity authentication  
- Server-side validation  
- CSRF protection (anti-forgery tokens)  
- Protection against SQL Injection (EF Core)  
- Authorization enforcement on endpoints  

---

## 🚀 Features

### 👤 User Management
- Registration & login
- Role assignment

### 🚘 Car Management
- Add / edit / delete vehicles
- Image upload support
- Availability tracking

### 📅 Reservation System
- Book cars for selected time periods
- Dealer can create reservations for customers
- Calendar view

### 📊 Admin Panel
- Full system control
- Manage users, cars, and reservations

---

## ☁️ Azure Deployment

The application is deployed to Microsoft Azure:

- **App Service** – hosting  
- **Azure SQL Database** – production database  
- **Azure Storage** – file handling  
- **Application Insights** – monitoring  

### 🔄 CI/CD

- GitHub Actions pipeline  
- Automatic build & deployment  
- Environment configuration via App Settings  

---

## 🧠 Architecture

The project follows a layered architecture:

- Controllers → request handling  
- Services → business logic  
- EF Core → data access  
- Models → domain entities  

This improves maintainability and scalability.

---

## 📊 Monitoring (Application Insights)

Application performance is monitored using Azure Application Insights:

- Request tracking  
- Exception logging  
- Performance metrics  
- Availability monitoring  

---

## 📷 Screenshots

### 🧑 Customer Panel
<img width="1840" height="924" alt="{0BB4923A-25C9-444E-A1D2-C05B8F21DEC0}" src="https://github.com/user-attachments/assets/41cc4032-32c2-4317-bd58-56f3134e77b6" />
<img width="1838" height="921" alt="{516F519F-2148-4DF6-B443-512A5021782E}" src="https://github.com/user-attachments/assets/558485b6-d2b5-495b-ada8-95c11b42dbdd" />
<img width="1858" height="923" alt="{D289C873-EFAF-4030-9344-971C20868D70}" src="https://github.com/user-attachments/assets/1b53ca45-79e3-4c43-bbe3-fec3b3dceb5c" />
<img width="1856" height="921" alt="{C7446109-44D6-4250-B0D9-EF62AE06526C}" src="https://github.com/user-attachments/assets/7671ed48-a9e7-494c-8c65-2348001ecdce" />
<img width="1860" height="920" alt="{1708E147-BAF4-4A4C-950F-00BC8DA972BB}" src="https://github.com/user-attachments/assets/020e240b-9165-4049-ade8-2757e6479a53" />

---

### 🏢 Dealer Panel
<img width="1840" height="922" alt="{14049DFC-56C6-40B1-8E1C-B910E829F322}" src="https://github.com/user-attachments/assets/8d0692e8-839c-4b80-ad4d-dbbafa116911" />
<img width="1860" height="923" alt="{3255B88A-B174-4391-AD1B-22E63045C043}" src="https://github.com/user-attachments/assets/cab603b8-eb50-4426-878a-59ec7a45f929" />
<img width="1861" height="922" alt="{264339F0-E0D1-41C0-B1AF-5C12683A0DE8}" src="https://github.com/user-attachments/assets/c58ff9ba-20f6-4052-8814-c2ce003c1e8f" />
<img width="1858" height="923" alt="{36BD360B-7AEE-440B-A3DA-D054AE724BA5}" src="https://github.com/user-attachments/assets/213b5260-1da9-473a-99bb-c914a949175f" />
<img width="1860" height="921" alt="{BF2895F9-037B-4F42-BCE3-91924C9AC47F}" src="https://github.com/user-attachments/assets/beb1022f-18b0-4628-8925-883387cbe78a" />


---

### 🛠 Admin Panel
<img width="1861" height="923" alt="{5124C3B5-24A0-4F0D-B40D-866B6B21BACC}" src="https://github.com/user-attachments/assets/376d6595-e9a6-4357-8ee1-66c57d64df96" />
<img width="1858" height="926" alt="{8F90E136-05B0-4557-9F8C-746160E60EB7}" src="https://github.com/user-attachments/assets/c003c90c-8e83-4e09-bcfe-0908219349d7" />
<img width="1860" height="922" alt="{7BD7C035-9A3D-4579-8924-784AFDB7F95C}" src="https://github.com/user-attachments/assets/9b543ac7-c2d3-450e-929b-151c46cc7039" />
<img width="1861" height="924" alt="{EB498B3A-FDFB-4BDF-BDD1-EDF80D07F56F}" src="https://github.com/user-attachments/assets/779758d6-8257-4933-a934-ad77fad66811" />
<img width="1859" height="923" alt="{B3BB69CF-F5DB-471A-BE16-0BD743343814}" src="https://github.com/user-attachments/assets/6802b8d3-1379-45b6-ab23-709eb7252494" />
<img width="1839" height="922" alt="{A7539707-52C1-4D6E-A394-EB06C4286AD4}" src="https://github.com/user-attachments/assets/110b7c99-4b31-40df-ac0c-9b05076be73f" />
<img width="1469" height="715" alt="image" src="https://github.com/user-attachments/assets/7135b2a6-95df-4b76-95f9-4f81b2a091df" />
<img width="1837" height="922" alt="{0EA93CBE-292E-46F8-96E3-D809E00411BA}" src="https://github.com/user-attachments/assets/22405bce-6ac2-4b4b-8027-c5739bfefdde" />
<img width="1843" height="924" alt="{44C53D4C-29CD-44F6-9219-048D35E64467}" src="https://github.com/user-attachments/assets/4ad1e009-cd84-42ac-99e2-135dc7e620f1" />

### 🌐 AZURE
<img width="1483" height="720" alt="image" src="https://github.com/user-attachments/assets/6af2b44f-f1b1-4b52-bd1e-9644d798d7f4" />

<img width="1857" height="924" alt="{5CFE6A7B-152B-47A6-9C4D-9029E27044CC}" src="https://github.com/user-attachments/assets/45d8d13a-b5fc-4616-88e0-6335ab70f624" />

<img width="1485" height="721" alt="image" src="https://github.com/user-attachments/assets/877c9a22-f9cb-42fd-b876-1fd48b3c1ddf" />
<img width="1842" height="919" alt="{506CD99A-5152-4853-8AEE-5A1EF166DCF9}" src="https://github.com/user-attachments/assets/fc192d13-35e6-447e-9a62-b883d91903f7" />
<img width="1858" height="924" alt="{2A67D939-EF17-4E63-ACC8-08C89A13BF05}" src="https://github.com/user-attachments/assets/cb12323a-74d3-4e5a-9092-28f74d2c54bd" />


## 🧪 Future Improvements

- JWT authentication (API support)  
- REST API layer  
- Unit & integration tests  
- Advanced logging & alerting  
- Payment integration  

---
##📄 License

Educational / portfolio project.

##💼 Portfolio Value

This project demonstrates:

Full-stack .NET development
Cloud deployment (Azure)
CI/CD pipeline
Secure application design


## 📎 Run Locally

```bash
git clone https://github.com/your-username/CarRental.git
cd CarRental
```
Update connection string in appsettings.json
```bash
dotnet ef database update
dotnet run
```

