#!/usr/bin/env bash
# ============================================================
# TaskFlow API — terminal smoke/regression test
# Tests the same flow a reviewer runs through Swagger UI.
#
# Usage:
#   ./scripts/swagger-api-smoke-test.sh
#   BASE_URL=http://localhost:5017 ./scripts/swagger-api-smoke-test.sh
# ============================================================
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:5000}"

# ── colour helpers ───────────────────────────────────────────
RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'
CYAN='\033[0;36m'; BOLD='\033[1m'; RESET='\033[0m'

pass() { echo -e "  ${GREEN}✔ $*${RESET}"; }
fail() { echo -e "  ${RED}✗ $*${RESET}"; }
warn() { echo -e "  ${YELLOW}⚠ $*${RESET}"; }
step() { echo -e "\n${CYAN}${BOLD}$*${RESET}"; }
info() { echo -e "  $*"; }

# ── result variables (one per test; bash 3.2 compatible) ─────
R_SWAGGER="SKIP"; R_REGISTER="SKIP"; R_LOGIN="SKIP"; R_ME="SKIP"
R_CREATE_PROJECT="SKIP"; R_GET_PROJECTS="SKIP"
R_CREATE_TASK="SKIP"; R_GET_TASKS="SKIP"
R_STATUS_INPROGRESS="SKIP"; R_STATUS_DONE="SKIP"
R_CREATE_COMMENT="SKIP"; R_GET_COMMENTS="SKIP"
R_DASHBOARD="SKIP"
R_NEG_A="SKIP"; R_NEG_B="SKIP"; R_NEG_C="SKIP"; R_NEG_D="SKIP"
R_NEG_E="SKIP"; R_NEG_F="SKIP"; R_NEG_G="SKIP"
R_NEG_H="SKIP"
PASSED=0
FAILED=0
WARNINGS=0

# ── jq check ────────────────────────────────────────────────
if ! command -v jq &>/dev/null; then
  echo -e "${RED}ERROR: jq is required but not installed.${RESET}"
  echo "  Install it with:  brew install jq"
  exit 1
fi

# ── HTTP call helper ──────────────────────────────────────────
# Sets globals: HTTP_CODE  BODY
# Returns 0 if HTTP_CODE == expected, 1 otherwise (does NOT exit).
http_call() {
  local expected="$1"; shift
  local tmp; tmp=$(mktemp)
  HTTP_CODE=$(curl -s -o "$tmp" -w "%{http_code}" "$@") || true
  BODY=$(cat "$tmp"); rm -f "$tmp"
  info "HTTP $HTTP_CODE  (expected $expected)"
  [[ "$HTTP_CODE" == "$expected" ]]
}

# ── unique email per run ─────────────────────────────────────
TIMESTAMP=$(date +%s)
EMAIL="reviewer.${TIMESTAMP}@example.com"
PASSWORD="Password123!"
TOKEN=""
PROJECT_ID=""
TASK_ID=""
COMMENT_ID=""
FUTURE_DATE="2026-06-01T00:00:00Z"

echo -e "\n${BOLD}TaskFlow API — terminal smoke test${RESET}"
echo -e "  Base URL : ${CYAN}${BASE_URL}${RESET}"
echo -e "  Email    : ${CYAN}${EMAIL}${RESET}"

# ============================================================
# STEP 0 — Swagger JSON
# ============================================================
step "Step 0 — Swagger JSON"
info "GET ${BASE_URL}/swagger/v1/swagger.json"
HTTP_CODE=$(curl -s -o /tmp/swagger.json -w "%{http_code}" \
  "${BASE_URL}/swagger/v1/swagger.json") || true
info "HTTP $HTTP_CODE"
if [[ "$HTTP_CODE" != "200" ]]; then
  fail "Swagger JSON not available (HTTP $HTTP_CODE)."
  echo -e "${RED}Is the API running at ${BASE_URL}?${RESET}"
  echo "  Start it with:  docker compose up --build"
  R_SWAGGER="FAIL"; exit 1
