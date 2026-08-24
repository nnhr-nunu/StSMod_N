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

Console.WriteLine("EnchantPreviewPairCheck ok");
return 0;
