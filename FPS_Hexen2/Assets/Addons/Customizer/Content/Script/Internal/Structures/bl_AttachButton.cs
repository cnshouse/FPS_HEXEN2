using UnityEngine;
using System.Collections;

public class bl_AttachButton : MonoBehaviour
{

    public bl_AttachType m_Type;

    [SerializeField]private MeshRenderer TextRender;
    [SerializeField] private AudioClip OnEnterSound;
    private Vector3 DefaultScale = Vector3.zero;
    private Vector3 CurrentScale;
    private AudioSource ASource;
    private Renderer m_Render;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        DefaultScale = transform.localScale;
        CurrentScale = DefaultScale;
        ASource = GetComponent<AudioSource>();
        m_Render = GetComponent<Renderer>();
    }

    void OnEnable()
    {
        StartCoroutine(OnUpdate());
    }

    public void Active(bool v)
    {
        TextRender.enabled = v;
        CurrentScale = (v) ? DefaultScale * 2 : DefaultScale;
    }

    private void OnMouseEnter()
    {
        if (!m_Render.enabled) return;
        ASource.clip = OnEnterSound;
        ASource.Play();
    }

    public IEnumerator OnUpdate()
    {
        while (true)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, CurrentScale, Time.deltaTime * 15);
            yield return null;
        }
    }
}