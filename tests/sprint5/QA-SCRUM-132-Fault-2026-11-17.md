# SCRUM-132 — release retest and browser fault handling

QA Nguyễn Thuận assisted by Codex, 17/11/2026 (Asia/Saigon). Application source `origin/main` `8f56909e3088f2d9712ef65f5ddaab11275d64ea` on existing `Thuanaj`. Windows/.NET 10 Release, local HTTPS Web/API, `BranchFlow_Test`. Proxy `tests/sprint4/qa-fault-proxy.ps1` forwarded to the real API and altered only a successful `POST /api/orders` response after upstream HTTP 201. Test-only accounts/orders were removed afterward.

| Evidence on current source | Expected | Actual | Result |
|---|---|---|---|
| API/Web and management/revenue runner builds | Build succeeds | Four Release builds: 0 errors/warnings | Pass |
| SQL-enabled xUnit suite | Full suite passes | 127/127 Pass, 0 Fail/Skip, recorded in [same-source release report](QA-Public-Retest-2026-11-17.md); not rerun in this session | Pass, reused same-source evidence |
| Menu and order adjustment | API/SQL current-source checks pass | [SCRUM-129 report](QA-SCRUM-129-2026-11-17.md): 34/34 Menu and 53/53 Order assertions, plus real UI/SQL journey | Pass |
| Stock and revenue | UI/API/SQL same filter and stock no-autodeduct | [SCRUM-130 report](QA-SCRUM-130-2026-11-17.md): 72/72 management, 12/12 revenue, same-filter UI/API/SQL and stock 7→8→7 | Pass |
| Azure smoke | Four roles and main routes on deployed candidate | [Public report](QA-Public-Retest-2026-11-17.md): 27/27 on owner-reported candidate `2844101`; running binary SHA not independently attested, so this is not a public run of `8f56909` | Pass for observed HTTP, version caveat |
| Human UAT | Three distinct internal participants each create and confirm an order | No attributable confirmation from three participants in SCRUM-131 | Not Run / external input |

## Fault injection on current source

| Branch | Expected | Actual browser/API/SQL | Result |
|---|---|---|---|
| 401 substituted after upstream 201, first run | One POST/order; cart retained, uncertain result shown, resend locked, own-orders link | Proxy logged one upstream create POST. SQL order Id 320, 29,000đ exactly once. Browser kept 1-cup cart and warning, disabled resend; own-orders listed code `...-0003` | Pass for order safety |
| Timeout: proxy holds upstream 201 response for 30 seconds | Same safety behavior | Proxy logged one upstream create POST. SQL order Id 321, 29,000đ exactly once. Browser showed connection/uncertain warning, kept cart, disabled resend; own-orders listed code `...-0004` | Pass for order safety |
| 401 after upstream 201, single Web app rerun | Immediate re-login works | Proxy logged one upstream create POST. SQL Id 322, 29,000đ. First login form submission after clicking “Đăng nhập lại” went to `/auth/login` and displayed “Form đã hết hạn. Vui lòng tải lại trang.”; reload `/login` then login succeeded; cart remained guarded | Fail for first re-login; [SCRUM-140](https://khangba153.atlassian.net/browse/SCRUM-140) |

The first 401 run also produced the form error. Initially two local Web processes shared the `localhost` browser cookies on different ports. The second run stopped the other Web process but reused the browser profile, so cookie interference cannot yet be excluded as cause. SCRUM-140 asks FE to verify on one clean origin. The observed recovery needs an extra page reload; no duplicate order or lost cart was observed. Severity proposed Minor / priority Medium, not a confirmed release blocker.

All three fault orders and the earlier revenue order were removed by a guarded SQL transaction; 0 QA order/user/stock/audit rows remained, stock balance 7 litres. No secrets were logged. No actual hours were inferred from the estimate. QA technical retest is complete; SCRUM-132 stays **In Review** for Khang's assessment of the open observation, deployed SHA and human UAT.
