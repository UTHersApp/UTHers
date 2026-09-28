# UTHers — Codex Project Instructions / Hướng dẫn dự án cho Codex

## 1. Project baseline / Nền tảng dự án

- VI: UTHers là ứng dụng web PWA dành cho sinh viên UTH, phát triển trong một monorepo.
- EN: UTHers is a PWA web application for UTH students, developed as a monorepo.
- Frontend: React + TypeScript + PWA, managed with `npm`.
- Backend: ASP.NET Core on .NET 10 (`net10.0`), managed with `dotnet`.
- Database: PostgreSQL.
- Planned persistence baseline / Nền tảng persistence dự kiến: Entity Framework Core + Npgsql.
- Git flow: `main` -> `develop` -> `feature/*`.
- VI: Frontend chỉ giao tiếp với backend UTHers. Backend là integration boundary duy nhất với API/hệ thống của trường.
- EN: The frontend communicates only with the UTHers backend. The backend is the only integration boundary to university APIs/systems.

## 2. Mandatory approval boundaries / Các thay đổi bắt buộc phải xin phép

Codex MUST stop and ask the user before doing any of the following:
Codex PHẢI dừng lại và hỏi người dùng trước khi thực hiện các việc sau:

1. Add, install, remove, replace, or upgrade any npm/NuGet dependency or development tool.
   Thêm, cài, xóa, thay thế hoặc nâng cấp bất kỳ npm/NuGet dependency hoặc development tool nào.
2. Create, remove, rewrite, or apply a database migration/schema migration.
   Tạo, xóa, viết lại hoặc chạy database migration/schema migration.
3. Introduce a new major framework, infrastructure component, persistence technology, state-management system, or cross-cutting architectural pattern.
   Thêm framework lớn, thành phần hạ tầng, công nghệ persistence, state-management system hoặc architectural pattern ảnh hưởng toàn hệ thống.
4. Change the selected runtime/platform baseline: React + TypeScript, .NET 10, or PostgreSQL.
   Thay đổi nền tảng đã chốt: React + TypeScript, .NET 10 hoặc PostgreSQL.

Do not treat silence or prior general approval as approval for a new dependency-changing command.
Không xem sự im lặng hoặc phê duyệt chung trước đó là phê duyệt cho một command thay đổi dependency mới.

## 3. Git ownership / Quyền sở hữu Git

Codex may inspect Git state but MUST NOT stage, commit, push, merge, rebase, tag, or perform destructive history/working-tree operations for the user.
Codex được đọc trạng thái Git nhưng KHÔNG được stage, commit, push, merge, rebase, tag hoặc thực hiện thao tác phá hủy history/working tree thay người dùng.

Allowed read-only examples / Ví dụ chỉ đọc:
- `git status`
- `git diff`
- `git log`
- `git show`
- `git rev-parse`
- `git branch --show-current`

User-owned actions / Hành động do người dùng thực hiện:
- `git add`
- `git commit`
- `git push`
- merge/rebase
- tags/releases
- destructive reset/clean

Codex may edit project files only. The user owns staging, committing, pushing, merging, and releases.
Codex chỉ được sửa file trong project. Người dùng chịu trách nhiệm stage, commit, push, merge và release.

## 4. University integration boundaries / Ranh giới tích hợp hệ thống trường

Treat Portal, Courses, and THNN as independent external systems.
Xem Portal, Courses và THNN là ba hệ thống bên ngoài độc lập.

Known behavior / Hành vi đã xác nhận:
- Portal uses JWT authentication.
- Courses uses PHP form/session authentication.
- THNN uses PHP form/session authentication.
- A Portal timetable endpoint has been identified.
- Portal JWT lifetime observed from inspected token data is about 30 days, but lifetime remains an upstream behavior and must not be hardcoded as a universal guarantee.
- A Portal refresh-token flow has NOT been confirmed.

Mandatory rules / Quy tắc bắt buộc:
- Never invent undocumented university API behavior.
  Không tự suy đoán hành vi API trường chưa được xác minh.
- Never create a refresh-token endpoint/client flow unless evidence confirms it exists.
  Không tự tạo refresh-token flow nếu chưa có bằng chứng cơ chế đó tồn tại.
- Do not assume session lifetime, cookie behavior, response schema, error schema, retry behavior, or API availability without evidence.
  Không giả định lifetime session, cookie behavior, response/error schema, retry behavior hoặc availability khi chưa có bằng chứng.
