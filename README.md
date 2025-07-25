# .NET Core Microservices - The Complete Guide (.NET 8 MVC)
Learn Microservices architecture with .NET Core MVC(.NET 8), Entity Framework Core, .NET Identity with Azure Service Bus

- .NET Core Microservices - The Complete Guide (.NET 8 MVC)
- Implementing 7 microservices using .NET 8
- .NET API with Authentication and Authorization
- Role based authorization with .NET Identity
- Async and Sync communication between Microservices
- Azure Service Bus - Topics and Queues
- Gateways in Microservices
- Implementing Ocelot gateway
- Swagger Open API implementation
- N-Layer implementation with Repository Pattern
- ASPNET Core Web Application with Bootstrap 5
- Entity Framework Core with SQL Server Database

https://www.udemy.com/course/net-core-microservices-the-complete-guide-net-6-mvc

# How to use
* Use run-container.sh to run MS SQL Server and RabbitMQ
* Use stop-clean-container.sh to remove container with the MS SQL Server and RabbitMQ
* Use "Run all" to start all API services and web project
* Don't forget to setup Stripe SecretKey
* Use AzureServiceBusConnection instead of RabbitMQConnection to use AzureServiceBus
* To disable Gateway update appsettings.json and don't run Mango.GatewaySolution project