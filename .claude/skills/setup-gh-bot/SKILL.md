One-time setup: registers a GitHub App as a bot contributor so Claude can post PR comments under a separate bot identity instead of the user's personal account. Idempotent — safe to re-run; subsequent runs just refresh the token.

---

## Agent

`claude`

---

## Docs

| Doc | Read/Write | Purpose |
|---|---|---|
| `.env` | Read + Write | App ID, Installation ID, private key path |
| `.gitignore` | Read + Write | Ensure secrets are never committed |
| `.claude/scripts/refresh-gh-token.py` | Write | Python script that generates the installation token |
| `.gh-token` | Write (via script) | Short-lived token read by `gh` calls; gitignored |

---

## Steps

**Step 1 — Check if already set up**

Check whether `.env` and `.claude/scripts/refresh-gh-token.py` both exist.
- If both exist → skip to Step 4.
- If either is missing → continue to Step 2.

---

**Step 2 — Gather credentials**

Ask the user in one message:

> "Provide three values from your GitHub App settings:
>
> 1. **App ID** — GitHub → Settings → Developer settings → GitHub Apps → your app → General → App ID
> 2. **Installation ID** — run `gh api /app/installations` and read the `id` field, or find it in the installation URL
> 3. **Private key path** — full path to the `.pem` file you downloaded from GitHub App → Private keys (e.g. `C:\Users\you\keys\my-app.pem`)"

Wait for the user's response before continuing.

---

**Step 3 — Write config files**

**Step 3a — Gitignore**

Read `.gitignore`. Add any of the following lines that are not already present:
```
.env
*.pem
.gh-token
```

**Step 3b — .env**

Write `.env` at the project root:
```
GITHUB_APP_ID=<value from user>
GITHUB_APP_INSTALLATION_ID=<value from user>
GITHUB_APP_PRIVATE_KEY_PATH=<value from user>
```

**Step 3c — refresh-gh-token.py**

Write `.claude/scripts/refresh-gh-token.py` with this exact content (stdlib + system `openssl` only — no third-party packages):

```python
import os, time, json, base64, urllib.request, urllib.error, subprocess, tempfile

def b64url(data):
    return base64.urlsafe_b64encode(data).rstrip(b'=').decode()

def load_env(path):
    env = {}
    with open(path) as f:
        for line in f:
            line = line.strip()
            if '=' in line and not line.startswith('#'):
                k, v = line.split('=', 1)
                env[k.strip()] = v.strip()
    return env

# Two levels up from .claude/scripts/ → project root
project_root = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
env = load_env(os.path.join(project_root, '.env'))

app_id          = env['GITHUB_APP_ID']
installation_id = env['GITHUB_APP_INSTALLATION_ID']
key_path        = env['GITHUB_APP_PRIVATE_KEY_PATH']

with open(key_path, 'rb') as f:
    private_key_pem = f.read()

now = int(time.time())
header  = b64url(json.dumps({"alg":"RS256","typ":"JWT"}).encode())
payload = b64url(json.dumps({"iat": now - 60, "exp": now + 600, "iss": app_id}).encode())
signing_input = f"{header}.{payload}".encode()

with tempfile.NamedTemporaryFile(delete=False, suffix='.pem') as kf:
    kf.write(private_key_pem)
    kf_path = kf.name

with tempfile.NamedTemporaryFile(delete=False) as sf:
    sf_path = sf.name

try:
    subprocess.run(
        ['openssl', 'dgst', '-sha256', '-sign', kf_path, '-out', sf_path],
        input=signing_input, check=True, capture_output=True
    )
    with open(sf_path, 'rb') as sf:
        sig = b64url(sf.read())
finally:
    os.unlink(kf_path)
    os.unlink(sf_path)

jwt = f"{header}.{payload}.{sig}"

req = urllib.request.Request(
    f"https://api.github.com/app/installations/{installation_id}/access_tokens",
    method='POST',
    data=b'{}',
    headers={
        'Authorization': f'Bearer {jwt}',
        'Accept': 'application/vnd.github+json',
        'Content-Type': 'application/json',
        'X-GitHub-Api-Version': '2022-11-28',
    }
)

try:
    with urllib.request.urlopen(req) as resp:
        data = json.loads(resp.read())
except urllib.error.HTTPError as e:
    print(f"HTTP {e.code}: {e.read().decode()}")
    exit(1)

token   = data['token']
expires = data['expires_at']

with open(os.path.join(project_root, '.gh-token'), 'w') as f:
    f.write(token)

print(f"Done. Expires: {expires}")
print(f"Preview: {token[:15]}...")
```

---

**Step 4 — Generate token**

Run:
```
python .claude/scripts/refresh-gh-token.py
```

- On success: script prints `Done. Expires: ...` and writes `.gh-token` at the project root.
- On failure: report the error to the user and stop — do not continue to Step 5.

---

**Step 5 — Verify bot identity**

Find the current open PR number:
```
gh pr list --state open
```

If an open PR exists, post a test comment using the bot token:
```
GH_TOKEN=$(cat .gh-token) gh pr comment <number> --body "GitHub App bot identity confirmed — /setup-gh-bot complete."
```

Confirm a comment URL is returned. Tell the user to check GitHub and verify the comment shows the bot name, not their personal account.

If no open PR exists, skip the comment and tell the user: "No open PR found. Token is ready — verify bot identity next time you run `/reconcile-gdd`."

---

## Exit Condition

`.gh-token` exists at the project root and is non-empty. Test comment posted (or skipped with notice if no open PR).

---

## Constraints

- Never commit `.env`, `*.pem`, or `.gh-token` — these are secrets
- Never use PowerShell for token generation — use `python .claude/scripts/refresh-gh-token.py`
- Never install third-party packages — the script uses Python stdlib + system `openssl` only
- On re-runs, skip Steps 2–3 and go straight to Step 4 — do not overwrite existing `.env`
- `.claude/scripts/refresh-gh-token.py` must be re-run every ~1 hour when the token expires