fi
pass "Swagger JSON 200"
for path in /api/auth/register /api/auth/login /api/auth/me /api/projects /api/tasks /api/dashboard/summary; do
  if jq -e ".paths | has(\"${path}\")" /tmp/swagger.json >/dev/null 2>&1; then
    pass "Path present: $path"
  else
    warn "Path missing in spec: $path"
  fi
done
R_SWAGGER="PASS"

# ============================================================
# STEP 1 — Register
# ============================================================
step "Step 1 — Register"
if http_call "200" \
    -X POST "${BASE_URL}/api/auth/register" \
    -H "Content-Type: application/json" \
    -d "{\"email\":\"${EMAIL}\",\"password\":\"${PASSWORD}\",\"firstName\":\"Jane\",\"lastName\":\"Doe\"}"; then
  TOKEN=$(echo "$BODY" | jq -r '.token // empty')
  if [[ -z "$TOKEN" ]]; then
    fail "No token in response"; info "Body: $BODY"; R_REGISTER="FAIL"; exit 1
  fi
  pass "Token received (${TOKEN:0:20}…)"; R_REGISTER="PASS"
else
  fail "Register failed"; info "Body: $BODY"; R_REGISTER="FAIL"; exit 1
fi

# ============================================================
# STEP 2 — Login
# ============================================================
step "Step 2 — Login"
if http_call "200" \
    -X POST "${BASE_URL}/api/auth/login" \
    -H "Content-Type: application/json" \
    -d "{\"email\":\"${EMAIL}\",\"password\":\"${PASSWORD}\"}"; then
  LOGIN_TOKEN=$(echo "$BODY" | jq -r '.token // empty')
  if [[ -z "$LOGIN_TOKEN" ]]; then
    fail "No token in login response"; R_LOGIN="FAIL"; exit 1
  fi
  TOKEN="$LOGIN_TOKEN"
  pass "Token received (${TOKEN:0:20}…)"; R_LOGIN="PASS"
else
  fail "Login failed"; info "Body: $BODY"; R_LOGIN="FAIL"; exit 1
fi

AUTH_HEADER="Authorization: Bearer ${TOKEN}"

# ============================================================
# STEP 3 — Me
# ============================================================
step "Step 3 — Me  (GET /api/auth/me)"
info "Sending: Authorization: Bearer \${TOKEN}  (NOT Bearer Bearer)"
if http_call "200" -X GET "${BASE_URL}/api/auth/me" -H "$AUTH_HEADER"; then
  ME_EMAIL=$(echo "$BODY" | jq -r '.email // empty')
  ME_FIRST=$(echo "$BODY" | jq -r '.firstName // empty')
  ME_LAST=$(echo "$BODY"  | jq -r '.lastName // empty')
  ME_ROLE=$(echo "$BODY"  | jq -r '.role // empty')
  if [[ "$ME_EMAIL" == "$EMAIL" ]]; then pass "email matches"; else
    fail "email mismatch: got '$ME_EMAIL'"; R_ME="FAIL"; exit 1; fi
  [[ "$ME_FIRST" == "Jane" ]] && pass "firstName = Jane" || warn "firstName: $ME_FIRST"
  [[ "$ME_LAST"  == "Doe"  ]] && pass "lastName = Doe"   || warn "lastName: $ME_LAST"
  [[ "$ME_ROLE"  == "User" ]] && pass "role = User"      || warn "role: $ME_ROLE"
  R_ME="PASS"
else
  fail "Me failed"; info "Body: $BODY"; R_ME="FAIL"; exit 1
fi

# ============================================================
# STEP 4 — Create project
# ============================================================
step "Step 4 — Create project"
if http_call "200" \
    -X POST "${BASE_URL}/api/projects" \
    -H "$AUTH_HEADER" -H "Content-Type: application/json" \
    -d '{"name":"Portfolio Backend","description":"TaskFlow API terminal smoke test project"}'; then
  PROJECT_ID=$(echo "$BODY" | jq -r '.id // empty')
  if [[ -z "$PROJECT_ID" ]]; then
    fail "No id in response"; R_CREATE_PROJECT="FAIL"; exit 1
  fi
  pass "projectId = $PROJECT_ID"; R_CREATE_PROJECT="PASS"
