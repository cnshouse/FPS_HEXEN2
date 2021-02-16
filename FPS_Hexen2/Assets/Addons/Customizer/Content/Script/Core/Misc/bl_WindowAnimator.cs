using UnityEngine;

namespace MFPS.Addon.Customizer
{
    public class bl_WindowAnimator : MonoBehaviour
    {

        [SerializeField] private Animator Anim = null;
        [SerializeField] private string Parameter = null;

        private bool Showing = true;

        public void Do()
        {
            Showing = !Showing;
            Anim.SetBool(Parameter, Showing);
        }
    }
}