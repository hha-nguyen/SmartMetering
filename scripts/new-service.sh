#!/usr/bin/env bash
# Scaffold a new bounded-context service (4-layer Clean Architecture) + add to solution.
# Usage:  bash scripts/new-service.sh <Name>      e.g.  bash scripts/new-service.sh Billing
set -euo pipefail

NAME="${1:?Usage: bash scripts/new-service.sh <Name>  (e.g. Billing)}"
ROOT="src/$NAME"
P="SmartMetering.$NAME"

echo "==> Scaffolding service: $NAME"
dotnet new classlib -n "$P.Domain"         -o "$ROOT/$P.Domain"
dotnet new classlib -n "$P.Application"     -o "$ROOT/$P.Application"
dotnet new classlib -n "$P.Infrastructure"  -o "$ROOT/$P.Infrastructure"
dotnet new web      -n "$P.Api"             -o "$ROOT/$P.Api"

rm -f "$ROOT"/*/Class1.cs

echo "==> Wiring references (inward only)"
dotnet add "$ROOT/$P.Application"    reference "$ROOT/$P.Domain"
dotnet add "$ROOT/$P.Infrastructure" reference "$ROOT/$P.Application"
dotnet add "$ROOT/$P.Api"            reference "$ROOT/$P.Application" "$ROOT/$P.Infrastructure"

echo "==> Adding to solution"
dotnet sln add \
  "$ROOT/$P.Domain" \
  "$ROOT/$P.Application" \
  "$ROOT/$P.Infrastructure" \
  "$ROOT/$P.Api"

echo "==> Building"
dotnet build

echo "✅ Service '$NAME' scaffolded (Domain/Application/Infrastructure/Api) + added to solution."