else
  fail "Create project failed"; info "Body: $BODY"; R_CREATE_PROJECT="FAIL"; exit 1
fi

# ============================================================
# STEP 5 — Get projects
# ============================================================
step "Step 5 — Get projects"
if http_call "200" -X GET "${BASE_URL}/api/projects" -H "$AUTH_HEADER"; then
  FOUND=$(echo "$BODY" | jq --arg id "$PROJECT_ID" '[.[] | select(.id==$id)] | length')
  [[ "$FOUND" -ge 1 ]] && pass "Created project in list" || warn "Project not found in list"
  R_GET_PROJECTS="PASS"
else
  fail "Get projects failed"; R_GET_PROJECTS="FAIL"
fi

# ============================================================
# STEP 6 — Create task with string enum priority = "High"
# ============================================================
step "Step 6 — Create task  (priority = \"High\")"
HTTP_CODE=$(curl -s -o /tmp/task_body.json -w "%{http_code}" \
  -X POST "${BASE_URL}/api/tasks" \
  -H "$AUTH_HEADER" -H "Content-Type: application/json" \
  -d "{\"projectId\":\"${PROJECT_ID}\",\"title\":\"Add Swagger terminal tests\",\"description\":\"Verify full TaskFlow API flow through curl\",\"priority\":\"High\",\"dueDate\":\"${FUTURE_DATE}\"}")
BODY=$(cat /tmp/task_body.json)
info "HTTP $HTTP_CODE"

if [[ "$HTTP_CODE" == "400" ]]; then
  CODE=$(echo "$BODY" | jq -r '.code // empty' 2>/dev/null || echo "")
  if [[ "$CODE" == "validation.failed" ]] && echo "$BODY" | grep -qi "priority"; then
    fail "BUG: API does not accept string enum \"High\" for priority."
    fail "Fix: Add JsonStringEnumConverter in Program.cs AddControllers().AddJsonOptions()"
    info "Body: $BODY"; R_CREATE_TASK="FAIL"; exit 1
  fi
  fail "Create task 400 — Body: $BODY"; R_CREATE_TASK="FAIL"; exit 1
elif [[ "$HTTP_CODE" != "200" ]]; then
  fail "Unexpected HTTP $HTTP_CODE"; info "Body: $BODY"; R_CREATE_TASK="FAIL"; exit 1
fi

TASK_ID=$(echo "$BODY" | jq -r '.id // empty')
TASK_STATUS=$(echo "$BODY"   | jq -r '.status // empty')
TASK_PRIORITY=$(echo "$BODY" | jq -r '.priority // empty')
if [[ -z "$TASK_ID" ]]; then
  fail "No id in task response"; R_CREATE_TASK="FAIL"; exit 1
fi
pass "taskId = $TASK_ID"
[[ "$TASK_STATUS"   == "Todo" ]] && pass "status = Todo"   || warn "status: $TASK_STATUS"
[[ "$TASK_PRIORITY" == "High" ]] && pass "priority = High" || warn "priority: $TASK_PRIORITY"
R_CREATE_TASK="PASS"

# ============================================================
# STEP 7 — Get tasks
# ============================================================
step "Step 7 — Get tasks"
if http_call "200" -X GET "${BASE_URL}/api/tasks" -H "$AUTH_HEADER"; then
  FOUND=$(echo "$BODY" | jq --arg id "$TASK_ID" '(.items // .) | [.[] | select(.id==$id)] | length')
  [[ "$FOUND" -ge 1 ]] && pass "Created task in list" || warn "Task not found in list"
  R_GET_TASKS="PASS"
else
  fail "Get tasks failed"; R_GET_TASKS="FAIL"
fi

# ============================================================
# STEP 8 — Update status → InProgress
# ============================================================
step "Step 8 — PATCH status → InProgress"
if http_call "200" \
    -X PATCH "${BASE_URL}/api/tasks/${TASK_ID}/status" \
    -H "$AUTH_HEADER" -H "Content-Type: application/json" \
    -d '{"status":"InProgress"}'; then
  S=$(echo "$BODY" | jq -r '.status // empty')
  [[ "$S" == "InProgress" ]] && pass "status = InProgress" || warn "status: $S"
  R_STATUS_INPROGRESS="PASS"
