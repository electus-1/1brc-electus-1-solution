#!/bin/bash

# Clean up any previous build artifacts (optional)
dotnet clean

# Build the .NET application
dotnet build -c Release

# Run the benchmark using Hyperfine
hyperfine --warmup 0 \
          --runs 2 \
          --export-markdown benchmark_results.md \
          "dotnet bin/Release/net8.0/1BillionRowChallenge.dll measurements.txt"
