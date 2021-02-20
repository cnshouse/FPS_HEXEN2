using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Runtime.UI
{
    public class bl_LoadoutDropdown : MonoBehaviour
    {
        private Dropdown dropdown;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            if (dropdown == null)
            {
                dropdown = GetComponent<Dropdown>();
                int lid = (int)PlayerClass.Assault.GetSavePlayerClass();
                dropdown.value = lid;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void OnChanged(int value)
        {
            var loadout = (PlayerClass)value;
            loadout.SavePlayerClass();
#if CLASS_CUSTOMIZER
            bl_ClassManager.Instance.CurrentPlayerClass = loadout;
#endif
            bl_EventHandler.DispatchPlayerClassChange(loadout);
        }
    }
}