else
  fail "Status InProgress failed"; info "Body: $BODY"; R_STATUS_INPROGRESS="FAIL"
fi

# ============================================================
# STEP 9 — Update status → Done
# ============================================================
step "Step 9 — PATCH status → Done"
if http_call "200" \
    -X PATCH "${BASE_URL}/api/tasks/${TASK_ID}/status" \
    -H "$AUTH_HEADER" -H "Content-Type: application/json" \
    -d '{"status":"Done"}'; then
  S=$(echo "$BODY"  | jq -r '.status // empty')
  CA=$(echo "$BODY" | jq -r '.completedAt // empty')
  [[ "$S"  == "Done" ]] && pass "status = Done"        || warn "status: $S"
  [[ -n "$CA" ]]        && pass "completedAt = $CA"    || warn "completedAt is null"
  R_STATUS_DONE="PASS"
else
  fail "Status Done failed"; info "Body: $BODY"; R_STATUS_DONE="FAIL"
fi

# ============================================================
# STEP 10 — Create comment
# ============================================================
step "Step 10 — Create comment"
if http_call "200" \
    -X POST "${BASE_URL}/api/tasks/${TASK_ID}/comments" \
    -H "$AUTH_HEADER" -H "Content-Type: application/json" \
    -d '{"content":"Terminal smoke test comment."}'; then
  COMMENT_ID=$(echo "$BODY" | jq -r '.id // empty')
  [[ -n "$COMMENT_ID" ]] && pass "commentId = $COMMENT_ID" || warn "No id in comment response"
  R_CREATE_COMMENT="PASS"
else
  fail "Create comment failed"; info "Body: $BODY"; R_CREATE_COMMENT="FAIL"
fi

# ============================================================
# STEP 11 — Get comments
# ============================================================
step "Step 11 — Get comments"
if http_call "200" \
    -X GET "${BASE_URL}/api/tasks/${TASK_ID}/comments" \
    -H "$AUTH_HEADER"; then
  if [[ -n "$COMMENT_ID" ]]; then
    FOUND=$(echo "$BODY" | jq --arg id "$COMMENT_ID" '[.[] | select(.id==$id)] | length')
    [[ "$FOUND" -ge 1 ]] && pass "Comment found in list" || warn "Comment not in list"
  fi
  R_GET_COMMENTS="PASS"
else
  fail "Get comments failed"; R_GET_COMMENTS="FAIL"
fi

# ============================================================
# STEP 12 — Dashboard
# ============================================================
step "Step 12 — Dashboard summary"
if http_call "200" -X GET "${BASE_URL}/api/dashboard/summary" -H "$AUTH_HEADER"; then
  TP=$(echo "$BODY" | jq -r '.totalProjects // 0')
  TT=$(echo "$BODY" | jq -r '.totalTasks // 0')
  DT=$(echo "$BODY" | jq -r '.doneTasks // 0')
  [[ "$TP" -ge 1 ]] && pass "totalProjects = $TP"  || warn "totalProjects = $TP (expected ≥ 1)"
  [[ "$TT" -ge 1 ]] && pass "totalTasks = $TT"     || warn "totalTasks = $TT (expected ≥ 1)"
  [[ "$DT" -ge 1 ]] && pass "doneTasks = $DT"      || warn "doneTasks = $DT (expected ≥ 1)"
  R_DASHBOARD="PASS"
else
  fail "Dashboard failed"; R_DASHBOARD="FAIL"
fi

# ============================================================
# NEGATIVE A — Missing token → 401
# ============================================================
step "Negative A — Missing token (expect 401)"
HTTP_CODE=$(curl -s -o /tmp/neg_a.json -w "%{http_code}" \
  -X GET "${BASE_URL}/api/auth/me") || true
