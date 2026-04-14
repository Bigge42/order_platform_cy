# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Order management platform (OCP) for a manufacturing company. Monorepo with a Vue 3 frontend, a .NET 8 backend, and a data visualization dashboard.

## Repository Structure

| Directory | Tech | Purpose |
|-----------|------|---------|
| `web.vite/` | Vue 3 + Vite + Element Plus | Main frontend app |
| `api/` | .NET 8 / ASP.NET Core | Backend API (solution: `Haodee.CY.OCP.sln`) |
| `dataview/` | Vue 3 (Go-View) + ECharts | Data visualization dashboards |
| `app/` | Electron wrapper | Desktop app shell |
| `docs/` | Markdown | Feature documentation (alerts, ESB, materials) |

## Common Commands

### Frontend (`web.vite/`)

```bash
cd web.vite
npm run dev          # Dev server on port 9010
npm run build        # Production build
npm run build:stage  # Staging build
npm run test:unit    # Run vitest
npm run lint         # ESLint with auto-fix
npm run format       # Prettier formatting
npm run type-check   # vue-tsc type checking
```

### Dataview (`dataview/`)

```bash
cd dataview
npm run dev          # Dev server
make dist            # Production build (or npm run build)
npm run lint:fix     # ESLint with auto-fix
```

### Backend (`api/`)

Built with Visual Studio or `dotnet` CLI. Solution file: `api/Haodee.CY.OCP.sln`.

```bash
cd api
dotnet build Haodee.CY.OCP.sln
dotnet run --project HDPro.WebApi    # API server on port 9200
```

## Architecture

### Frontend (web.vite)

- **Framework layer:** Built on "vol" — a pre-built enterprise framework that handles routing, theming, auth, and permissions. Do not re-implement what vol already provides.
- **State:** Vuex store (`src/store/index.js`) — user info persisted to localStorage, permissions loaded at login.
- **Routing:** Vue Router with hash mode. Menu-driven dynamic routes.
- **API client:** Axios (`src/api/http.js`) with JWT Bearer token auth. Token refresh via `vol_exp` response header. Base URL from `VITE_API_BASE_URL` env var (dev: `http://127.0.0.1:9200/`).
- **UI:** Element Plus. Custom business components in `src/comp/`, reusable components in `src/components/`.
- **Path alias:** `@` → `src/`
- **Key libs:** SignalR (real-time), ECharts (charts), hiprint (printing), VTable/Gantt (tables), AMap (maps), wangeditor (rich text)

### Backend (.NET)

- **Solution projects:**
  - `HDPro.WebApi` — API host, controllers, SignalR hubs, startup config
  - `HDPro.Core` — Framework core: base controllers, Dapper/EF data access, caching, Quartz jobs, middleware, filters
  - `HDPro.Entity` — Domain models / EF entities
  - `HDPro.CY.Order` — Order module business logic (services, repositories)
  - `HDPro.Sys` — System module (users, menus, authorization)
  - `HDPro.MES` — Manufacturing execution module
  - `HDPro.Builder` — Dynamic form builder
  - `HDPro.Utilities` — Shared utility functions
- **Patterns:** Service-Repository with interface segregation (`IServices/`, `IRepositories/`). Autofac for DI. JWT auth. NLog for logging.
- **Data access:** Hybrid EF Core + Dapper. Controllers in `Controllers/{Module}/` folders.
- **Real-time:** SignalR hubs for WebSocket messaging.
- **Scheduling:** Quartz.NET for background jobs.

### API Convention

REST endpoints follow: `POST /api/{module}/{entity}/{action}` (e.g., `/api/Order/OCP_OrderMain/GetPageData`). Most queries use POST with JSON body parameters.

## Environment Configuration

| Env | API Base URL | Env file |
|-----|-------------|----------|
| Development | `http://127.0.0.1:9200/` | `web.vite/.env.development` |
| Staging | See `.env.staging` | `web.vite/.env.staging` |
| Production | `http://10.11.0.18:9200/` | `web.vite/.env.production` |

## Development Rules (from .cursorrules)

- The project uses the **vol enterprise framework** — routing, theming, and user session are already handled. Do not re-wrap these.
- Use **Element Plus** for UI components. Do not introduce other UI libraries.
- Write clear Chinese comments for pages, components, and API interfaces.
- Follow Vue 3 Composition API conventions.
- Do not blindly extract abstractions — decide based on the actual scenario whether reuse is warranted.
