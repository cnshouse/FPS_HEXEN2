using UnityEngine;
using UnityEngine.UI;
namespace MFPS.ULogin
{
    public class bl_SignIn : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InputField UserNameInput = null;
        [SerializeField] private InputField PasswordInput = null;
        [SerializeField] private Text KillStatic = null;
        [SerializeField] private Text Deathstatic = null;
        [SerializeField] private Text ScoreStatic = null;
        [SerializeField] private Text PlayTimeStatic = null;
        [SerializeField] private Text CoinsText = null;
        [SerializeField] private GameObject changePassButton;
        public Text ClanNameText;
        [SerializeField] private Toggle RememberToggle = null;
        private bl_LoginPro Login;

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            Login = bl_LoginPro.Instance;
            string credentials = bl_LoginProDataBase.Instance.RememberCredentials;
            bool b = string.IsNullOrEmpty(credentials);
            if (bl_LoginProDataBase.Instance.rememberMeBehave == RememberMeBehave.RememberUserName)
            {
                if (b == false) { UserNameInput.text = credentials; }
            }
            RememberToggle.isOn = !b;
        }

        /// <summary>
        /// 
        /// </summary>
        void OnDisable()
        {
            // UserNameInput.text = string.Empty;
            PasswordInput.text = string.Empty;
        }

        public void OnLogin(LoginUserInfo info)
        {
            KillStatic.text = info.Kills.ToString();
            Deathstatic.text = info.Deaths.ToString();
            ScoreStatic.text = info.Score.ToString();
            CoinsText.text = info.Coins.ToString();
            PlayTimeStatic.text = bl_DataBaseUtils.TimeFormat(info.PlayTime);
            changePassButton.SetActive(info.authenticationType == AuthenticationType.ULogin);
#if CLANS
        if (info.Clan != null && info.Clan.haveClan)
        {
            ClanNameText.text = info.Clan.Name;
        }
        else { ClanNameText.transform.parent.gameObject.SetActive(false); }
#else
            ClanNameText.transform.parent.gameObject.SetActive(false);
#endif
        }

        public void DoSignIn()
        {
            if (bl_ULoginLoadingWindow.Instance.isShowing) return;
            SignIn();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool SignIn()
        {
            if (Login == null)
                return false;

            string user = UserNameInput.text;
            string pass = PasswordInput.text;

            if (string.IsNullOrEmpty(user))
            {
                Debug.Log("User name can't be empty, please write your user name");
                Login.SetLogText("User name can't be empty, please write your user name");
                return false;
            }
            if (string.IsNullOrEmpty(pass))
            {
                Debug.Log("Password can't be empty, please write your password");
                Login.SetLogText("Password can't be empty, please write your password");
                return false;
            }
            Login.Login(user, pass);
            if (bl_LoginProDataBase.Instance.rememberMeBehave == MFPS.ULogin.RememberMeBehave.RememberUserName)
            {
                if (RememberMe())
                    bl_LoginProDataBase.Instance.RememberCredentials = user;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetFields()
        {
            PasswordInput.text = string.Empty;
            UserNameInput.text = string.Empty;
        }

        public void SetCredentials(string user, string password)
        {
            UserNameInput.text = user;
            PasswordInput.text = password;
        }

        public bool RememberMe() => RememberToggle.isOn;
    }
}