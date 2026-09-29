# -*- coding: utf-8 -*-
"""ゲーム実機ログの健康診断（ゲーム起動後に実行）。

ビルドが通っても、Harmony パッチは起動時に束縛できず未適用になることがある（引数名の不一致、
オープンジェネリックなど）。その結果は godot.log の次の行にしか出ない:
  [HypnosisCreator] Harmony patches applied: N classes ok, M failed
  [HypnosisCreator] Harmony patch failed for <クラス名>: <理由>

使い方:  python tools_check_game_log.py [ログパス]   （省略時は %APPDATA%\\SlayTheSpire2\\logs\\godot.log）
失敗が1件でもあれば終了コード 1。
"""
import io
import os
import re
import sys


def default_log():
    appdata = os.environ.get("APPDATA", "")
    return os.path.join(appdata, "SlayTheSpire2", "logs", "godot.log")


def main():
    path = sys.argv[1] if len(sys.argv) > 1 else default_log()
    if not os.path.exists(path):
        print(f"ログが見つかりません: {path}")
        return 2

    with io.open(path, encoding="utf-8", errors="replace") as f:
        lines = f.read().splitlines()

    summary = None
    failures = []
    pending = []
    for line in lines:
        if "[HypnosisCreator] Harmony patch failed for" in line:
            pending.append(line)
        elif "[HypnosisCreator] Harmony patches applied:" in line:
            # 失敗行は集計行より前に出る。最後の起動ぶんだけ見る
            summary, failures, pending = line, pending, []

    if summary is None:
        print("Harmony 適用の記録がありません（mod が読み込まれていない可能性）")
        return 2

    print(summary.strip())
    for line in failures:
        print("  " + line.strip()[:240])

    match = re.search(r"(\d+) failed", summary)
    failed = int(match.group(1)) if match else len(failures)
    print("OK" if failed == 0 else f"NG: {failed} 件のパッチが未適用")
    return 0 if failed == 0 else 1


if __name__ == "__main__":
    sys.exit(main())
