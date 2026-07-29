// カードライブラリの枠プール補正パッチは撤去（2026-07-29）。
//
// 経緯: afea14a でプール返却時の (0,0) 浮きカード対策として導入したが、本家 NCardGrid の
// スライディングウィンドウ方式（表示中の行だけ実体を持ち、スクロール時は AllocateCardHolders /
// ReallocateAbove・Below が既存ホルダーを使い回す）と前提が食い違っており、以後 5 回の修正
// （34617de, d89395e, 97d481d ほか）でも再発を繰り返した。
//
// 本家 NGridCardHolder.OnReturnedFromPool は Position=zero・Visible=true・Modulate=白に
// 正しくリセットしており、NCardGrid.InitGrid も生成直後に Visible=true と UpdateGridPositions
// を明示している。本パッチの HideOrphans／CardLibraryPoolReturnPatch はこれらと競合し、
// スクロール中の再割り当てと重なると正規の枠まで誤って隠す・座標がリセット前で固まるなど、
// より重い症状（アイアンクラッドのスクロール崩れ、サイレントの表示消失）を引き起こしていた。
//
// 一旦すべて撤去し、本家の挙動に委ねる。ライブラリの説明改変は CardLibraryUiGuard で抑止（2026-07-29）。
