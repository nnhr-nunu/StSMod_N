using BaseLib.Abstracts;
using HypnosisCreator.HypnosisCreatorCode.Extensions;
using Godot;

namespace HypnosisCreator.HypnosisCreatorCode.Character;

public class HypnosisCreatorCardPool : CustomCardPoolModel
{
    public override string Title => HypnosisCreator.CharacterId; //This is not a display name.
    
    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();


    /* These HSV values will determine the color of your card back.
    They are applied as a shader onto an already colored image,
    so it may take some experimentation to find a color you like.
    Generally they should be values between 0 and 1. */
    // マゼンタ寄り（催眠制作者）
    public override float H => 0.92f;
    public override float S => 0.55f;
    public override float V => 0.95f;
    
    //Alternatively, leave these values at 1 and provide a custom frame image.
    /*public override Texture2D CustomFrame(CustomCardModel card)
    {
        //This will attempt to load HypnosisCreator/images/cards/frame.png
        return PreloadManager.Cache.GetTexture2D("cards/frame.png".ImagePath());
    }*/

    //Color of small card icons
    public override Color DeckEntryCardColor => new("ffffff");
    
    public override bool IsColorless => false;
}