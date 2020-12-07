using UnityEngine;
using UnityEngine.UI;

public class bl_WindowAnimator : MonoBehaviour {

    [SerializeField]private Animator Anim;
    [SerializeField]private string Parameter;
    [SerializeField]private Text m_Text;


    private bool Showing = true;
    private bool hasDefaultVert;

    void Awake()
    {
        hasDefaultVert = (m_Text.text == ">>");
    }

	public void Do()
    {
        Showing = !Showing;
        Anim.SetBool(Parameter,Showing);
        if (hasDefaultVert)
        {
            m_Text.text = (Showing) ? ">>" : "<<";
        }
        else
        {
            m_Text.text = (Showing) ? "<<" : ">>";
        }
    }
}