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

def find_openssl():
    from shutil import which
    found = which('openssl')
    if found:
        return found
    # Fall back to the openssl bundled with Git for Windows (not on PATH by default)
    for p in (r'C:\Program Files\Git\usr\bin\openssl.exe',
              r'C:\Program Files\Git\mingw64\bin\openssl.exe',
              r'C:\Program Files (x86)\Git\usr\bin\openssl.exe'):
        if os.path.exists(p):
            return p
    return 'openssl'

try:
    subprocess.run(
        [find_openssl(), 'dgst', '-sha256', '-sign', kf_path, '-out', sf_path],
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