BODY=$(cat /tmp/neg_a.json)
info "HTTP $HTTP_CODE"
if [[ "$HTTP_CODE" == "401" ]]; then
  pass "Got 401"
  if [[ -z "$BODY" ]]; then
    warn "401 body is empty — JWT OnChallenge not returning ApiErrorResponse"
    R_NEG_A="WARN"
  else
    CD=$(echo "$BODY" | jq -r '.code // empty' 2>/dev/null || echo "")
    MS=$(echo "$BODY" | jq -r '.message // empty' 2>/dev/null || echo "")
    pass "Unified 401 body: code=$CD message=$MS"
    R_NEG_A="PASS"
  fi
else
  fail "Expected 401 — got $HTTP_CODE"; R_NEG_A="FAIL"
fi

# ============================================================
# NEGATIVE B — Wrong password → 401
# ============================================================
step "Negative B — Wrong password (expect 401)"
HTTP_CODE=$(curl -s -o /tmp/neg_b.json -w "%{http_code}" \
  -X POST "${BASE_URL}/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"${EMAIL}\",\"password\":\"WrongPassword123!\"}") || true
BODY=$(cat /tmp/neg_b.json)
info "HTTP $HTTP_CODE"
if [[ "$HTTP_CODE" == "401" ]]; then
  CODE=$(echo "$BODY" | jq -r '.code // empty' 2>/dev/null || echo "")
  pass "Got 401"
  [[ "$CODE" == "auth.invalid_credentials" ]] && pass "code = auth.invalid_credentials" || warn "code = $CODE"
  R_NEG_B="PASS"
else
  fail "Expected 401 — got $HTTP_CODE"; R_NEG_B="FAIL"
fi

# ============================================================
# NEGATIVE C — Duplicate email → 409
# ============================================================
step "Negative C — Duplicate register (expect 409)"
HTTP_CODE=$(curl -s -o /tmp/neg_c.json -w "%{http_code}" \
  -X POST "${BASE_URL}/api/auth/register" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"${EMAIL}\",\"password\":\"${PASSWORD}\",\"firstName\":\"Jane\",\"lastName\":\"Doe\"}") || true
BODY=$(cat /tmp/neg_c.json)
info "HTTP $HTTP_CODE"
if [[ "$HTTP_CODE" == "409" ]]; then
  CODE=$(echo "$BODY" | jq -r '.code // empty' 2>/dev/null || echo "")
  pass "Got 409"
  [[ "$CODE" == "auth.email_exists" ]] && pass "code = auth.email_exists" || warn "code = $CODE"
  R_NEG_C="PASS"
else
  fail "Expected 409 — got $HTTP_CODE"; R_NEG_C="FAIL"
fi

# ============================================================
# NEGATIVE D — Whitespace project name → 400
# ============================================================
step "Negative D — Whitespace project name (expect 400)"
HTTP_CODE=$(curl -s -o /tmp/neg_d.json -w "%{http_code}" \
  -X POST "${BASE_URL}/api/projects" \
  -H "$AUTH_HEADER" -H "Content-Type: application/json" \
  -d '{"name":"   ","description":"Invalid project"}') || true
BODY=$(cat /tmp/neg_d.json)
info "HTTP $HTTP_CODE"
if [[ "$HTTP_CODE" == "400" ]]; then
  CODE=$(echo "$BODY" | jq -r '.code // empty' 2>/dev/null || echo "")
  pass "Got 400"
  info "code = $CODE  (validation.failed or project.invalid_name both accepted)"
  [[ "$CODE" == "validation.failed" || "$CODE" == "project.invalid_name" ]] \
    && pass "code is expected" || warn "code = $CODE"
  R_NEG_D="PASS"
else
  fail "Expected 400 — got $HTTP_CODE"; R_NEG_D="FAIL"
fi

# ============================================================
# NEGATIVE E — Empty task title → 400
# ============================================================
step "Negative E — Empty task title (expect 400)"
HTTP_CODE=$(curl -s -o /tmp/neg_e.json -w "%{http_code}" \
  -X POST "${BASE_URL}/api/tasks" \
  -H "$AUTH_HEADER" -H "Content-Type: application/json" \
  -d "{\"projectId\":\"${PROJECT_ID}\",\"title\":\"\",\"description\":\"Invalid\",\"priority\":\"High\",\"dueDate\":\"${FUTURE_DATE}\"}") || true