- Keep Portal, Courses, and THNN integrations separated behind dedicated adapters/clients.
  Giữ Portal, Courses và THNN tách biệt qua adapter/client riêng.
- Do not expose raw university DTOs to UI/domain code.
  Không đưa raw DTO của trường vào UI/domain code.
- Normalize upstream data at the backend boundary before returning application-facing contracts.
  Chuẩn hóa dữ liệu upstream tại backend boundary trước khi trả contract hướng ứng dụng.

## 5. Authentication and sensitive data / Xác thực và dữ liệu nhạy cảm

Never log, commit, document with real values, or expose to the browser unnecessarily:
Không bao giờ log, commit, ghi tài liệu bằng giá trị thật hoặc đưa xuống browser khi không cần thiết:

- passwords / mật khẩu
- full JWTs / JWT đầy đủ
- `Authorization` headers
- session cookies / cookie phiên
- `Set-Cookie` values
- captcha tokens
- `cf_clearance`
- real student credentials / tài khoản sinh viên thật

Additional rules / Quy tắc bổ sung:
- Never hardcode real credentials or tokens.
  Không hardcode credential hoặc token thật.
- Do not persist plaintext passwords.
  Không lưu mật khẩu plaintext.
- Prefer backend-owned upstream authentication/session state.
  Ưu tiên backend quản lý trạng thái auth/session upstream.
- Do not expose Portal JWT or Courses/THNN session cookies to the PWA unless the user explicitly approves that architecture.
  Không đưa Portal JWT hoặc session cookie Courses/THNN xuống PWA nếu chưa được người dùng phê duyệt rõ ràng.
- Redact secrets and sensitive personal fields in logs and errors.
  Mask secret và dữ liệu cá nhân nhạy cảm trong log và error.

## 6. Frontend/backend boundary / Ranh giới frontend/backend

- React PWA -> UTHers .NET API -> university adapters.
- Browser code MUST NOT directly call Portal, Courses, or THNN.
  Browser code KHÔNG được gọi trực tiếp Portal, Courses hoặc THNN.
- University integration logic belongs in backend infrastructure/integration code.
  Logic tích hợp trường thuộc backend infrastructure/integration code.
- Frontend-facing API contracts must be stable and application-oriented, not copies of upstream payloads.
  Contract cho frontend phải ổn định, hướng ứng dụng, không copy nguyên payload upstream.

## 7. Coding and language conventions / Quy ước code và ngôn ngữ

- Frontend source code MUST use TypeScript (`.ts` / `.tsx`), not new plain JavaScript files, unless the tool/config format itself requires JavaScript and no TypeScript equivalent is practical.
  Source frontend PHẢI dùng TypeScript (`.ts` / `.tsx`), không tạo JavaScript thuần mới trừ khi tooling/config bắt buộc.
- Use English for identifiers: variables, classes, methods, filenames, API routes, database names, and public contracts.
  Dùng tiếng Anh cho identifier: biến, class, method, filename, API route, database name và public contract.
- For non-obvious comments, use concise bilingual comments only when a comment is genuinely useful.
  Với logic không hiển nhiên, dùng comment song ngữ ngắn gọn khi thực sự cần.
- Preferred comment format / Format comment ưu tiên:
  - `// VI: ...`
  - `// EN: ...`
- Do not comment obvious code.
  Không comment code tự rõ nghĩa.
- New project documentation must be bilingual Vietnamese + English.
  Tài liệu project mới phải song ngữ Việt + Anh.

## 8. Change discipline / Kỷ luật thay đổi

- Change only what is required by the task.
  Chỉ thay đổi những gì task yêu cầu.
- Do not perform unrelated refactors.
  Không refactor ngoài phạm vi.
- Preserve public contracts unless the task explicitly changes them.
  Giữ public contract trừ khi task yêu cầu thay đổi.
- Prefer small, reviewable changes.
  Ưu tiên thay đổi nhỏ, dễ review.
- When behavior is uncertain, inspect existing code/evidence or ask the user instead of guessing.
  Khi chưa chắc hành vi, kiểm tra code/bằng chứng hoặc hỏi người dùng thay vì đoán.

## 9. Automated testing strategy / Chiến lược kiểm thử tự động

UTHers aims for full-project automated verification, using the appropriate test layer rather than forcing every scenario into E2E.
UTHers hướng tới kiểm thử tự động toàn dự án bằng tầng test phù hợp, không ép mọi scenario thành E2E.

