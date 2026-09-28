# UTHers API — ASP.NET Core .NET 10 Instructions / Hướng dẫn ASP.NET Core .NET 10

These rules extend the repository-root `AGENTS.md`.
Các rule này bổ sung cho `AGENTS.md` ở root repository.

## Runtime and persistence baseline / Nền tảng runtime và persistence

- Target .NET 10 (`net10.0`) unless the user explicitly approves a different target.
  Target .NET 10 (`net10.0`) trừ khi người dùng phê duyệt target khác.
- Database is PostgreSQL.
  Database là PostgreSQL.
- Planned persistence baseline is Entity Framework Core + Npgsql.
  Persistence baseline dự kiến là Entity Framework Core + Npgsql.
- Do not switch database provider, ORM, or persistence architecture without explicit user approval.
  Không đổi database provider, ORM hoặc persistence architecture nếu chưa được phê duyệt.
- Package installation/removal/upgrades and migrations still require explicit approval before execution.
  Cài/xóa/nâng cấp package và migration vẫn phải xin phép trước khi thực hiện.

## Backend responsibility / Trách nhiệm backend

- The backend is the integration boundary between UTHers clients and university systems.
  Backend là boundary tích hợp giữa client UTHers và hệ thống trường.
- Keep upstream HTTP/session/JWT mechanics inside infrastructure/integration code.
  Giữ cơ chế HTTP/session/JWT upstream trong infrastructure/integration code.
- Controllers/endpoints should depend on application-facing services/contracts, not raw university response DTOs.
  Controller/endpoint phụ thuộc service/contract hướng ứng dụng, không phụ thuộc raw DTO trường.
- Keep domain/application logic independent from ASP.NET transport details where practical.
  Giữ domain/application logic độc lập với ASP.NET transport detail khi thực tế phù hợp.

## University adapters / Adapter hệ thống trường

Maintain separate integration clients/adapters for:
Duy trì client/adapter riêng cho:
- Portal
- Courses
- THNN

Known authentication behavior / Hành vi xác thực đã biết:
- Portal: JWT.
- Courses: PHP form/session.
- THNN: PHP form/session.
- Portal refresh token: not confirmed / chưa xác nhận.

Rules / Quy tắc:
- Do not merge the three integrations into one generic client that hides materially different auth/session behavior.
  Không gộp ba integration thành một generic client che khuất khác biệt auth/session.
- Never invent undocumented endpoints, refresh flows, cookie behavior, or schemas.
  Không tự tạo endpoint, refresh flow, cookie behavior hoặc schema chưa được xác minh.
- Normalize upstream responses into UTHers-owned contracts/models at the integration boundary.
  Chuẩn hóa upstream response thành contract/model do UTHers sở hữu tại integration boundary.

## HTTP integration / Tích hợp HTTP

- Prefer dependency-injected `HttpClient` patterns established by ASP.NET Core and the repository.
  Ưu tiên `HttpClient` qua dependency injection theo ASP.NET Core và pattern repo.
- Keep base URLs, timeouts, headers, auth/session behavior, and serialization quirks inside the relevant adapter configuration.
  Giữ base URL, timeout, header, auth/session behavior và serialization quirk trong config adapter tương ứng.
- Translate upstream failures into explicit UTHers application/integration errors rather than leaking raw upstream errors to the frontend.
  Chuyển upstream failure thành lỗi application/integration UTHers rõ ràng thay vì lộ raw upstream error xuống frontend.
- Do not blindly retry authentication or non-idempotent upstream operations.
  Không retry mù quáng auth hoặc thao tác upstream không idempotent.

## Sensitive data / Dữ liệu nhạy cảm

- Never log passwords, full JWTs, authorization headers, session cookies, captcha tokens, or `cf_clearance`.
  Không log password, JWT đầy đủ, authorization header, session cookie, captcha token hoặc `cf_clearance`.
- Do not persist plaintext passwords.
  Không lưu password plaintext.
- Redact personal/sensitive upstream response fields before logging.
  Mask field cá nhân/nhạy cảm của upstream trước khi log.
- Prefer server-side ownership of upstream auth/session state.
  Ưu tiên backend quản lý auth/session state upstream.

## Database rules / Quy tắc database

- PostgreSQL-specific behavior should be tested against PostgreSQL for integration tests rather than relying only on an in-memory substitute.
  Behavior đặc thù PostgreSQL nên được integration test với PostgreSQL thật thay vì chỉ dùng in-memory substitute.
- Keep persistence concerns out of domain models/services where practical.
  Giữ persistence concern khỏi domain model/service khi thực tế phù hợp.
- Do not create/apply/remove migrations without explicit user approval.
  Không tạo/chạy/xóa migration khi chưa được phê duyệt.
- Do not run destructive database commands against a non-test database as part of automated verification.
  Không chạy command database phá hủy trên database không phải test trong automated verification.

## Backend testing / Kiểm thử backend

Project direction / Hướng đã chốt:
- xUnit for unit tests.
- `WebApplicationFactory` for ASP.NET Core API integration tests.
- Testcontainers for temporary PostgreSQL integration environments.

Rules / Quy tắc:
- Unit-test domain/application logic independently where practical.
  Unit test domain/application logic độc lập khi thực tế phù hợp.
- Unit-test university adapter mapping, normalization, and error translation using mocks/fakes/sanitized fixtures.
  Unit test mapping, normalization và error translation của adapter trường bằng mock/fake/fixture đã sanitize.
- API integration tests should exercise the real ASP.NET Core pipeline through `WebApplicationFactory` when appropriate.
  API integration test nên chạy qua ASP.NET Core pipeline thật bằng `WebApplicationFactory` khi phù hợp.
- Persistence integration tests should use temporary PostgreSQL via Testcontainers when the behavior depends on real PostgreSQL/EF Core semantics.
  Persistence integration test nên dùng PostgreSQL tạm qua Testcontainers khi behavior phụ thuộc semantics PostgreSQL/EF Core thật.
- Never use real student credentials in automated tests.
  Không dùng credential sinh viên thật trong automated test.
- Never call live university login endpoints in unit/default CI tests.
  Không gọi live login endpoint trường trong unit/default CI test.
- Live-system tests must be separate and opt-in.
  Test hệ thống trường thật phải tách riêng và opt-in.
- Bug fixes should include regression tests when feasible.
  Bug fix nên có regression test khi khả thi.

## Verification / Kiểm tra

When relevant and already configured, run:
Khi liên quan và đã được cấu hình, chạy:
- `dotnet build`
- relevant `dotnet test` projects
- relevant API/PostgreSQL integration suites

Do not add packages or migrations merely to make verification run without explicit approval.
Không tự thêm package hoặc migration chỉ để chạy verification khi chưa được phê duyệt.