BODY=$(cat /tmp/neg_e.json)
info "HTTP $HTTP_CODE"
if [[ "$HTTP_CODE" == "200" ]]; then
  fail "CRITICAL BUG: empty task title accepted with 200 OK."
  fail "Fix: repair RequestValidationService — FluentValidation pipeline is not firing."
  R_NEG_E="FAIL"; exit 1
elif [[ "$HTTP_CODE" == "400" ]]; then
  CODE=$(echo "$BODY" | jq -r '.code // empty' 2>/dev/null || echo "")
  pass "Got 400"
  [[ "$CODE" == "validation.failed" ]] && pass "code = validation.failed" || warn "code = $CODE"
  R_NEG_E="PASS"
else
  fail "Expected 400 — got $HTTP_CODE"; R_NEG_E="FAIL"
fi

# ============================================================
# NEGATIVE F — Past due date → 400
# ============================================================
step "Negative F — Past due date (expect 400)"
HTTP_CODE=$(curl -s -o /tmp/neg_f.json -w "%{http_code}" \
  -X POST "${BASE_URL}/api/tasks" \
  -H "$AUTH_HEADER" -H "Content-Type: application/json" \
  -d "{\"projectId\":\"${PROJECT_ID}\",\"title\":\"Past due task\",\"priority\":\"High\",\"dueDate\":\"2020-01-01T00:00:00Z\"}") || true
BODY=$(cat /tmp/neg_f.json)
info "HTTP $HTTP_CODE"
if [[ "$HTTP_CODE" == "400" ]]; then
  CODE=$(echo "$BODY" | jq -r '.code // empty' 2>/dev/null || echo "")
  pass "Got 400"
  info "code = $CODE  (validation.failed or task.invalid_due_date both accepted)"
  [[ "$CODE" == "validation.failed" || "$CODE" == "task.invalid_due_date" ]] \
    && pass "code is expected" || warn "code = $CODE"
  R_NEG_F="PASS"
else
  fail "Expected 400 — got $HTTP_CODE"; R_NEG_F="FAIL"
fi

# ============================================================
# NEGATIVE G — Todo→Done skip → 409
# ============================================================
step "Negative G — Invalid status transition Todo→Done (expect 409)"
HTTP_CODE=$(curl -s -o /tmp/neg_g_t.json -w "%{http_code}" \
  -X POST "${BASE_URL}/api/tasks" \
  -H "$AUTH_HEADER" -H "Content-Type: application/json" \
  -d "{\"projectId\":\"${PROJECT_ID}\",\"title\":\"Transition test\",\"priority\":\"Low\",\"dueDate\":null}") || true
NEW_TASK_BODY=$(cat /tmp/neg_g_t.json)
NEW_TASK_ID=""
if [[ "$HTTP_CODE" == "200" ]]; then
  NEW_TASK_ID=$(echo "$NEW_TASK_BODY" | jq -r '.id // empty')
  info "Fresh task: $NEW_TASK_ID"
fi
if [[ -n "$NEW_TASK_ID" ]]; then
  HTTP_CODE=$(curl -s -o /tmp/neg_g.json -w "%{http_code}" \
    -X PATCH "${BASE_URL}/api/tasks/${NEW_TASK_ID}/status" \
    -H "$AUTH_HEADER" -H "Content-Type: application/json" \
    -d '{"status":"Done"}') || true
  BODY=$(cat /tmp/neg_g.json)
  info "HTTP $HTTP_CODE"
  if [[ "$HTTP_CODE" == "409" ]]; then
    CODE=$(echo "$BODY" | jq -r '.code // empty' 2>/dev/null || echo "")
    pass "Got 409"
    [[ "$CODE" == "task.invalid_status_transition" ]] \
      && pass "code = task.invalid_status_transition" || warn "code = $CODE"
    R_NEG_G="PASS"
  else
    fail "Expected 409 — got $HTTP_CODE"; R_NEG_G="FAIL"
  fi
else
  warn "Could not create task for Negative G — skipped"
  R_NEG_G="WARN"
fi

