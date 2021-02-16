using UnityEngine;
using System.Collections;

public class Skill_Roadhog_Rake_Me : MonoBehaviour
{
    #region 属性

    #region 变量

    /// <summary>
    /// 检测层
    /// </summary>
    public LayerMask Detection_Layer
    {
        get
        {
            return (1 << LayerMask.NameToLayer("force2_player")) | (1 << LayerMask.NameToLayer("force2_npc")) | (1 << LayerMask.NameToLayer("force1_player")) | (1 << LayerMask.NameToLayer("force1_npc"));
        }
    }

    #endregion

    #region 对象

    /// <summary>
    /// 链子对象
    /// </summary>
    private GameObject _Chain_Object = null;

    public GameObject Chain_Object
    {
        get 
        {
            if (_Chain_Object == null)
            {
                Transform _Transform = null;

                _Transform = Get_Child_Transform(gameObject, "Chain");

                if (_Transform != null)
                {
                    _Chain_Object = _Transform.gameObject;
                }
            }

            return _Chain_Object; 
        }

        set { _Chain_Object = value; }
    }

    /// <summary>
    /// 耙子对象
    /// </summary>
    private GameObject _Weapon_Rake_Object = null;

    public GameObject Weapon_Rake_Object
    {
        get 
        {
            if (_Weapon_Rake_Object == null)
            {
                Transform _Transform = null;

                _Transform = Get_Child_Transform(gameObject, "Object001");

                if (_Transform != null)
                {
                    _Weapon_Rake_Object = _Transform.gameObject;  
                }
            }

            return _Weapon_Rake_Object; 
        }

        set { _Weapon_Rake_Object = value; }
    }

    /// <summary>
    /// 链子对象
    /// </summary>
    private GameObject _Weapon_Chain_Object = null;

    public GameObject Weapon_Chain_Object
    {
        get 
        {
            if (_Weapon_Chain_Object == null)
            {
                Transform _Transform = null;

                _Transform = Get_Child_Transform(gameObject, "mod_weapon_gouzi");

                if (_Transform != null)
                {
                    _Weapon_Chain_Object = _Transform.gameObject;
                }
            }
 
            return _Weapon_Chain_Object; 
        }

        set { _Weapon_Chain_Object = value; }
    }

    #endregion

    #region 组件

    /// <summary>
    /// 链子处理组件
    /// </summary>
    private Chain_Animator_Handle _Chain_Animator_Handle_Component = null;

    public Chain_Animator_Handle Chain_Animator_Handle_Component
    {
        get 
        {
            if (_Chain_Animator_Handle_Component == null)
            {
                if (Chain_Object != null)
                {
                    _Chain_Animator_Handle_Component = Chain_Object.GetComponent<Chain_Animator_Handle>();

                    if (_Chain_Animator_Handle_Component == null)
                    {
                        _Chain_Animator_Handle_Component = Chain_Object.AddComponent<Chain_Animator_Handle>();
                    }
                }
            }

            return _Chain_Animator_Handle_Component; 
        }

        set { _Chain_Animator_Handle_Component = value; }
    }

    /// <summary>
    /// 动画组件
    /// </summary>
    private Animator _Animator_Component = null;

    public Animator Animator_Component
    {
        get 
        {
            if (_Animator_Component == null)
            { 
                _Animator_Component = gameObject.GetComponent<Animator>();
            }

            return _Animator_Component; 
        }

        set { _Animator_Component = value; }
    }

    #endregion

    #endregion

    /// <summary>
    /// 用来初始化
	/// </summary>
	private void Start () 
    {
        Chain_Object.SetActive(false);
	}
	
	/// <summary>
    /// 每帧更新调用一次
	/// </summary>
	private void Update () 
    {
	    
	}

    /// <summary>
    /// 绘制UI
    /// </summary>
    private void OnGUI() 
    {
        if (GUI.Button(new Rect(100, 100, 100, 30), "扔链子"))
        {
            Throw_Out_Rake();
        }
    }

    /// <summary>
    /// 得到子对象
    /// </summary>
    public Transform Get_Child_Transform(GameObject _Parent_Object, string _Child_Object_Name)
    {
        Transform _Result = null;

        foreach (Transform _Transform in _Parent_Object.transform)
        {
            if (_Transform.gameObject.name == _Child_Object_Name)
            {
                _Result = _Transform;
            }
            else
            {
                _Result = Get_Child_Transform(_Transform.gameObject, _Child_Object_Name);
            }

            if (_Result != null)
            {
                break;
            }
        }

        return _Result;
    }

    /// <summary>
    /// 扔出耙子
    /// </summary>
    private void Throw_Out_Rake()
    {
        Animator_Component.SetInteger("Skill_Rake", 1);

        StartCoroutine(Throw_Out_Rake(0.3f));
    }

    /// <summary>
    /// 扔出耙子
    /// </summary>
    /// <param name="_Wait_Time"></param>
    /// <returns></returns>
    private IEnumerator Throw_Out_Rake(float _Wait_Time)
    {
        yield return new WaitForSeconds(_Wait_Time);

        Chain_Object.SetActive(true);

        Weapon_Rake_Object.SetActive(false);
        Weapon_Chain_Object.SetActive(false);

        Chain_Animator_Handle_Component.Open();
        Chain_Animator_Handle_Component.Play();

        float _Unit = 10.0f;

        ArrayList _Result = null;

        float _Distance = 0.0f;

        _Result = Detection_Rake_Hit();

        if (_Result.Count < 1)
        {
            _Result.Add(null);
            _Result.Add(20);
        }

        _Distance = (int)_Result[1];

        StartCoroutine(Stop_Rake(_Distance / _Unit / 5));
    }


    /// <summary>
    /// 停止耙子
    /// </summary>
    /// <param name="_Wait_Time"></param>
    private IEnumerator Stop_Rake(float _Wait_Time)
    {
        yield return new WaitForSeconds(_Wait_Time);

        Chain_Animator_Handle_Component.Stop();

        yield return new WaitForSeconds(0.1f);

        StartCoroutine(Take_Back_Rake(_Wait_Time));
    }

    /// <summary>
    /// 收回耙子
    /// </summary>
    private IEnumerator Take_Back_Rake(float _Wait_Time)
    {
        Chain_Animator_Handle_Component.Play_Reverse();

        yield return new WaitForSeconds(_Wait_Time);

        Animator_Component.SetInteger("Skill_Rake", 2);

        Chain_Animator_Handle_Component.Close();

        Chain_Object.SetActive(false);

        Weapon_Rake_Object.SetActive(true);
        Weapon_Chain_Object.SetActive(true);
    }

    /// <summary>
    /// 检测击中
    /// </summary>
    private ArrayList Detection_Rake_Hit()
    {
        ArrayList _Result = new ArrayList();

        GameObject _Hit_Object = null;

        RaycastHit _RaycastHit;

        Vector3 _Direction = Vector3.zero;

        Vector3 _Position = Vector3.zero;

        float _Distance = 0.0f;

        _Direction = gameObject.transform.forward;

        if (Physics.Raycast(_Position, _Direction, out _RaycastHit, Detection_Layer))
        {
            _Hit_Object = _RaycastHit.collider.gameObject;

            _Distance = Vector3.Distance(_Position, _Hit_Object.transform.position);

            if (_Distance > 20.0f)
            {
                return _Result;
            }

            _Result.Add(_Hit_Object);
            _Result.Add(_Distance);
        }

        return _Result;
    }
}
