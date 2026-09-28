# UTHers Web — React + TypeScript PWA Instructions / Hướng dẫn React + TypeScript PWA

These rules extend the repository-root `AGENTS.md`.
Các rule này bổ sung cho `AGENTS.md` ở root repository.

## Stack / Công nghệ

- Use React with TypeScript for application source.
  Dùng React với TypeScript cho source ứng dụng.
- New React components should use `.tsx`; non-JSX TypeScript modules should use `.ts`.
  Component React mới dùng `.tsx`; module TypeScript không JSX dùng `.ts`.
- Avoid `any`. Use explicit domain/API types, `unknown` plus validation/narrowing when data is untrusted, and inference when it remains clear.
  Hạn chế `any`. Dùng type domain/API rõ ràng, `unknown` + validation/narrowing cho dữ liệu không tin cậy và inference khi vẫn rõ nghĩa.
- Do not introduce a new routing, state-management, styling, form, validation, or data-fetching library without explicit user approval.
  Không thêm routing, state-management, styling, form, validation hoặc data-fetching library mới khi chưa được phê duyệt.

## Frontend boundary / Ranh giới frontend

- The PWA MUST call only the UTHers backend for university data and authentication workflows.
  PWA CHỈ gọi backend UTHers cho dữ liệu và auth workflow liên quan hệ thống trường.
- Never call Portal, Courses, or THNN endpoints directly from browser code.
  Không gọi trực tiếp Portal, Courses hoặc THNN từ browser code.
- Never place university credentials, Portal JWTs, PHP session cookies, captcha tokens, or backend secrets in client storage or client-visible environment variables.
  Không đặt credential trường, Portal JWT, PHP session cookie, captcha token hoặc backend secret vào client storage hoặc biến môi trường lộ phía client.
- Frontend models should match UTHers application-facing API contracts, not raw upstream university payloads.
  Model frontend phải theo contract hướng ứng dụng của UTHers, không theo raw payload upstream.

## PWA safety / An toàn PWA

- Treat browser storage and service-worker caches as client-accessible, non-secret storage.
  Xem browser storage và service-worker cache là nơi phía client truy cập được, không phải nơi chứa secret.
- Do not cache authentication responses, secret-bearing responses, or sensitive personal data offline unless the architecture is explicitly designed and approved for it.
  Không cache auth response, response chứa secret hoặc dữ liệu cá nhân nhạy cảm offline khi chưa có thiết kế và phê duyệt rõ ràng.
- Offline behavior must not weaken authentication, authorization, or privacy boundaries.
  Offline behavior không được làm yếu auth, authorization hoặc privacy boundary.

## Component and state design / Thiết kế component và state

- Keep components focused and favor composition over large all-in-one components.
  Giữ component tập trung và ưu tiên composition thay vì component quá lớn.
- Keep server/API access outside purely presentational components.
  Giữ API/server access ngoài component chỉ có nhiệm vụ trình bày.
- Prefer explicit loading, success, empty, and error states for async UI.
  Ưu tiên state loading, success, empty và error rõ ràng cho UI async.
- Do not duplicate backend business rules in the frontend unless the duplication is intentionally for UX and the backend remains authoritative.
  Không duplicate business rule backend ở frontend trừ khi có chủ đích UX và backend vẫn là nguồn quyết định cuối cùng.

## Frontend testing / Kiểm thử frontend

Project direction / Hướng đã chốt:
- Vitest for unit tests.
- React Testing Library for component/user-interaction tests.
- Playwright for cross-stack/browser E2E tests.

Rules / Quy tắc:
- Test behavior visible to users rather than implementation details where possible.
  Ưu tiên test behavior người dùng thấy thay vì implementation detail.
- New/changed components with meaningful behavior should include relevant automated tests.
  Component mới/thay đổi có behavior đáng kể phải có automated test phù hợp.
- Cover async loading/error/empty states when they are meaningful to the feature.
  Cover loading/error/empty state khi có ý nghĩa với feature.
- Avoid brittle selectors; prefer accessible roles, labels, and user-observable text for component/E2E tests.
  Tránh selector mong manh; ưu tiên role, label và text người dùng quan sát được.
- Do not mock away the behavior under test; mock at clear boundaries.
  Không mock mất chính behavior đang test; chỉ mock tại boundary rõ ràng.

## Verification / Kiểm tra

Use repository-defined scripts when available.
Dùng script đã định nghĩa trong repository khi có.

Relevant checks may include:
- TypeScript typecheck
- lint
- Vitest tests
- production build
- relevant Playwright spec when the change crosses frontend/backend boundaries

Do not install tooling merely to run a check without explicit user approval.
Không cài tooling chỉ để chạy check nếu chưa được người dùng phê duyệt rõ ràng.
