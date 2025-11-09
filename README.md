
# .NET Microservices

This project consists of multiple microservices built with **ASP.NET Core**, an **AdminUI** dashboard built with **ReactJS**, an **API Gateway**, a **MySQL Database**, and **NGINX** for load balancing. All services communicate internally via a **Docker Network**.

## System Architecture

```
[ AdminUI ] → [ NGINX Load Balancer ] → [ API Gateway ] → [ Auth Services / Product Services / Order Services ] → [ MySQL ]
```

- **AdminUI**: Frontend dashboard for administration
- **NGINX**: Load balancer for routing and distributing traffic across service instances
- **API Gateway**: The single entry point for client requests
- **AuthService**: AuthService, CustomerService, TokenService
- **ProductService**: ProductService, InventoryService, CategoryService, SupplierService
- **OrderService**: OrderService, PaymentService, PromotionService, AnalyticsService
- **MySQL**: Primary data storage for services

---

## Requirements

Before running the project, ensure the following are installed:

| Tool              | Recommended Version |
|------------------|--------------------|
| Docker           | ≥ 20.10            |
| Docker Compose   | ≥ 1.29             |

Check your versions using:

```
docker -v
docker compose version
```

---

## Environment Variables (ENV)

Each service requires its own `.env` file.

Create the following `.env` files:
```
AuthService/.env
ProductService/.env
OrderService/.env
ApiGateway/.env
```

### `.env` file structure (update database name per service):

```
CONNECTION_STRING=Server=mysql;Database=<DATABASE_NAME>;User=<DB_USER>;Password=<DB_PASSWORD>;
JWT_SECRET=<JWT_SECRET_KEY>
```

There are 3 different databases used in this project:
- AuthService → `auth_db`
- ProductService → `product_db`
- OrderService → `order_db`

---

## Run the Project

From the root directory of the project, run:

```
docker compose up --build -d
```

> `--build` ensures containers are rebuilt when source code changes.  
> `-d` runs the containers in detached mode.

---

## Check Running Services

List running containers:
```
docker ps
```

View logs from a specific service:
```
docker logs -f auth-service
```

---

## Access the System

| Component          | URL |
|-------------------|-----|
| Admin UI (Frontend) | http://localhost:3000 |
| API Gateway        | http://localhost:8080 |

---

