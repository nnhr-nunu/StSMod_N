# -*- coding: utf-8 -*-
"""ローカライズ整合チェック（publish 前に実行）。

eng を基準に、他言語の
  - キー欠落 / 余剰キー
  - プレースホルダ名の不一致（例: {Damage} と {Block}）… 本家 SmartFormat が失敗し、生の {…} 表示＋Sentry 送信になる
  - BBCode タグの閉じ忘れ
を報告する。キー欠落は英語フォールバックで表示されるだけなので警告、
プレースホルダ不一致・タグ崩れは実害があるのでエラー（終了コード 1）。

使い方:  python tools_check_localization.py [--strict]   （--strict でキー欠落もエラー扱い）
"""
import io
import json
import os
import re
import sys

ROOT = os.path.dirname(os.path.abspath(__file__))
LOC = os.path.join(ROOT, "HypnosisCreator", "localization")
PLACEHOLDER = re.compile(r"\{([A-Za-z_][A-Za-z0-9_]*)")
TAG = re.compile(r"\[(/?)(\w+)(?:=[^\]]*)?\]")


def load(lang, name):
    path = os.path.join(LOC, lang, name)
    if not os.path.exists(path):
        return None
    with io.open(path, encoding="utf-8-sig") as f:
        return json.load(f)


def tags_balanced(text):
    stack = []
    for m in TAG.finditer(text):
        if not m.group(1):
            stack.append(m.group(2))
        elif stack and stack[-1] == m.group(2):
            stack.pop()
        else:
            return False
    return not stack


def main():
    strict = "--strict" in sys.argv
    errors = warnings = 0
    langs = sorted(d for d in os.listdir(LOC) if os.path.isdir(os.path.join(LOC, d)) and d != "eng")
    for name in sorted(f for f in os.listdir(os.path.join(LOC, "eng")) if f.endswith(".json")):
        base = load("eng", name)
        for lang in langs:
            data = load(lang, name)
            if data is None:
                print(f"[WARN] {lang}/{name}: ファイルなし（英語フォールバック）")
                warnings += 1
                continue
            missing = sorted(set(base) - set(data))
            extra = sorted(set(data) - set(base))
            if missing:
                print(f"[{'ERROR' if strict else 'WARN'}] {lang}/{name}: 欠落 {len(missing)} 件 例: {missing[:3]}")
                errors += strict
                warnings += not strict
            if extra:
                print(f"[WARN] {lang}/{name}: eng に無い余剰キー {len(extra)} 件 例: {extra[:3]}")
                warnings += 1
            for key, en in base.items():
                text = data.get(key)
                if not isinstance(en, str) or not isinstance(text, str):
                    continue
                if set(PLACEHOLDER.findall(en)) != set(PLACEHOLDER.findall(text)):
                    print(f"[ERROR] {lang}/{name}: プレースホルダ不一致 {key}")
                    errors += 1
                if not tags_balanced(text):
                    print(f"[ERROR] {lang}/{name}: BBCode タグ崩れ {key}")
                    errors += 1
    print(f"完了: エラー {errors} / 警告 {warnings}")
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main())
