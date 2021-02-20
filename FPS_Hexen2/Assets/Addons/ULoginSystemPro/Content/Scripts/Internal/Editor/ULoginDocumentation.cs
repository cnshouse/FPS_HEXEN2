using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor;
using UnityEngine.Networking;

public class ULoginDocumentation : TutorialWizard
{

    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "login-pro/editor/";
    private NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "https://www.awardspace.com/images/web_hosting_v2_04.jpg",Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.png", Image = null},
        new NetworkImages{Name = "img-5.jpg", Image = null},
        new NetworkImages{Name = "img-6.jpg", Image = null},
        new NetworkImages{Name = "img-7.jpg", Image = null},
        new NetworkImages{Name = "img-8.jpg", Image = null},
        new NetworkImages{Name = "img-9.jpg", Image = null},
        new NetworkImages{Name = "img-10.jpg", Image = null},
        new NetworkImages{Name = "img-11.jpg", Image = null},
        new NetworkImages{Name = "img-12.jpg", Image = null},
        new NetworkImages{Name = "img-13.png", Image = null},
        new NetworkImages{Name = "img-14.png", Image = null},
        new NetworkImages{Name = "img-15.png", Image = null},
        new NetworkImages{Name = "img-16.png", Image = null},
        new NetworkImages{Name = "img-17.png", Image = null},//16
        new NetworkImages{Name = "img-18.png", Image = null},
        new NetworkImages{Name = "img-19.png", Image = null},
        new NetworkImages{Name = "img-20.png", Image = null},
        new NetworkImages{Name = "img-21.png", Image = null},
        new NetworkImages{Name = "img-22.png", Image = null},
    };
    private readonly GifData[] AnimatedImages = new GifData[]
   {
        new GifData{ Path = "lsp-cd-1.gif" },
        new GifData{ Path = "lsp-uffz-2.gif" },
   };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "Require", StepsLenght = 0 , DrawFunctionName = nameof(DrawRequire)},
    new Steps { Name = "Hosting", StepsLenght = 3  , DrawFunctionName = nameof(DrawHosting)},
    new Steps { Name = "ULogin", StepsLenght = 4  , DrawFunctionName = nameof(DrawULogin)},
    new Steps { Name = "Admin Panel", StepsLenght = 0  , DrawFunctionName = nameof(DrawAdminPanel)},
    new Steps { Name = "Version Checking", StepsLenght = 0 , DrawFunctionName = nameof(DrawVersionChecking) },
    new Steps { Name = "Email Confirmation", StepsLenght = 0  , DrawFunctionName = nameof(DrawEmailConfirmation)},
    new Steps { Name = "Security", StepsLenght = 0  , DrawFunctionName = nameof(SecurityDoc)},
    new Steps { Name = "Common Problems", StepsLenght = 0  , DrawFunctionName = nameof(DrawCommonProblems)},
    new Steps { Name = "Installation Service", StepsLenght = 0  , DrawFunctionName = nameof(InstallationServiceDoc)},
    };
    //final required////////////////////////////////////////////////

    EditorWWW www = new EditorWWW();
   // public ULoginFileUploader fileUploader;

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder, AnimatedImages);
        GUISkin gs = Resources.Load<GUISkin>("content/MFPSEditorSkin") as GUISkin;
        if (gs != null)
        {
            base.SetTextStyle(gs.customStyles[2]);
        }
        FetchWebTutorials("login-pro/tuts/");
    }

    public override void WindowArea(int window)
    {
        AutoDrawWindows();
    }

    void DrawRequire()
    {
        DrawText("ULogin System allow to player can authenticated so they can save and load their game progress and other data trough sessions," +
            " to reach this all these data are store in a external database using PHP and Mysqli, this database need to be set up before can be accessed.");
        DownArrow();
        DrawText("In order to create a Database you need:");
        DrawHorizontalColumn("Domain", "domain or web name is the address where Internet users can access your website or hosting files, for example: <i>lovattostudio.com</i>");
        DrawHorizontalColumn("Hosting Space", "an online server for store your web files on Internet so they can be viewed and accessible");
        DrawHorizontalColumn("FTP Client", "stands for File Transfer Protocol. Using an FTP client is a method to upload, download, and manage files on our server, there are some free third party program" +
            " or optionals some hosting services provide a admin panel for this (like we will see later in this tutorial)");
        DrawText("For iOS and Android an SSL Certificated <i>(https)</i> is needed.\n");
        DownArrow();
        DrawText("if you have or had a web site, you may be familiar with these requirements, if not you can learn how to obtain these for free in the next step.");
        GUILayout.Space(15);
    }

    void DrawHosting()
    {
        if (subStep == 0)
        {
            DrawText("Like I said before, you need a web hosting in order to use ULogin Pro, if you already have one and you know how to upload files to your server you can skip this step.");
            DownArrow();
            DrawText("there are a lot of web hosting services around Internet where you can register a domain and use hosting space, the majority of this offer only paid plans, but there are someones that" +
                " offers free accounts with limitations of course, but that is ok, you can use these free features in your Game Development phase and upgrade to a better plan when you release your game.");
            DrawText("so in this tutorial I will use a Hosting Service that offer a good free plan as example, it's called <b>Awardspace</b>, you can check it here: ");
            if (DrawImageAsButton(GetServerImage(0)))
            {
                Application.OpenURL("https://www.awardspace.com/?id=AW-16015-0");
            }
            DownArrow();
            DrawText("So in the web site of the hosting, create a account like in another site and then select the free plan or the plan that you prefer.");
            DrawText("Once you have register your account, you should see this dashboard:");
            DrawImage(GetServerImage(1));
        }else if(subStep == 1)
        {
            DrawText("Now you need to register a domain, for it go to (in the site dashboard) Hosting Tools -> Domain Manager:");
            DrawImage(GetServerImage(2));
            DownArrow();
            DrawText("Now if you have the a free plan you need to select <b>Register a Free Subdomain</b> or if you have a free plan but wanna have a custom domain like .com,.net,.uk, etc... <i>(Recommended)</i> you can use <b>Register a Domain</b> option and purchase it, but in this tutorial I will use only free alternatives, it is all up to you. So write your domain name in the input field like for example: <i>mygametest</i> and click on <b>Create</b>.");
            DrawImage(GetServerImage(3));
            DrawText("if the domain if available to register, you will see other steps to finish with the registration, so just follow it, just be sure to not select any pay feature that they will offer (if you don't want it of course)");
            DownArrow();
        }/*else if(subStep == 2)
        {
            DrawText("Now we have the hosting and the domain name ready, let's create a directory to store our ulogin files, so go to <b>Hosting Tools -> File Manager</b>");
            DownArrow();
            DrawText("in the page that will load, double click to open the folder with your domain name, then you will see that the folder is empty, here you need create folders that work as" +
                " directory of your server, so create a folder to host your game files, simply click on the button <b>Create</b> and in the popup window that will appear write your folder name:");
            DrawImage(GetServerImage(4));
            DownArrow();
            DrawText("Open the new folder, and if you want can create other folder to store only the php script, example called it \"php\" so the directory will be like: \"game-name\\php\\\".");          
        }*/else if(subStep == 2)
        {
            DrawText("Right, so now we have the hosting and domain name and our the directory to upload the ulogin files, now you need create a Database");
            DownArrow();
            DrawText("Go to <b>Hosting Tools -> MySQL DataBases</b>, in the loaded page you will see a simply form for create the database, just set the database name and a password, \nfor the name you" +
                " can simple set something like \"game\" and a secure password, and then click on <b>Create DataBase</b> button");
            DrawImage(GetServerImage(5));
            DownArrow();
            DrawText("Good, now you have your web hosting and database ready!, continue with the next step to set up ULogin");
        }
    }

    int checkID = 0;
    public string unformatedURL = "";
    public string formatedURL = "";
    void DrawULogin()
    {
        if (subStep == 0)
        {
            DrawText("Now that you have all necessary, it's time to set up the ULogin files, first thing that you need to do is upload the php scripts in your web hosting directory," +
                "these php scripts allow the communication from the game client to your server and the server handle with the Database stuff with the info receive from the client, but before that you need " +
                "set some info in one of the php scripts.");
            DrawText("<b>Open</b> the script called <b>bl_Common.php</b> (on the PHP folder of ULogin)");
            DrawImage(GetServerImage(7));
            DownArrow();
            DrawText("In this scripts you need set the information of your database, that is used in order to access to the database for read and write when we required it, for obtain the information" +
                " of the database varies of hosting on hosting, but in Awardspace you can get in: Hosting Tools -> MySQL DataBase -> (wait until load finish) -> click on the button under <b>Options</b>" +
                " -> Information -> database information will display like this: ");
            DrawImage(GetServerImage(8));
            DownArrow();
            DrawText("Now in <b>bl_Common.php</b> you need set the database information like this");
            DrawHorizontalColumn("HOST_NAME", "The name of your host, get it from the database info in the hosting page");
            DrawHorizontalColumn("DATA_BASE_NAME", "The name the database, get it from the database info in the hosting page");
            DrawHorizontalColumn("DATA_BASE_USER", "The user name of the database, in Awardspace this is the same as the database name");
            DrawHorizontalColumn("DATA_BASE_PASSWORLD", "The password that you set when create the database");
            DrawHorizontalColumn("SECRET_KEY", "Set a custom 'password' key, that just work as a extra layer of security to prevent others can execute the php code, is <b>Highly recommended that you set your own secret key</b>, just make sure to remember it since you will need it in the next step.");
            DrawHorizontalColumn("ADMIN_EMAIL", "Your server email from where 'Register confirmation' will be send (Only require if you want use confirmation for register), <b>NOTE:</b> this email need to be configure in your hosting, in Awardspace you can create a " +
                "email account in <b>Hosting Tools -> E-mail Account</b>.");
            DrawText("So after you set all the info, your script should look like this (with your own info of course)");
            DrawImage(GetServerImage(21));
            DrawText("Don't forget to save the changes in the script.");
        }
        else if (subStep == 1)
        {
           /* if (fileUploader == null) fileUploader = new ULoginFileUploader(this);

            fileUploader.DrawGUI();*/

            DrawText("Now the next step is upload the server side scripts (.php, .sql and .xml) to your server domain directory.\n\nLets explain first in what consist a directory first, very short, let say your registered domain is <color=#79D3FFFF>myexampledomain.com</color>, that is the root of your domain address/URL, now you can create subfolders in that domain to extend the address, so if you create a folder called <i>phpfiles</i> then the address to point to that folder will be <color=#79D3FFFF>myexampldomain.com/phpfiles/</color> if you create another folder inside this you simple add the folder name plus a right-slash at the end.");

            DrawHyperlinkText("Now to manage your folders/domain directory and upload files to it you need an FTP Client, since some web hosting file uploaders don't support nested folders uploads we have to use <b>FileZilla FTP Client</b> to upload our files to our server, FileZilla is free software, you can download it here: <link=https://filezilla-project.org/>https://filezilla-project.org/</link> or using your preferred FTP Client program.\n\n<b>Download the FTP Client</b> not the FTP Server.\n\nOnce you download it -> install it following the installation wizard of the program -> then <b>Open</b> it.\n\nNow to be able to upload the files using this tools you need to set your FTP Credentials, all web hostings provide these credentials in their panel/dashboard, in Awardspace you can find these credentials in: <b>Hosting Tools -> FTP Manager</b>");
            DrawServerImage(16);
             DownArrow();
            DrawText("There you will see a form to create a FTP account, fill the required info and click on <b>Create FTP Account</b> button");
            DrawServerImage(17);
            DownArrow();
            DrawText("After this below that box you'll see the FTP Account section, there you should see the FTP account that you just create, click on the <i><b>settings</b></i> button on the right side of your FTP account -> then select the <b>Information</b> tab, there you will see your FTP account credentials with which you can access to your server:");
            DrawServerImage(18);
            DrawText("Now open FileZilla and at the top of the menu you will see the fields to insert your FTP credentials, insert the credentials showing in your hosting, in Awardspace, they are the ones that you just open above, once you insert the credentials click on the <b>Quickconnect</b> button, if the credentials are correct you should see a message like <i><b>Directory listing of \"/\" successful</b></i>, if that's so then you are ready to upload the files.");
            DrawServerImage(19);
            DownArrow();
            DrawText("So now on the FileZilla in the <b>Remote Site side panel</b> you should see a directory folder with at least a folder in it, open the folder <b>that has your domain name</b> -> now with the root directory open you have two options: upload the scripts files right there in the root or created a sub directory to be more organized, I recommend create a subfolder , for it simple right click on the window -> Create directory -> in the popup window that will appear set the folder name as you want, for this tutorial I will create two nested folders: <b>mygame -> php</b> <i>where I'll upload my files)</i>");
            DrawAnimatedImage(0);
            DrawText("With the created folder open you have to upload the ULogin Pro server files, for it in the Unity Editor go to the folder located at: <i><b>Assets -> Addons -> ULoginSystemPro -> Content -> Scripts-> Php</b></i> select one of the script on that folder -> Right Mouse Click over it -> Show on Explorer.");
            DrawServerImage(15, TextAlignment.Center);
            DrawText("Now in the OS window explorer <b>select all the files including the phpseclib folder</b> <i><size=10><color=#76767694>(without the .meta files)</color></size></i> and drag them in the <b>Remote Site</b> panel on <b>FileZilla</b>");
            DrawAnimatedImage(1);
            DownArrow();
            DrawText("Once all scripts are uploaded, copy the URL from the <b>Remote Site</b> directory field <i>(on FileZilla)</i>");
            DrawServerImage(20);
            DrawText("This is just the directory to your files folder container, you have to format it to a working URL <i>(add the HTTP prefix)</i>, for it simple paste the copied url bellow and click on <b>Format</b> button, after formated it -> click on the <b>Assign URL</b> button.");

           /* DrawServerImage(14, TextAlignment.Center);

            DrawText("In the new page you should see a directory panel with at least a folder in it, open the folder <b>that has your domain name</b> -> now with the root directory open you have two options: upload the scripts files right there in the root or created a sub directory to be more organized, I recommend create a subfolder , for it simple click on the button Create and in the popup window that will appear set the folder name as you want, for this tutorial I will call it as: <b>php</b> -> after creating the folder open it and click on the <b>Upload</b> button -> in the popup window that will show, drag all the scripts of the PHP folder from ULogin Pro as follow:\n\nIn the Unity Editor go to the folder located at: <i>Assets->Addons->ULoginSystemPro->Content->Scripts->Php</i> select one of the script on that folder -> Right Mouse Click over it -> Show on Explorer.\n");
            DrawServerImage(15, TextAlignment.Center);
            DownArrow();
            DrawText("In your PC/Mac window explorer that will open select <b>ALL</b> the scripts <b>except</b> for the ones that ends with .meta, then drag all those in the Awardspace popup panel and click on the <b>Upload</b> button.\n");
            DrawImage(GetServerImage(6));
            DownArrow();
            DrawText("Once all scripts are uploaded, copy the URL from the panel field <i>(<b>NOT</b> the browser search field)</i>");
            DrawImage(GetServerImage(10));
            DownArrow();
            DrawText("Now this is just the directory to your files folder container, you have to convert to a working URL <i>(add the HTTP prefix)</i>, for it simple paste the copied url bellow and click on <b>Format</b> button, after formated click on the <b>Assign URL</b> button.");*/

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(100);
                if (string.IsNullOrEmpty(formatedURL))
                {
                    unformatedURL = EditorGUILayout.TextField(unformatedURL);
                    GUI.enabled = !string.IsNullOrEmpty(unformatedURL);
                    if (GUILayout.Button("Format", EditorStyles.toolbarButton, GUILayout.Width(100)))
                    {
                        string url = unformatedURL;
                        if (url.StartsWith("/"))
                        {
                            url = url.Remove(0, 1);
                            url = $"http://www.{url}";
                        }
                        else if (!url.StartsWith("http"))
                        {
                            url = $"http://www.{url}";
                        }

                        if (!url.EndsWith("/"))
                        {
                            url += "/";
                        }
                        formatedURL = url;
                        GUI.FocusControl(null);
                        Repaint();
                    }
                }
                else
                {
                    GUILayout.Label(EditorGUIUtility.IconContent("Collab").image,GUILayout.Width(20));
                    formatedURL = EditorGUILayout.TextField(formatedURL);
                    GUI.enabled = !string.IsNullOrEmpty(formatedURL);
                    if (GUILayout.Button("Assign URL", EditorStyles.toolbarButton, GUILayout.Width(100)))
                    {
                        bl_LoginProDataBase.Instance.PhpHostPath = formatedURL;
                        EditorUtility.SetDirty(bl_LoginProDataBase.Instance);
                        Selection.activeObject = bl_LoginProDataBase.Instance;
                        EditorGUIUtility.PingObject(bl_LoginProDataBase.Instance);
                    }
                }
                GUI.enabled = true;
                GUILayout.Space(100);
            }
            GUILayout.EndHorizontal();
            DownArrow();
            DrawHyperlinkText("Now in the Inspector Window of the Unity Editor you should see the fields of <link=asset:Assets/Addons/ULoginSystemPro/Content/Resources/LoginDataBasePro.asset>LoginDataBasePro</link> <i>(otherwise click the button bellow)</i>, if everything work correctly you should see your URL assigned in the <b>PhpHostPath</b> field.\n\n-Remember from the last step the <b>SecretKey</b> that was in <b>bl_Common.php -> SECRET_KEY</b>, if you have change it in <b>bl_Common.php</b> you have to set the same Key in the field <b>Secret Key</b> of <link=asset:Assets/Addons/ULoginSystemPro/Content/Resources/LoginDataBasePro.asset>LoginDataBasePro</link>.");
            DrawImage(GetServerImage(11));
            if (DrawButton("LoginDataBasePro"))
            {
                Selection.activeObject = bl_LoginProDataBase.Instance;
                EditorGUIUtility.PingObject(bl_LoginProDataBase.Instance);
            }
        }
        else if (subStep == 2)
        {
            DrawText("Now you need set up the tables in your database, for this we'll use SQL, you can do that manually or automatically," +
                "for do it manually you can use some database tool like PhpMyAdmin and run the SQL query in their sql panel or you can do it here.");
            DownArrow();
            DrawText("First lets check that the tables has not been created yet, click on the button bellow to check the tables.");
            GUILayout.Space(5);
            bool isLURL = bl_LoginProDataBase.Instance.PhpHostPath.Contains("lovatto");
            if (isLURL)
            {
                GUILayout.Label("<color=red>You're still using the lovatto studio url which is for demonstration only, please use your own url to continue.</color>");
            }
            GUI.enabled = (checkID == 0 && !isLURL);
            if (DrawButton("Check Tables"))
            {
                WWWForm wf = new WWWForm();
                wf.AddField("type", 3);
                www.SendRequest(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Creator), wf, CheckResult);
                checkID = 1;
            }
            GUI.enabled = true;
            if (checkID == 1)
            {
                GUILayout.Label("LOADING...");
            }
            else if (checkID == 2)
            {
                GUILayout.Label("RESULT: " + checkLog);
            }
            else if (checkID >= 3)
            {
                GUI.enabled = (checkID == 3);
                DrawText("Ok, we have checked and tables are not created yet, so we can do it now, click on the button bellow to create tables");
                GUILayout.Space(5);
                if (DrawButton("Create Tables"))
                {
                    /* WWWForm wf = new WWWForm();
                     wf.AddField("type", 4);
                     www.SendRequest(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Creator), wf, CheckCreation);*/
                    EditorCoroutines.StartBackgroundTask(DoCreateTables());
                    checkID = 4;
                }
                GUI.enabled = true;
                if (checkID == 4) { GUILayout.Label("LOADING..."); }
                if(checkID == 5)
                {
                    DrawText("Done!, Tables has been created successfully.");
                }else if(checkID == 6)
                {
                    DrawText("Couldn't create tables error: <color=red>" + checkLog + "</color>");
                }
            }
        }
        else if(subStep == 3)
        {
            DrawText("Done!, you have set up your database and are ready to use ULogin System!");
            DownArrow();
            DrawText("So let me explain some the rest of options in <b>LoginDataBasePro</b>");
            DrawHorizontalColumn("Update IP", "ULogin collect the IP of the register user in order to block their IP if they get banned in game, if you enable this ULogin will check the IP each time" +
                " that the user log-in so always keep the store IP update");
            DrawHorizontalColumn("Detect Ban", "Basically is = to say: you wanna use Ban Features.");
            DrawHorizontalColumn("Require Email Verification", "After register a account user is required to verify his email in order to log-in?");
            DrawHorizontalColumn("Check Ban In Mid", "Check if the player has not been banned each certain amount of time, if this is false that check will only be fire when player log-in");
            DrawHorizontalColumn("Can Register Same Email", "Can a email be use to register different accounts?");
        }
    }

    IEnumerator DoCreateTables()
    {
        WWWForm wf = new WWWForm();
        wf.AddField("type", 4);
        var url = bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Creator);
        using (UnityWebRequest w = UnityWebRequest.Post(url, wf))
        {
            w.SendWebRequest();
            while (!w.isDone) { yield return null; }

            if(!w.isNetworkError && !w.isHttpError)
            {
                var text = w.downloadHandler.text;
                if (text.Contains("done"))
                {
                    checkLog = "Tables created successfully";
                    checkID = 5;
                }
                else
                {
                    checkLog = text;
                    checkID = 6;
                }
                Repaint();
            }
            else
            {
                if(w.error.Contains("destination host"))
                {
                    Debug.LogError($"{w.error}\nTry removing the www. from the URL in LoginDataBasePro->PhpHostPath");
                }else
                Debug.LogError($"{w.error}\n{url} - {w.responseCode}");
                checkID = 3;
            }
        }
    }
    
    void DrawAdminPanel()
    {
        DrawText("ULogin comes with a handy scene that allow the admin/dev and their game moderators make some basic management like Ban, Ascend,etc... to the users right in the game");
        DrawText("For example if you wanna ban a user because X or Y reason you can do as simple as open and play the AdminPanel scene -> in the left side panel, write the user login or nick name -> " +
            "click on Search -> Click on Ban and done, same for UnBan ascend, descend, etc...");
        DownArrow();
        DrawText("Also allow to reply to Tickets summited by players, check some game statistics and database stat, to access to the AdminPanel in game user need to have a status / role of" +
            " Admin or Moderator, so when he/she log-in a AdminPanel button will appear in the log-in confirmation window.");
    }

    void DrawVersionChecking()
    {
        DrawText("ULogin 1.6 comes with a new featured which is 'Version Checking' what this do is compare the local game version with the server game version, if the local version is different then " +
            "players will be not able to login or play the game until they update the game.");
        DownArrow();
        DrawText("To enable this feature, simple go to <i>Assets -> Addons -> ULoginSystemPro -> Content -> Resources -> LoginDataBasePro -> check 'Check Game Version'");
        DownArrow();
        DrawText("Now to change the Game Version, first the Local Game Version is in the GameData in the Resources folder of MFPS -> GameData -> GameVersion.");
        DrawText("For change the server Game Version you have to edit the bl_Common.php script in your server and edit the variable '$GameVersion'");
    }

    void DrawEmailConfirmation()
    {
        DrawText("ULogin Pro comes with an <b>Email confirmation</b> feature, this feature is optional and is disable by default you can enabled it in <b>LoginDataBasePro</b> -> Required Email Verification.\n\nOnce the feature is enable, when an user register/create a new account a email will send to the user email to verified that the email is real, player wont be able to log in until the email is verified.\n\nNow, this feature use SMTP <i>(Simple Mail Transport Protocol)</i> which is the protocol to send emails to servers, <b>this feature has to be enabled in your server in order to be able to send emails</b>, if you are using Awardspace SMTP is enabled by default, if you are using other hosting provider and you don't know how to check if it's enabled or how to enable it, contact the support of your hosting provider.\n\nOk, once SMTP is activated, you also need an Email account on your server that catch all your server mails, normally hosting doesn't have an default email so you have to manually created it, in Awardspace to create an Email account go to: Hosting Tools -> Emails Accounts.\n");
        DrawServerImage(12, TextAlignment.Center);
        DrawText("Now in the open page you will have a form to create an Email account, just fill the email name and set the password, but before created click in the \"<b>Advance Settings</b>\" and in the new settings that will appear <b>Turn On</b> the \"<b>Catch-all</b>\" and \"<b>Default e-mail for scripts</b>\" toggles, then click on <b>Create</b> button.");
        DrawServerImage(13, TextAlignment.Center);
        DrawText("Ok, now that the email is created you have to assign it in <b>bl_Common.php</b> script <i>(the one that you upload in your server)</i> -> $emailFrom.\n\nNow your email confirmation system will be ready!\n");
    }

    void SecurityDoc()
    {
        DrawText("ULogin Pro uses many security measures in both clients and server-side to prevent common attacks/exploits to make sure that the user data is safe and also to protect the game database.\n\nJust to enums some of them:\n\n■ SQL sanitize to prevent SQL injection from the user input.\n■ Custom hash required to execute functions on server-side.\n■ P2P encryption using RSA and AES algorithms on all sensitivity requests using the latest phpseclib functions.\n■  Max login attempts.\n■ Password hashed and encrypted.\n■ Email verification.\n■ None sensitivity data is serialized in the game build.\n\nBut by default some of those features are disabled, here is a list of things that you should do to use ULogin with the maximum security measures:\n\n<size=16>1. Set a custom and secure <b>Secret Key</b>:</size>");

        DrawHyperlinkText("- You have to set this key in two places in <link=asset:Assets/Addons/ULoginSystemPro/Content/Resources/LoginDataBasePro.asset>LoginProDataBase</link> ➔ Secret Key and in the PHP script bl_Common.php ➔ SECRET_KEY (the one that you upload to your server), it has to be the same key, make sure it is at least 16 chars long and includes letters, digits, and symbols.");
        Space(25);
        DrawHyperlinkText("<size=16>2 - Enable PeerToPeer encryption:</size>\n\n- This feature is disabled by default because it uses RSA encryption on the server-side which is a little bit slower and at a large scale requires more server resources.\n\nTo enable it simply turn on the toggle '<b>PeerToPeer Encryption</b>' in <link=asset:Assets/Addons/ULoginSystemPro/Content/Resources/LoginDataBasePro.asset>LoginDataBasePro</link> and in <b>bl_Common.php</b> <i>(the script in your server)</i> set the const '<b>PER_TO_PER_ENCRYPTION</b>' = true.");
        Space(25);

        DrawHyperlinkText("<size=16>3 - Use HTTPS</size>\n\n- This feature is external to ULogin but is a highly recommended and some times required feature that your server should have, <b>many platforms like iOS and Android required the use of secure https</b> request so if you are planning release your game on one of those platforms you must have a valid SSL certificated for your domain.\n\nTo get an SSL certificate <i>(and be able to use HTTPS instead of HTTP)</i>\nyou need to buy it, if you don't know how to install an SSL certificate on your server your best option is contact to your server hosting provider since more likely they self sell and install certificates.\n\nOnce you have a valid SSL certificated installed in your domain, you simply have to use HTTPS instead of HTTP in the URL that you set in <link=asset:Assets/Addons/ULoginSystemPro/Content/Resources/LoginDataBasePro.asset>LoginDataBasePro</link> ➔ <b>PhpHostPath</b>, e.g: <i>http://www.mydomain.com/files/</i> ➔ should be: <i>https://www.mydomain.com/files/</i>");
    }

    void DrawCommonProblems()
    {

    }

    void InstallationServiceDoc()
    {
        DrawText("If you are having trouble or just want to save time setting up the server-side database, we offer the installation service upon requests for a small fee.\n");
        if(Buttons.FlowButton("Contact Us"))
        {
            Application.OpenURL("https://www.lovattostudio.com/en/select-support/index.html");
        }
    }

    string checkLog = "";
    void CheckResult(string data, bool isError)
    {
        if (data.Contains("yes"))
        {
            checkLog = "Tables are already created";
            checkID = 2;
        }
        else if(data.Contains("no"))
        {
            checkLog = "Tables are not created yet";
            checkID = 3;
        }else
        {
            checkLog = data;
            checkID = 2;
        }
        Repaint();
    }

    void CheckCreation(string data, bool isError)
    {
        if (data.Contains("done"))
        {
            checkLog = "Tables created successfully";
            checkID = 5;
        }
        else
        {
            checkLog = data;
            checkID = 6;
        }
        Repaint();
    }

    [MenuItem("MFPS/Addons/ULogin/Tutorial")]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ULoginDocumentation));
    }

    [MenuItem("MFPS/Tutorials/ULogin Pro")]
    private static void ShowWindowMFPS()
    {
        EditorWindow.GetWindow(typeof(ULoginDocumentation));
    }
}