using HypnosisCreator.HypnosisCreatorCode.Utils;

static int Fail(string message)
{
    Console.Error.WriteLine("FAIL: " + message);
    return 1;
}

// 付与確認: 戦闘フックが 1 を返しても矢印は素値→エンチャント後
var (enchanted, preview) = EnchantPreviewPair.Resolve(15m, 24m, 1m, isEnchantmentPreview: true);
if (enchanted != 15m || preview != 24m)
    return Fail($"enchant UI: enchanted={enchanted} preview={preview}");

// 戦闘中・弱体: エンチャント後 24 より低い 18 を隠さない（旧 Math.Max だと 24 になる）
(enchanted, preview) = EnchantPreviewPair.Resolve(15m, 24m, 18m, isEnchantmentPreview: false);
if (enchanted != 24m || preview != 18m)
    return Fail($"weak: enchanted={enchanted} preview={preview}");

// 未エンチャント・弱体: 素値より低いプレビューを隠さない
(enchanted, preview) = EnchantPreviewPair.Resolve(15m, 15m, 11m, isEnchantmentPreview: false);
if (enchanted != 15m || preview != 11m)
    return Fail($"unenchanted weak: enchanted={enchanted} preview={preview}");

// スロウで 15×1.25=18.75 のあと弱体1.5。途中切り捨てだと 18×1.5=27、本家は 28.125→表示28
var scaled = CombatPreviewMath.ScaleByUpcomingVulnerable(18.75m, 1m, 1.5m);
if (scaled != 28.125m)
    return Fail($"vuln scale: {scaled}");

Console.WriteLine("EnchantPreviewPairCheck ok");
return 0;
