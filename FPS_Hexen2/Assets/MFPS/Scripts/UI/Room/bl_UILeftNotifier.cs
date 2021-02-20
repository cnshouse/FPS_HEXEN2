using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace MFPS.Runtime.UI.Bindings
{
    public class bl_UILeftNotifier : MonoBehaviour
    {

        [SerializeField] private Text m_Text = null;


        public void SetInfo(string t, float time)
        {
            m_Text.text = t;
            StartCoroutine(Hide(time));
        }

        IEnumerator Hide(float t)
        {
            yield return new WaitForSeconds(t);
            Destroy(gameObject);
        }
    }
}