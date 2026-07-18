# Hypno Creator — エージェント知見（正本）

人間が承認した知見のみ追記する。ルール再分割は確認後。

## 完了前の必須事項

- コード変更ありの完了報告・commit / push の前に **C2 セルフレビュー必須**（水平展開・再発防止を含む）
- 詳細手順は `.cursor/rules/composer-self-review.mdc`
- 完了チェックリスト・返信形式は `.cursor/rules/hypnosis-creator.mdc`
- 知見の提案フローは `.cursor/rules/skills_collector.mdc`（**承認前に本ファイルを書き換えない**）

## 正本の役割分担

| ファイル | 役割 |
| -------- | ---- |
| `AGENTS.md`（本ファイル） | エージェント運用・再発防止の知見 |
| `mechanics-lock.md` | 確定ゲームメカニクス（ユーザー指示時のみ変更） |
| `task.md` | 未完了タスクのみ |
| `README.md` | セットアップ・導線 |

## 水平展開（クイック参照）

| 変更の種類 | 調査例 |
| ---------- | ------ |
| カード | 同レアリティ・同キーワード他カード、eng/jpn キー |
| パワー／パッチ | `Powers/`・`Patches/` の同フック |
| 心臓レリック | `Relics/Hearts/`・`HeartInventory` |
| アセット | `dotnet publish` で `.pck` 更新（build だけではローカライズが載らない） |

## 蓄積知見

（承認後に追記）
