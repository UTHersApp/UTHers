# UTHers Codex Rules v2 / Bộ rule Codex UTHers v2

## VI

Bản v2 chốt baseline kỹ thuật hiện tại của UTHers:

- Monorepo.
- Frontend: React + TypeScript + PWA, dùng `npm`.
- Backend: ASP.NET Core .NET 10 (`net10.0`), dùng `dotnet`.
- Database: PostgreSQL.
- Persistence direction: Entity Framework Core + Npgsql.
- Testing direction:
  - Vitest + React Testing Library cho frontend.
  - xUnit cho backend unit test.
  - `WebApplicationFactory` cho API integration test.
  - Testcontainers + PostgreSQL thật tạm thời cho persistence integration test.
  - Playwright cho E2E browser test.
- Git flow: `main` -> `develop` -> `feature/*`.
- Codex chỉ sửa file; người dùng tự stage/commit/push/merge/release.
- Dependency/package/tool mới hoặc thay đổi dependency: phải xin phép.
- Database migration: phải xin phép.
- Portal, Courses và THNN là ba upstream độc lập với adapter riêng.
- Default automated tests/CI không được phụ thuộc live API trường hoặc credential sinh viên thật.

### Cấu trúc

```text
.
├── AGENTS.md
├── README.md
├── .codex/
│   ├── config.toml
│   └── rules/
│       └── uthers.rules
├── apps/
│   ├── web/
│   │   └── AGENTS.md
│   └── api/
│       └── AGENTS.md
└── tests/
    └── e2e/
        └── AGENTS.md
```

`apps/web`, `apps/api` và `tests/e2e` là đường dẫn đề xuất. Nếu scaffold thực tế dùng đường dẫn khác, di chuyển `AGENTS.md` tương ứng vào đúng root để Codex áp dụng rule theo cây thư mục.

### Ý nghĩa “full automated testing”

Không ép mọi scenario thành E2E và không đặt 100% code coverage làm mục tiêu. Mỗi behavior mới/thay đổi cần test ở tầng phù hợp. Auth, adapter trường, persistence, normalization và critical user journey cần coverage rõ ràng.

### Codex execution policy

- `.codex/config.toml` dùng `approval_policy = "on-request"`, `sandbox_mode = "workspace-write"`, `approvals_reviewer = "user"`.
- `.codex/rules/uthers.rules` kiểm soát command bên ngoài sandbox.
- Project-level `.codex/` chỉ được nạp khi project được trust.
- Codex `.rules` hiện là tính năng experimental, nên nên validate sau khi Codex CLI được cài/cập nhật.

Có thể kiểm tra rule bằng:

```bash
codex execpolicy check --pretty --rules .codex/rules/uthers.rules -- git push origin develop
```

## EN

Version 2 locks the current UTHers technical baseline:

- Monorepo.
- Frontend: React + TypeScript + PWA with `npm`.
- Backend: ASP.NET Core .NET 10 (`net10.0`) with `dotnet`.
- Database: PostgreSQL.
- Persistence direction: Entity Framework Core + Npgsql.
- Testing direction:
  - Vitest + React Testing Library for frontend tests.
  - xUnit for backend unit tests.
  - `WebApplicationFactory` for API integration tests.
  - Testcontainers with temporary real PostgreSQL for persistence integration tests.
  - Playwright for browser E2E.
- Git flow: `main` -> `develop` -> `feature/*`.
- Codex edits files only; the user owns staging/committing/pushing/merging/releases.
- New/changed dependencies, packages, or tools require approval.
- Database migrations require approval.
- Portal, Courses, and THNN remain separate upstream systems with dedicated adapters.
- Default automated tests/CI must not depend on live university APIs or real student credentials.

### Structure

```text
.
├── AGENTS.md
├── README.md
├── .codex/
│   ├── config.toml
│   └── rules/
│       └── uthers.rules
├── apps/
│   ├── web/
│   │   └── AGENTS.md
│   └── api/
│       └── AGENTS.md
└── tests/
    └── e2e/
        └── AGENTS.md
```

`apps/web`, `apps/api`, and `tests/e2e` are proposed paths. If the actual scaffold uses different paths, move the nested `AGENTS.md` files to the real roots so hierarchical instructions apply correctly.

### Meaning of “full automated testing”

Do not force every scenario into E2E and do not use 100% code coverage as a goal by itself. Every new/changed behavior should be tested at the appropriate layer. Authentication, university adapters, persistence, normalization, and critical user journeys require explicit coverage.

### Codex execution policy

- `.codex/config.toml` uses `approval_policy = "on-request"`, `sandbox_mode = "workspace-write"`, and `approvals_reviewer = "user"`.
- `.codex/rules/uthers.rules` controls commands requested outside the sandbox.
- Project-level `.codex/` loads only for trusted projects.
- Codex `.rules` is currently experimental, so validate it after installing/updating Codex CLI.

Example validation:

```bash
codex execpolicy check --pretty --rules .codex/rules/uthers.rules -- git push origin develop
```