# ============================================================
# NEGATIVE H — Wrong task id → 404
# ============================================================
step "Negative H — Wrong task id (expect 404)"
HTTP_CODE=$(curl -s -o /tmp/neg_h.json -w "%{http_code}" \
  -X PATCH "${BASE_URL}/api/tasks/3fa85f64-5717-4562-b3fc-2c963f66afa6/status" \
  -H "$AUTH_HEADER" -H "Content-Type: application/json" \
  -d '{"status":"InProgress"}') || true
BODY=$(cat /tmp/neg_h.json)
info "HTTP $HTTP_CODE"
if [[ "$HTTP_CODE" == "404" ]]; then
  CODE=$(echo "$BODY" | jq -r '.code // empty' 2>/dev/null || echo "")
  pass "Got 404"
  if [[ "$CODE" == "task.not_found" ]]; then
    pass "code = task.not_found"
    R_NEG_H="PASS"
  elif [[ -n "$CODE" ]]; then
    warn "code = $CODE (expected task.not_found)"
    R_NEG_H="WARN"
  else
    warn "404 received but response body has no code field"
    R_NEG_H="WARN"
  fi
else
  fail "Expected 404 — got $HTTP_CODE  Body: $BODY"
  R_NEG_H="FAIL"
fi

# ============================================================
# SUMMARY TABLE
# ============================================================
OVERALL="PASS"

print_row() {
  local label="$1" result="$2"
  local colour
  case "$result" in
    PASS)
      colour=$GREEN
      PASSED=$((PASSED+1))
      ;;
    WARN)
      colour=$YELLOW
      WARNINGS=$((WARNINGS+1))
      [[ "$OVERALL" != "FAIL" ]] && OVERALL="WARN"
      ;;
    FAIL)
      colour=$RED
      FAILED=$((FAILED+1))
      OVERALL="FAIL"
      ;;
    *)
      colour=$YELLOW
      result="SKIP"
      ;;
  esac
  printf "  %-42s %b%s%b\n" "$label" "$colour" "$result" "$RESET"
}

echo ""
echo -e "${BOLD}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}"
echo -e "${BOLD}  TaskFlow API — terminal smoke test summary${RESET}"
echo -e "${BOLD}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}"

print_row "Swagger JSON"                   "$R_SWAGGER"
print_row "Register"                       "$R_REGISTER"
print_row "Login"                          "$R_LOGIN"
print_row "Me"                             "$R_ME"
print_row "Create project"                 "$R_CREATE_PROJECT"
print_row "Get projects"                   "$R_GET_PROJECTS"
print_row "Create task (string enum)"      "$R_CREATE_TASK"
print_row "Get tasks"                      "$R_GET_TASKS"
print_row "Update status InProgress"       "$R_STATUS_INPROGRESS"
print_row "Update status Done"             "$R_STATUS_DONE"
print_row "Create comment"                 "$R_CREATE_COMMENT"
print_row "Get comments"                   "$R_GET_COMMENTS"
print_row "Dashboard"                      "$R_DASHBOARD"
print_row "Missing token 401"              "$R_NEG_A"
print_row "Invalid login"                  "$R_NEG_B"
print_row "Duplicate register"             "$R_NEG_C"
print_row "Invalid project name"           "$R_NEG_D"
print_row "Empty task title"               "$R_NEG_E"
print_row "Past due date"                  "$R_NEG_F"
print_row "Invalid transition"             "$R_NEG_G"
print_row "Wrong task id"                 "$R_NEG_H"

echo -e "${BOLD}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}"
printf "  Passed: %d   Failed: %d   Warnings: %d\n" "$PASSED" "$FAILED" "$WARNINGS"
case "$OVERALL" in
  PASS) echo -e "  ${GREEN}${BOLD}Final result: PASS${RESET}" ;;
  WARN) echo -e "  ${YELLOW}${BOLD}Final result: PASS (with warnings)${RESET}" ;;
  FAIL) echo -e "  ${RED}${BOLD}Final result: FAIL${RESET}" ;;
esac
echo ""

[[ "$FAILED" -gt 0 ]] && exit 1
exit 0
