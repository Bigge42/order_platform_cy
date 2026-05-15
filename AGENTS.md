# Repository Guidelines

## Project Structure & Module Organization

This repository is an order management platform monorepo:

- `api/` contains the ASP.NET Core backend solution `Haodee.CY.OCP.sln`. Key projects are `HDPro.WebApi`, `HDPro.Core`, `HDPro.Entity`, and `HDPro.CY.Order`.
- `web.vite/` is the main Vue 3 + Vite + Element Plus frontend. Source lives in `src/`, reusable UI in `src/components/` and `src/comp/`, assets in `public/`.
- `dataview/` is the Vue 3 data visualization app based on Go-View and ECharts.
- `app/` is a uni-app client with pages under `pages/`, shared components under `components/`, and packaged modules under `uni_modules/`.
- `docs/` stores feature and integration notes.

## Build, Test, and Development Commands

- `cd web.vite && npm run dev`: start the main frontend on port `9010`.
- `cd web.vite && npm run build`: production frontend build.
- `cd web.vite && npm run test:unit`: run Vitest unit tests.
- `cd web.vite && npm run lint && npm run type-check`: fix lint issues and run Vue/TypeScript checks.
- `cd dataview && npm run dev`: start the dashboard app.
- `cd dataview && npm run build`: type-check and build dashboards.
- `cd api && dotnet build Haodee.CY.OCP.sln`: build backend projects.
- `dev_run.bat`: run `HDPro.WebApi` with `dotnet watch`.

## Coding Style & Naming Conventions

Frontend code uses Prettier from `web.vite/.prettierrc.json`: 2 spaces, single quotes, no semicolons, `printWidth` 100, and no trailing commas. Use Vue 3 Composition API and Element Plus. Preserve generated names such as `OCP_*`, `Sys_*`, and module folders matching table/entity names.

C# code should follow the service/repository layout: `Controllers/{Module}/`, `Services/`, `IServices/`, `Repositories/`, and `IRepositories/`. Keep custom logic in `Partial/` folders where already used.

## Testing Guidelines

Use Vitest for `web.vite` tests, with specs under `__tests__/` or `tests/unit/` using `*.spec.ts` or `*.spec.js`. No strict coverage threshold is configured. The backend has no dedicated unit-test project; validate backend changes with `dotnet build` and targeted API checks.

## Commit & Pull Request Guidelines

Recent commits use short summaries, sometimes terse. Prefer a concise imperative subject that names the change, for example `fix: correct export filters` or `feat: add BOM query fallback`. `dataview` includes Conventional Commit tooling, so use `feat:`, `fix:`, `docs:`, or `chore:` there when possible.

Pull requests should include a short description, affected areas (`api`, `web.vite`, `dataview`, `app`), validation commands run, linked issue/task, and screenshots for UI changes.

## Security & Configuration Tips

Do not commit secrets or machine-specific overrides. Review `.env.*`, `appsettings*.json`, generated downloads, and logs before staging changes.
