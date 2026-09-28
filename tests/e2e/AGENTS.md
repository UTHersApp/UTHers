# UTHers E2E — Playwright Instructions / Hướng dẫn Playwright E2E

These rules extend the repository-root `AGENTS.md`.
Các rule này bổ sung cho `AGENTS.md` ở root repository.

## Purpose / Mục đích

- E2E tests cover critical user journeys across the PWA and UTHers backend.
  E2E test cover user journey quan trọng xuyên suốt PWA và backend UTHers.
- Keep the E2E suite focused; do not duplicate every unit/component/integration assertion in browser tests.
  Giữ E2E suite tập trung; không duplicate mọi assertion của unit/component/integration test bằng browser test.

## Isolation / Cô lập

- Default E2E MUST NOT depend on live Portal, Courses, or THNN services.
  E2E mặc định KHÔNG phụ thuộc Portal, Courses hoặc THNN thật.
- Use controlled test doubles/stubs or a deterministic test backend boundary for university upstream behavior.
  Dùng test double/stub có kiểm soát hoặc test backend deterministic cho upstream trường.
- Never store real student credentials, JWTs, session cookies, captcha tokens, or `cf_clearance` in E2E source, fixtures, snapshots, traces, videos, or screenshots.
  Không lưu credential sinh viên, JWT, session cookie, captcha token hoặc `cf_clearance` thật trong source, fixture, snapshot, trace, video hoặc screenshot E2E.

## Test quality / Chất lượng test

- Prefer resilient selectors: accessible roles, labels, test IDs only when necessary.
  Ưu tiên selector bền vững: accessible role, label, test ID chỉ khi cần.
- Avoid arbitrary sleeps; rely on Playwright waiting/assertion behavior.
  Tránh sleep tùy ý; dựa vào waiting/assertion của Playwright.
- Keep tests independent and deterministic where practical.
  Giữ test độc lập và deterministic khi thực tế cho phép.
- Critical journeys should include success and meaningful failure/recovery paths.
  User journey quan trọng nên có success path và failure/recovery path có ý nghĩa.
- Collect trace/screenshot/video artifacts on failure according to repository configuration, while ensuring they do not contain real secrets.
  Thu trace/screenshot/video khi fail theo config repo nhưng phải đảm bảo không chứa secret thật.

## Suggested critical journeys / User journey quan trọng gợi ý

As features are implemented, prioritize flows such as:
Khi feature được triển khai, ưu tiên các flow như:
- app boot/navigation
- authentication through the UTHers backend
- dashboard loading
- timetable loading/display
- normalized upstream error handling
- session-expiry/re-authentication behavior once the product contract is defined

Do not invent product behavior solely to create an E2E test.
Không tự bịa product behavior chỉ để tạo E2E test.
