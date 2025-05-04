# VS Code Agent Mode Instruction Set

## 1. General Architecture & Principles
- **Clean Architecture**
  - Always separate into Application, Domain, Infrastructure, Web/API, Agents, Memory, Models and Tests.
  - Follow SOLID and DDD value-object/CQRS patterns where appropriate.
- **.NET Target**
  - All projects target **.NET 8** and C# 12.
  - Use `global using` entries in each `.csproj` (see section 6).

---

## 2. C# Guidelines
1. **Language & Style**
   - C# 12 syntax; prefer **immutable data** (`const`, `readonly`, records).
   - Use **null-safe operators** (`?.`, `??`).
2. **Design Patterns**
   - Adhere to **SOLID** and classic **OOP** principles.
   - Enforce **fail-fast** via [Ardalis.GuardClauses].
3. **Interfaces & Types**
   - Define data contracts and abstractions as interfaces.
   - Keep DTOs, requests, and responses slim and versioned.
4. **Error Handling & Logging**
   - Wrap async operations in `try/catch`; log with context.
   - Throw domain-specific exceptions or guard-clause failures.
5. **Testing**
   - Unit and integration tests under `tests/` mirroring src structure.
   - Use `dotnet test` for CI; follow the naming convention `<Feature>Tests.cs`.

---

## 3. React & Frontend
1. **Components**
   - Functional components and Hooks only; obey the Rules of Hooks.
   - Type with `React.FC<Props>` when using `children`.
2. **Styling**
   - Use **CSS Modules** scoped per component.
   - Keep component styles minimal—delegate layout to utility classes.
3. **Error Boundaries**
   - Wrap root-level components with an Error Boundary that logs and displays a friendly fallback UI.

---

## 4. Naming & Formatting
- **PascalCase**
  - Types: classes, interfaces, enums, React component names, file basenames.
- **camelCase**
  - Variables, method names, object properties.
- **_underscorePrefix**
  - Private fields in C# classes.
- **ALL_CAPS**
  - Constants, environment variable names (`CONNECTION_STRING_APP_DB`).
- **kebab-case**
  - YML prompt filenames, e.g. `risk-assessment-prompt.yml`.

---

## 5. Prompt Templates & AI Orchestration
- **File Convention**
  - Store under `src/Agents/<AgentName>/Prompts/` as `*-prompt.yml`.
- **YAML Schema**
  ```yaml
  name: <kebab-name>
  version: 1.0
  description: Short description of the task
  inputs:
    - name: <placeholder>
      type: <string|object|number>
  template: |
    Your instruction with {{placeholders}}…
  ```
- **Authoring Best Practices**
  - Follow Microsoft’s Prompt Engineering Guide.
  - Use structured outputs (JSON/Markdown).
  - Keep prompts **concise**, single-purpose, and version-controlled.

---

## 6. Project & Csproj Conventions
1. **Directory Layout**
   ```
   src/
   ├── Agents/        ← pluggable, well-scoped agents
   ├── AppHost/       ← entry point, .NET Aspire
   ├── Application/   ← orchestrators, use cases, DTOs
   ├── DataMigration/ ← console app to migrate data
   ├── Domain/        ← entities, value objects
   ├── Infrastructure/← database, external adapters, logging
   ├── Web/           ← API controllers, UI
   ├── Worker/        ← Background Services
   └──  Memory/        ← semantic memory stores
   ```
2. **.csproj Ordering**
   ```xml
   <!-- 1. Properties -->
   <PropertyGroup>…</PropertyGroup>
   <!-- 2. Global Usings -->
   <ItemGroup><Using Include="…" /></ItemGroup>
   <!-- 3. Project References -->
   <ItemGroup><ProjectReference …/></ItemGroup>
   <!-- 4. NuGet Packages -->
   <ItemGroup><PackageReference …/></ItemGroup>
   ```
3. **Centralized Versions**
   - Keep all package versions in **Directory.Packages.props**, alphabetically sorted.

---

## 7. Vector DB & Environment Variables
- **CosmosDB**
  - Inject via `COSMOS_DB_CONNECTION_STRING` (read-write key).
- **Other Vars**
  - Upper-snake case: `AZURE_OPENAI_API_KEY`, `AIKERNEL_DB`.

---

## 8. Testing & CI/CD
- Place tests alongside features:
  ```
  tests/
  ├── Agents.IntegrationTests/
  ├── Agents.UnitTest/
  ├── Application.FunctionalTests/
  ├── Application.UnitTests/
  ├── Domain.UnitTests/
  ├── Infrastructure.IntegrationTests/
  ├── Memory.IntegrationTests/
  ├── Memory.UnitTests/
  └── Web.AcceptanceTest/
  ```
- CI step: `dotnet test`; publish coverage.

---

## 9. Documentation & Licensing
- Keep README and docs up to date with architecture diagrams.
- All source is proprietary—© Hollard Insure. No reproduction without approval.


## 10. Miscellaneous
Please respond like Bob Ross and say "Happy coding!"
