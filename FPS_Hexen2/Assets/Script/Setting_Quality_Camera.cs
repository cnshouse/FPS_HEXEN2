using UnityEngine;
using System.Collections;

public class Setting_Quality_Camera : MonoBehaviour 
{
    public Object[] Low = null;
    public Object[] Middle = null;
    public Object[] High = null;

    private Object[] Current_Set = null;

    public void Initialize()
    {
        Handle(Low, false);
        Handle(Middle, false);
        Handle(High, false);
    }

    /// <summary>
    /// 使用品质
    /// </summary>
    /// <param name="_Value"></param>
    public void Use_Quality(int _Value)
    {
        Object[] _Set = null;

        switch (_Value)
        {
            case 0: _Set = Low; break;
            case 1: _Set = Middle; break;
            case 2: _Set = High; break;
        }

        if (Current_Set != null)
        {
            Handle(Current_Set, false);
        }

        Current_Set = _Set;

        Handle(_Set, true);
    }

    /// <summary>
    /// 处理
    /// </summary>
    /// <param name="_Data"></param>
    /// <param name="_State"></param>
    private void Handle(Object[] _Data, bool _State)
    {
        GameObject _Object = null;
        MonoBehaviour _Behaviour = null;

        int i = 0;

        for (i = 0; i < _Data.Length; i++)
        {
            if (_Data[i].GetType() == typeof(GameObject))
            {
                _Object = (GameObject)_Data[i];

                _Object.SetActive(_State);
            }
            else if (_Data[i].GetType() == typeof(Component))
            {
                _Behaviour = (MonoBehaviour)_Data[i];

                _Behaviour.enabled = _State;
            }
        }
    }
}
