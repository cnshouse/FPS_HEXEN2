using UnityEngine;

namespace MFPS.ClassCustomization
{
    public class bl_ClassButtonHandler : MonoBehaviour
    {
        public bl_ClassCustomize m_ScriptTarget;
        public PlayerClass m_PlayerClass = PlayerClass.Assault;

        public void SendNewClass()
        {
            m_ScriptTarget.ChangeClass(m_PlayerClass);
        }
    }
}