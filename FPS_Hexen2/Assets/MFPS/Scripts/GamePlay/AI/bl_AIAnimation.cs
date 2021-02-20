﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class bl_AIAnimation : bl_MonoBehaviour
{
    [Header("Head Look")]
    [SerializeField,Range(0,1)] private float Weight = 0.8f;
    [SerializeField, Range(0, 1)] private float Body = 0.9f;
    [SerializeField, Range(0, 1)] private float Head = 1;
    [Header("Ragdoll")]
    public List<Rigidbody> mRigidBody = new List<Rigidbody>();
    public List<bl_AIHitBox> HitBoxes = new List<bl_AIHitBox>();

    private Animator m_animator;
    private Vector3 localVelocity;
    private float vertical;
    private float horizontal;
    private float turnSpeed;
    private bool parent = false;
    private Vector3 velocity;
    private float lastYRotation;
    private float TurnLerp;
    private float movementSpeed;
    private bl_AIShooterHealth AIHealth;
    private Vector3 headTarget;
    private Dictionary<string, int> animatorHashes;
    private bl_AIShooterReferences references;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        references = transform.root.GetComponent<bl_AIShooterReferences>();
        m_animator = GetComponent<Animator>();
        AIHealth = references.shooterHealth;
        SetKinecmatic();
        if (animatorHashes == null)
        {
            FetchHashes();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void FetchHashes()
    {
        //cache the hashes in a Array will be more appropriate but to be more readable for other users
        // I decide to cached them in a Dictionary with the key name indicating the parameter that contain
        animatorHashes = new Dictionary<string, int>();
        animatorHashes.Add("Vertical", Animator.StringToHash("Vertical"));
        animatorHashes.Add("Horizontal", Animator.StringToHash("Horizontal"));
        animatorHashes.Add("Speed", Animator.StringToHash("Speed"));
        animatorHashes.Add("Turn", Animator.StringToHash("Turn"));
        animatorHashes.Add("isGround", Animator.StringToHash("isGround"));
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        ControllerInfo();
        Animate();
    }

    /// <summary>
    /// 
    /// </summary>
    void ControllerInfo()
    {
        velocity = references.shooterNetwork.Velocity;
        float delta = Time.deltaTime;
        localVelocity = transform.InverseTransformDirection(velocity);
        localVelocity.y = 0;

        vertical = Mathf.Lerp(vertical, localVelocity.z, delta * 10);
        horizontal = Mathf.Lerp(horizontal, localVelocity.x, delta * 10);

        parent = !parent;
        if (parent)
        {
            lastYRotation = transform.rotation.eulerAngles.y;
        }
        turnSpeed = Mathf.DeltaAngle(lastYRotation, transform.rotation.eulerAngles.y);
        TurnLerp = Mathf.Lerp(TurnLerp, turnSpeed, 7 * delta);
        movementSpeed = velocity.magnitude;
    }

    /// <summary>
    /// 
    /// </summary>
    void Animate()
    {
        if (m_animator == null)
            return;

        m_animator.SetFloat(animatorHashes["Vertical"], vertical);
        m_animator.SetFloat(animatorHashes["Horizontal"], horizontal);
        m_animator.SetFloat(animatorHashes["Speed"], movementSpeed);
        m_animator.SetFloat(animatorHashes["Turn"], TurnLerp);
        m_animator.SetBool(animatorHashes["isGround"], true);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnAnimatorIK(int layerIndex)
    {
        if (layerIndex == 1)
        {
            m_animator.SetLookAtWeight(Weight, Body, Head, 1, 0.4f);
            headTarget = Vector3.Slerp(headTarget, references.aiShooter.LookAtPosition, Time.deltaTime * 3);
            m_animator.SetLookAtPosition(headTarget);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetKinecmatic()
    {
        for(int i = 0; i< HitBoxes.Count; i++)
        {
            mRigidBody[i].isKinematic = true;
        }
    }

    public void Ragdolled(Vector3 from, bool isExplosion = false)
    {
        m_animator.enabled = false;
        Vector3 hitPosition = HitBoxes[AIHealth.lastHitBoxHitted].transform.position;
        for (int i = 0; i < HitBoxes.Count; i++)
        {
            mRigidBody[i].isKinematic = false;
            mRigidBody[i].detectCollisions = true;
            Vector3 rhs = transform.position - from;
            mRigidBody[i].AddForceAtPosition(rhs.normalized * 150, hitPosition);
            if (isExplosion)
            {
                mRigidBody[i].AddExplosionForce(875, from, 7);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnGetHit()
    {
        int r = Random.Range(0, 2);
        string hit = (r == 1) ? "Right Hit" : "Left Hit";
        m_animator.Play(hit, 2, 0);
    }

#if UNITY_EDITOR
    [ContextMenu("Get RigidBodys")]
#endif
    public void GetRigidBodys()
    {
        Rigidbody[] R = this.transform.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in R)
        {
            if (!mRigidBody.Contains(rb))
            {
                mRigidBody.Add(rb);
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Set Scripts")]
#endif
    public void SetScripts()
    {
        for(int i = 0; i < mRigidBody.Count; i++)
        {
            mRigidBody[i].gameObject.tag = "AI";
            bl_AIHitBox box = null;
            if(mRigidBody[i].gameObject.GetComponent<bl_AIHitBox>() == null)
            {
                box = mRigidBody[i].gameObject.AddComponent<bl_AIHitBox>();
            }else
            {
                box = mRigidBody[i].gameObject.GetComponent<bl_AIHitBox>();
            }
            box.AI = transform.parent.GetComponent<bl_AIShooterHealth>();
            box.m_Collider = mRigidBody[i].gameObject.GetComponent<Collider>();
            box.isHead = gameObject.name.ToLower().Contains("head");
            HitBoxes.Add(box);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        for(int i = 0; i < HitBoxes.Count; i++)
        {
            if (HitBoxes[i] == null) continue;
            HitBoxes[i].ID = i;
        }
    }
#endif
}