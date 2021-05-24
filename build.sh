#!/bin/bash
cd src/Org.Quickstart
dotnet restore

cd Org.Quickstart.API
dotnet build