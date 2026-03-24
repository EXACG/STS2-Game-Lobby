#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SOURCE_DIR="$ROOT_DIR/lobby-service"
RELEASE_DIR="$SOURCE_DIR/release"
PACKAGE_NAME="sts2_lobby_service"
PACKAGE_ROOT="$RELEASE_DIR/$PACKAGE_NAME"
INSTALLER="$ROOT_DIR/scripts/install-lobby-service-linux.sh"

require_file() {
  local path="$1"
  [[ -f "$path" ]] || {
    echo "Expected file is missing from service package: $path" >&2
    exit 1
  }
}

verify_package_manifest() {
  local package_dir="$1"

  require_file "$package_dir/README.md"
  require_file "$package_dir/install-lobby-service-linux.sh"
  require_file "$package_dir/lobby-service/Dockerfile"
  require_file "$package_dir/lobby-service/.dockerignore"
  require_file "$package_dir/lobby-service/package.json"
  require_file "$package_dir/lobby-service/package-lock.json"
  require_file "$package_dir/lobby-service/tsconfig.json"
  require_file "$package_dir/lobby-service/.env.example"
  require_file "$package_dir/lobby-service/scripts/generate-server-admin-password-hash.mjs"
  require_file "$package_dir/lobby-service/src/server.ts"
  require_file "$package_dir/lobby-service/src/server-admin-state.ts"
  require_file "$package_dir/lobby-service/src/server-admin-ui.ts"
  require_file "$package_dir/lobby-service/deploy/.env.example"
  require_file "$package_dir/lobby-service/deploy/docker-compose.lobby-service.yml"
  require_file "$package_dir/lobby-service/deploy/lobby-service.docker.env.example"
}

verify_zip_manifest() {
  local zip_path="$1"
  local zip_listing
  zip_listing="$(zipinfo -1 "$zip_path")"

  [[ "$zip_listing" == *"$PACKAGE_NAME/install-lobby-service-linux.sh"* ]] || {
    echo "Service zip is missing install-lobby-service-linux.sh" >&2
    exit 1
  }
  [[ "$zip_listing" == *"$PACKAGE_NAME/lobby-service/Dockerfile"* ]] || {
    echo "Service zip is missing Dockerfile" >&2
    exit 1
  }
  [[ "$zip_listing" == *"$PACKAGE_NAME/lobby-service/.env.example"* ]] || {
    echo "Service zip is missing .env.example" >&2
    exit 1
  }
  [[ "$zip_listing" == *"$PACKAGE_NAME/lobby-service/scripts/generate-server-admin-password-hash.mjs"* ]] || {
    echo "Service zip is missing generate-server-admin-password-hash.mjs" >&2
    exit 1
  }
  [[ "$zip_listing" == *"$PACKAGE_NAME/lobby-service/src/server-admin-state.ts"* ]] || {
    echo "Service zip is missing server-admin-state.ts" >&2
    exit 1
  }
}

[[ -f "$SOURCE_DIR/package.json" ]] || {
  echo "lobby-service/package.json not found" >&2
  exit 1
}

rm -rf "$PACKAGE_ROOT"
mkdir -p "$PACKAGE_ROOT/lobby-service"

cp -R "$SOURCE_DIR/src" "$PACKAGE_ROOT/lobby-service/"
cp "$SOURCE_DIR/package.json" "$PACKAGE_ROOT/lobby-service/"
cp "$SOURCE_DIR/package-lock.json" "$PACKAGE_ROOT/lobby-service/"
cp "$SOURCE_DIR/tsconfig.json" "$PACKAGE_ROOT/lobby-service/"
cp "$SOURCE_DIR/.env.example" "$PACKAGE_ROOT/lobby-service/"
cp "$SOURCE_DIR/Dockerfile" "$PACKAGE_ROOT/lobby-service/"
cp "$SOURCE_DIR/.dockerignore" "$PACKAGE_ROOT/lobby-service/"
cp -R "$SOURCE_DIR/scripts" "$PACKAGE_ROOT/lobby-service/"
cp -R "$SOURCE_DIR/deploy" "$PACKAGE_ROOT/lobby-service/"
cp "$SOURCE_DIR/README.md" "$PACKAGE_ROOT/README.md"
cp "$INSTALLER" "$PACKAGE_ROOT/"
chmod +x "$PACKAGE_ROOT/install-lobby-service-linux.sh"
verify_package_manifest "$PACKAGE_ROOT"

cd "$RELEASE_DIR"
rm -f "${PACKAGE_NAME}.zip"
zip -qr "${PACKAGE_NAME}.zip" "$PACKAGE_NAME"
verify_zip_manifest "${PACKAGE_NAME}.zip"
echo "Package created at: $RELEASE_DIR/${PACKAGE_NAME}.zip"