Planned testing baseline / Bộ test đã chốt:
- Frontend unit/component: Vitest + React Testing Library.
- Backend unit: xUnit.
- Backend API integration: xUnit + `WebApplicationFactory`.
- PostgreSQL integration: Testcontainers with a real temporary PostgreSQL instance.
- Browser E2E: Playwright.

These tools are the agreed project direction, but installing/changing their packages still follows the explicit dependency-approval rule.
Các tool này là hướng kỹ thuật đã thống nhất, nhưng command cài/thay đổi package vẫn phải tuân theo rule xin phép dependency.

Testing rules / Quy tắc test:
- Every new or changed behavior must have appropriate automated tests when technically testable.
  Mỗi behavior mới hoặc thay đổi phải có automated test phù hợp khi có thể test được.
- Prioritize unit tests for pure/domain/business logic, integration tests for boundaries, and a smaller set of E2E tests for critical user journeys.
  Ưu tiên unit test cho logic thuần/domain/business, integration test cho boundary và số lượng E2E nhỏ hơn cho user journey quan trọng.
- Do NOT chase 100% code coverage as a goal by itself.
  KHÔNG lấy 100% code coverage làm mục tiêu tự thân.
- Critical authentication, authorization, university adapter mapping/error handling, persistence, and data-normalization paths require explicit automated coverage.
  Các luồng auth, authorization, adapter trường, persistence và data normalization quan trọng phải có coverage tự động rõ ràng.
- A bug fix should include a regression test when feasible.
  Bug fix nên có regression test khi khả thi.
- Tests must be deterministic and independent where practical.
  Test phải deterministic và độc lập khi thực tế cho phép.

## 10. University API test isolation / Cô lập test API trường

- Unit and default CI tests MUST NOT call live university login/API endpoints.
  Unit test và CI mặc định KHÔNG được gọi live login/API endpoint của trường.
- Never use real student credentials, real tokens, or real session cookies in fixtures.
  Không dùng credential, token hoặc session cookie thật trong fixture.
- Use mocked/fake HTTP handlers, captured sanitized fixtures, and contract-oriented adapter tests.
  Dùng mocked/fake HTTP handler, fixture đã sanitize và contract-oriented adapter test.
- Live university integration tests, if created, must be clearly separated, opt-in, non-default, and must read secrets from approved secure environment configuration rather than repository files.
  Live integration test với hệ thống trường nếu có phải tách riêng, opt-in, không chạy mặc định và lấy secret từ secure environment config đã được phê duyệt thay vì file trong repo.
- Default CI success must not depend on university uptime, captcha services, or real student accounts.
  CI mặc định không được phụ thuộc uptime của trường, captcha service hoặc tài khoản sinh viên thật.

## 11. Verification before completion / Xác minh trước khi hoàn thành

Before finishing a coding task, run the smallest relevant existing checks first, then broader checks when the change warrants them, without adding dependencies automatically.
Trước khi kết thúc task code, chạy kiểm tra hiện có nhỏ nhất liên quan trước, sau đó chạy kiểm tra rộng hơn khi thay đổi yêu cầu, không tự thêm dependency.

Typical checks when configured / Các check thường dùng khi đã được cấu hình:
- Frontend: typecheck, lint, Vitest, production build.
- Backend: `dotnet build`, relevant `dotnet test` projects.
- Integration: relevant API/PostgreSQL integration suite.
- E2E: relevant Playwright specs for critical cross-stack behavior.

Do not run the entire slow E2E suite for a tiny isolated change unless needed to establish confidence or the repository workflow requires it.
Không chạy toàn bộ E2E suite chậm cho thay đổi nhỏ, cô lập nếu không cần thiết hoặc workflow repo không yêu cầu.

Never claim a check/test passed if it was not actually run.
Không được nói check/test đã pass nếu thực tế chưa chạy.

## 12. Completion report / Báo cáo khi hoàn thành

When finishing a task, briefly report:
Khi hoàn thành task, báo cáo ngắn gọn:

1. Files changed / File đã thay đổi.
2. What was implemented / Đã triển khai gì.
3. Tests/checks actually run and their result / Test/check thực tế đã chạy và kết quả.
4. Unresolved assumptions or risks / Giả định hoặc rủi ro còn lại.
5. Actions still requiring user approval / Việc còn cần người dùng phê duyệt.
