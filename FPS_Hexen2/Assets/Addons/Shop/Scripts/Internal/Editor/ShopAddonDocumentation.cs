using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor;

public class ShopAddonDocumentation : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/shop/";
    private NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.jpg", Image = null},
        new NetworkImages{Name = "img-4.jpg", Image = null},
        new NetworkImages{Name = "img-5.jpg", Image = null},
        new NetworkImages{Name = "img-6.jpg", Image = null},
        new NetworkImages{Name = "img-7.jpg", Image = null},
        new NetworkImages{Name = "img-8.jpg", Image = null},
    };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "Integrate", StepsLenght = 0 },
    new Steps { Name = "Add Weapon", StepsLenght = 0 },
    new Steps { Name = "Coins", StepsLenght = 0 },
    new Steps { Name = "Add Payment", StepsLenght = 0 },
    new Steps { Name = "Add Skin", StepsLenght = 0 },
    new Steps { Name = "Add Camo", StepsLenght = 0 },
    };
    //final required////////////////////////////////////////////////

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder);
        GUISkin gs = Resources.Load<GUISkin>("content/MFPSEditorSkin") as GUISkin;
        if (gs != null)
        {
            base.SetTextStyle(gs.customStyles[2]);
        }
    }

    public override void WindowArea(int window)
    {
        if (window == 0)
        {
            DrawIntegrate();
        }
        else if (window == 1)
        {
            DrawAddWeapon();
        }
        else if (window == 2)
        {
            DrawAddCoins();
        }
        else if (window == 3)
        {
            DrawAddPayment();
        }else if(window == 4) { DrawAddSkin(); }
        else if (window == 5) { DrawAddCamo(); }
    }

    void DrawIntegrate()
    {
        DrawText("<b>Require:</b>\nMFPS 1.5++\nULogin Pro 1.6++\n \nIn order to integrate, first make sure you have the addon enable:");
#if SHOP
        DrawText("<color=green>Addon is Enabled</color>");
#else
                DrawText("Addon is not enabled yet, for enable it click in the button bellow");
        if (DrawButton("ENABLE"))
        {
            EditorUtils.SetEnabled("SHOP", true);
        }
#endif
        DownArrow();
        DrawText("For integrate simply go to (Toolbar) MFPS -> Addons -> Shop -> Integrate");
        DownArrow();
        DrawText("Finally you have to upload a PHP script in the same folder where you have the ULogin Pro PHP scripts in your server, the script that you have to upload is located in:" +
            " <i>Assets -> Addons -> Shop -> Scripts -> Php -> <b>bl_Shop.php</b></i>, if you don't not how to upload, please check the ULogin Pro documentation.\n \nThat's all for integration :)");
    }

    void DrawAddWeapon()
    {
        DrawText("In order to add weapon to be listed in the shop you simple have to increase the price of this > 0 in GameData. \n \n" +
            "Go to MFPS (folder) -> Resources -> GameData -> AllWeapons -> *, now in each weapon that you wanna listed in the shop just set a price > 0 in the weapon info -> <b>Price</b> field.");
        DrawImage(GetServerImage(0));
    }

    void DrawAddCoins()
    {
        DrawText("This shop addons comes with 4 options of <b>Coins purchase packs</b>, so player can buy, but you can add or edit as you wish, for it first you have to add / modify the pack information in: " +
            "Addons (Folder) -> Shop -> Resources -> ShopData -> CoinsPack -> there you can add more options or edit one of the existing ones, just have 3 variables:");
        DrawHorizontalColumn("Name","The name of the coin pack");
        DrawHorizontalColumn("Amount", "The amount of coins that this pack give");
        DrawHorizontalColumn("Price", "The real price (in a real currency) that this pack cost");
        DrawImage(GetServerImage(1));
        DownArrow();
        DrawText("Next, you have to update the UI, this is not done automatically just because manually you can customize each packs UI to highlight some packs.\n \n So in <b>MainMenu</b> scene go to " +
            "(Hierarchy) <i>Lobby -> Canvas -> Lobby -> ShopMenu -> ShopUI -> BuyCoins -> Image -> Panel -> *</i> there you can edit, add or remove the UI packs information.");
        DrawImage(GetServerImage(2));
        DownArrow();
        DrawText("Now if you have add a new coin pack, there are one more step that you have to do, simply in the new UI pack go to CoinPack (14) *or you new pack name* -> Button (5) -> in the Button component ->" +
            " on the OnClick function there is an Integer variable you have to set the index of the pack information that you create in ShopData.");
    }

    void DrawAddPayment()
    {
        DrawText("The coins are the virtual money in the game, in MFPS for default players earn coins playing the game (by kills, wins, etc...), so these coins are used to purchase in this shop system," +
    " if player want to get more coins without playing they have the options to purchase these, now this shops system implement just the half of the integration of this 'coins purchase'," +
    "this system have all the local logic, show coins pack options, save coins in database, but doesn't include any real payment system, like for example paypal or UnityIAP, why? well basically due 2 main reasons, first because there are many " +
    " platforms and almost each one of them require that use their own IAP / API system for process real money purchases, for example Android, IOS, Steam, Kongregate, etc... so integrate for example Paypal IPN " +
    "will be useless in any of these platforms, so don't worth it, second reason is security.\n \nSo that is why this system doesn't comes with a payment system and is all up to the dev to integrated it," +
    " even though the system comes with all setup to make easy integrate any payment system, there is how: well just superficially since each API is a bit different:\n \n<b>#1</b> -You may ask where should I process the payment (after player " +
    "have selected the coin pack and click on the purchase button), there is where: <b>bl_ShopManager.cs ->  public void BuyCoinPack(int id)) line 286 by default</b>, you'll see a <b>Switch</b> conditional with some payment options," +
    "use any of these for example is you are trying to integrate UnityIAP, put the code after: <b>case bl_ShopData.ShopPaymentTypes.UnityIAP:</b>,\n \n now there are some basic information that you may need to send to the payment API, " +
    "like a name and the price to charge, you can get it from the local variable <b>coinPack</b> -> coinPack.Name, coinPack.Price, etc...");
        DownArrow();
        DrawText("Right after you have implement the Payment system and you have tested you have to save these coins to the player info in your database (after the payment is confirmed), for this simply " +
            "where you receive the local confirmation from your payment api you call <b>bl_DataBase.Instance.SaveNewCoins(coinPack.Amount);</b> if you are calling inside bl_ShopManager.cs, if you are calling from other script:" +
            " <b>bl_DataBase.Instance.SaveNewCoins(bl_ShopManager.Instance.coinPack.Amount);</b>\n \nThat's.");
    }

    void DrawAddSkin()
    {
        DrawText("In order to add a player skin in the shop to be available only by purchase you simply need <b>Player Selector</b> addon.\n \nIf you already have imported Player Selector in your project and added your custom player prefabs, then all what you need is set the Price of this player prefab to be > to 0, in <b>PlayerSelector</b> located in: Assets -> Addons -> PlayerSelector -> Content -> Resources -> PlayerSelector -> fold out the player prefab info (from <b>All Players</b> list) and in the Price variable set any amount > to 0 and that's!.");
        DrawServerImage(3);
    }

    void DrawAddCamo()
    {
        DrawText("In order to add a weapon camo in the shop to be available only by purchase you need <b>Customizer</b> addon.\n \nOnce you import Customizer in your project and added your weapon camos, then all what you need is set the Price of the camos that are for sale to be > to 0, in <b>CustomizerData</b> located in: Assets -> Addons -> Customizer -> Resources -> CustomizerData -> fold out the \"CamoInfo\" info (from <b>GlobalCamos</b> list) and in the Price variable set any amount > to 0 and that's!.");
        DrawServerImage(4);
    }

    [MenuItem("MFPS/Tutorials/Shop")]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ShopAddonDocumentation));
    }

    [MenuItem("MFPS/Addons/Shop/Tutorial")]
    private static void ShowWindow2()
    {
        EditorWindow.GetWindow(typeof(ShopAddonDocumentation));
    }
}