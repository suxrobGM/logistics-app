@echo off
title Seed Databases
dotnet run --project ../src/Server/Logistics.DbMigrator
