using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EventTriggerListener : MonoBehaviour, IPointerEnterHandler, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler, IPointerExitHandler
{
    public delegate void VoidDelegate(GameObject go);
    public delegate void DataDelegate(GameObject go, PointerEventData eventData);


    public VoidDelegate onClick;
    public VoidDelegate onDown;
    public VoidDelegate onEnter;
    public VoidDelegate onExit;
    public VoidDelegate onUp;
    public VoidDelegate onSelect;
    public VoidDelegate onUpdateSelect;
    public VoidDelegate onDoubleClick;

    private DateTime t1, t2;
    private string sOverSound = "";
    private string sClickSound = "";
    private Selectable slcComp = null;

    static public EventTriggerListener Get(GameObject go)
	{
        if (go == null) return null;
        EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
        if (listener == null) listener = go.AddComponent<EventTriggerListener>();
        return listener;
	}
    // Awake 
    void Awake()
    {
        slcComp = gameObject.GetComponent<Selectable>();
    }

    // Update is called once per frame
    void Update()
    {
        if (onExit != null) onExit(gameObject);
        if (onUp != null) onUp(gameObject);
    }

    public void SetSoundStr(string sover = "", string sclick = "")
	{
        sOverSound = sover;
        sClickSound = sclick;
	}

    public void OnPointerClick(PointerEventData eventData)
	{
        if (slcComp != null && !slcComp.IsInteractable()) return;

        //if(!string.IsNullOrEmpty(sClickSound))
            //add a sound for not set sounds...

        if(onClick != null)
		{
            onClick(gameObject);
		}
	}

    public void OnPointerDown(PointerEventData eventData)
	{
        if (slcComp != null && !slcComp.IsInteractable()) return;

        if (onDown != null) onDown(gameObject);
        t2 = DateTime.Now;
        if(t2 - t1 < new TimeSpan(0,0,0, 500))
		{
            if (onDoubleClick != null) onDoubleClick(gameObject);
		}
        t1 = t2;
    }

    public void OnPointerEnter(PointerEventData eventData)
	{
        if (slcComp != null && !slcComp.IsInteractable()) return;
        //if(!string.IsNullOrEmpty(sOverSound))
        //add a hover sound here....

        if (onEnter != null) onEnter(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
	{
        if (onExit != null) onExit(gameObject);
	}

    public void OnPointerUp(PointerEventData eventData)
	{
        if (onUp != null) onUp(gameObject);
	}

	public void OnSelect(BaseEventData eventData)
	{
        if (slcComp != null && !slcComp.IsInteractable()) return;
        if (onSelect != null) onSelect(gameObject);
	}

    public void OnUpdateSelected(BaseEventData eventData)
	{
        if (slcComp != null && !slcComp.IsInteractable()) return;
        if (onUpdateSelect != null) onUpdateSelect(gameObject);
	}

}

