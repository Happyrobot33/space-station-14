#!/usr/bin/env pwsh

$name = "Fix Collumn Type"
dotnet ef migrations add --context SqliteServerDbContext -o Migrations/Sqlite $name
dotnet ef migrations add --context PostgresServerDbContext -o Migrations/Postgres $name
