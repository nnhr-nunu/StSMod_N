#!/usr/bin/env bash
# Cloud Agent 用 開発環境セットアップ（べき等・非対話）
#
# このスクリプトが用意するもの:
#   - .NET 9 SDK（C# コード編集・アナライザ・NuGet 復元用）
#   - Pillow（tools_*.py の画像生成ツール用）
#   - NuGet パッケージのウォームアップ（BaseLib 等）
#
# 注意: 実際の `dotnet build` / `dotnet publish` と mod の起動には
#       製品版 Slay the Spire 2 の sts2.dll / 0Harmony.dll（ローカル導入）と
#       MegaDot/Godot 4.5.1 が必要。これらはヘッドレスなクラウド VM では
#       用意できないため、ここでは復元までを検証対象とする。
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOTNET_DIR="$HOME/.dotnet"
DOTNET_CHANNEL="9.0"

echo "[install] リポジトリ: $REPO_ROOT"

# 1) .NET 9 SDK（未導入時のみ導入）
if [ ! -x "$DOTNET_DIR/dotnet" ]; then
  echo "[install] .NET ${DOTNET_CHANNEL} SDK を導入します"
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  bash /tmp/dotnet-install.sh --channel "$DOTNET_CHANNEL" --install-dir "$DOTNET_DIR"
else
  echo "[install] .NET SDK は導入済み: $("$DOTNET_DIR/dotnet" --version)"
fi

export DOTNET_ROOT="$DOTNET_DIR"
export PATH="$DOTNET_DIR:$PATH"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

# 2) 対話シェル用の PATH / 環境変数を bashrc に追記（マーカーでべき等）
MARKER_BEGIN="# >>> hypnosiscreator dotnet >>>"
if ! grep -qF "$MARKER_BEGIN" "$HOME/.bashrc" 2>/dev/null; then
  {
    echo "$MARKER_BEGIN"
    echo 'export DOTNET_ROOT="$HOME/.dotnet"'
    echo 'export PATH="$HOME/.dotnet:$PATH"'
    echo 'export DOTNET_CLI_TELEMETRY_OPTOUT=1'
    echo 'export DOTNET_NOLOGO=1'
    echo "# <<< hypnosiscreator dotnet <<<"
  } >> "$HOME/.bashrc"
  echo "[install] ~/.bashrc に PATH を追記しました"
fi

# 3) Python 画像ツール用 Pillow（tools_*.py 用・べき等）
if python3 -m pip install --user --break-system-packages --upgrade Pillow >/dev/null 2>&1; then
  echo "[install] Pillow 導入 OK"
else
  echo "[install] Pillow の導入をスキップ（pip 制約）"
fi

# 4) NuGet パッケージのウォームアップ
#    sts2.dll が無くても NuGet 復元だけは可能（プレースホルダのデータディレクトリを渡す）。
#    実ビルドには本編の sts2.dll / 0Harmony.dll が必要。
PLACEHOLDER="$HOME/.sts2-refs-placeholder"
mkdir -p "$PLACEHOLDER"
if "$DOTNET_DIR/dotnet" restore "$REPO_ROOT/HypnosisCreator.csproj" \
     -p:Sts2DataDir="$PLACEHOLDER" >/dev/null 2>&1; then
  echo "[install] NuGet 復元 OK（BaseLib 等をキャッシュ）"
else
  echo "[install] NuGet 復元は後で実施（ネットワーク要）"
fi

echo "[install] 完了"
