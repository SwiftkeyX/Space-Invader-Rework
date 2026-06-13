#!/bin/bash
set -e

# Two levels up from .claude/scripts/ → project root
SCRIPT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"

APP_ID=$(grep GITHUB_APP_ID "$SCRIPT_DIR/.env" | cut -d= -f2)
INSTALLATION_ID=$(grep GITHUB_APP_INSTALLATION_ID "$SCRIPT_DIR/.env" | cut -d= -f2)
KEY_PATH=$(grep GITHUB_APP_PRIVATE_KEY_PATH "$SCRIPT_DIR/.env" | cut -d= -f2-)

b64url() { openssl base64 -e | tr -d '=' | tr '+' '-' | tr '/' '_' | tr -d '\n'; }

now=$(date +%s)
header=$(echo -n '{"alg":"RS256","typ":"JWT"}' | b64url)
payload=$(echo -n "{\"iat\":$((now-60)),\"exp\":$((now+600)),\"iss\":\"$APP_ID\"}" | b64url)
sig=$(echo -n "$header.$payload" | openssl dgst -sha256 -sign "$KEY_PATH" | b64url)
jwt="$header.$payload.$sig"

response=$(curl -s -X POST \
    -H "Authorization: Bearer $jwt" \
    -H "Accept: application/vnd.github+json" \
    -H "X-GitHub-Api-Version: 2022-11-28" \
    "https://api.github.com/app/installations/$INSTALLATION_ID/access_tokens")

token=$(echo "$response" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
expires=$(echo "$response" | grep -o '"expires_at":"[^"]*"' | cut -d'"' -f4)

if [ -z "$token" ]; then echo "FAILED: $response"; exit 1; fi

echo "$token" > "$SCRIPT_DIR/.gh-token"
echo "Done. Expires: $expires"
echo "Preview: ${token:0:15}..."
