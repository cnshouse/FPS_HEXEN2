using UnityEngine;

[ExecuteInEditMode]
public class bl_PlayerIK : bl_MonoBehaviour
{
    #region Public members
    public Transform Target;
    [Header("UPPER BODY")]
    [Range(0, 1)] public float Weight;
    [Range(0, 1)] public float Body;
    [Range(0, 1)] public float Head;
    [Range(0, 1)] public float Eyes;
    [Range(0, 1)] public float Clamp;
    [Range(1, 20)] public float Lerp = 8;

    public Vector3 HandOffset;
    public Vector3 AimSightPosition = new Vector3(0.02f, 0.19f, 0.02f);

    [Header("FOOT IK")]
    public bool useFootPlacement = true;
    public LayerMask FootLayers;
    [Range(0.01f, 2)] public float FootDownOffset = 1.25f;
    [Range(0.1f, 1)] public float FootHeight = 0.43f;
    [Range(0.1f, 1)] public float PositionWeight = 1;
    [Range(0.1f, 1)] public float RotationWeight = 1;
    [Range(-0.5f, 0.5f)] public float TerrainOffset = 0.13f;
    [Range(-0.5f, 0.5f)] public float Radious = 0.125f;

    public bl_BodyIKHandler CustomArmsIKHandler { get; set; }
    public bool IsCustomHeadTarget { get; set; } = false;
    #endregion

    #region Private members
    private Transform RightFeed;
    private Transform LeftFeed;
    private float leftWeight = 0;
    private float rightWeight = 0;
    private float leftRotationWeight = 0;
    private float rightRotationWeight = 0;
    private Animator animator;
    private Vector3 targetPosition;
    private float RighHand = 1;
    private float LeftHand = 1;
    private float RightHandPos = 0;
    private Transform m_headTransform, rightUpperArm;
    private bl_PlayerAnimations PlayerAnimation;
    private float deltaTime = 0;
    private Transform m_headTarget;
    private Transform m_leftArmRef;
    private Transform oldLHTarget;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if (playerReferences == null) return;

        PlayerAnimation = playerReferences.playerAnimations;
        animator = GetComponent<Animator>();
        m_leftArmRef = playerReferences.leftArmTarget.transform;
        if (HeadLookTarget == null) HeadLookTarget = Target;

        m_headTransform = animator.GetBoneTransform(HumanBodyBones.Head);
        rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        if (useFootPlacement)
        {
            LeftFeed = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            RightFeed = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        }
    }

    /// <summary>
    /// Called from the Animator after the animation update
    /// </summary>
    void OnAnimatorIK(int layer)
    {
        if (HeadLookTarget == null || animator == null)
            return;

        deltaTime = Time.deltaTime;
        if (layer == 0) BottomBody();
        else if (layer == 1) UpperBody();
    }

    /// <summary>
    /// Control the legs IK
    /// </summary>
    void BottomBody()
    {
        animator.SetLookAtWeight(Weight, Body, Head, Eyes, Clamp);
        targetPosition = Vector3.Slerp(targetPosition, HeadLookTarget.position, deltaTime * 8);
        animator.SetLookAtPosition(targetPosition);

        if (useFootPlacement && PlayerAnimation.grounded && PlayerAnimation.localVelocity.magnitude <= 0.1f)
        {
            LegsIK();
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
        }
    }

    /// <summary>
    /// Control the arms and head IK bones
    /// </summary>
    void UpperBody()
    {
        //If there's another script handling the arms IK
        if (CustomArmsIKHandler != null)
        {
            CustomArmsIKHandler.OnUpdate();
        }
        else if (LeftHandTarget != null && !PlayerAnimation.isWeaponsBlocked && !PlayerAnimation.stopHandsIK)
        {
            ArmsIK();
        }
        else
        {
            ResetWeightIK();
        }
    }

    /// <summary>
    /// Control left and right arms
    /// </summary>
    void ArmsIK()
    {
        //control the arms only when the player is aiming or firing
        float weight = (inPointMode) ? 1 : 0;
        float lweight = (PlayerSync.FPState != PlayerFPState.Running) ? 1 : 0;
        RighHand = Mathf.Lerp(RighHand, lweight, deltaTime * 5);
        LeftHand = Mathf.Lerp(LeftHand, weight, deltaTime * 5);

        animator.SetIKRotation(AvatarIKGoal.LeftHand, m_leftArmRef.rotation);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, m_leftArmRef.position);

        if (RighHand > 0)
        {
            //Make the right arm aim where the player is looking at
            Vector3 rhs = HeadLookTarget.position - rightUpperArm.position;
            Quaternion lookAt = Quaternion.LookRotation(rhs);
            Vector3 v = lookAt.eulerAngles;
            v = v + HandOffset;
            animator.SetIKRotation(AvatarIKGoal.RightHand, Quaternion.Euler(v));
        }

        float rpw = (PlayerSync.FPState == PlayerFPState.Aiming || PlayerSync.FPState == PlayerFPState.FireAiming) ? 0.5f : 0;
        RightHandPos = Mathf.Lerp(RightHandPos, rpw, deltaTime * 7);
        Vector3 hf = m_headTransform.TransformPoint(AimSightPosition);
        animator.SetIKPosition(AvatarIKGoal.RightHand, hf);

        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, RighHand);
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, RightHandPos);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, LeftHand);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, LeftHand);
    }

    /// <summary>
    /// 
    /// </summary>
    void LegsIK()
    {
        RaycastHit lr;
        if (Physics.SphereCast(LeftFeed.position + (Vector3.up * FootHeight), Radious, (Vector3.down * FootHeight), out lr, FootDownOffset * FootHeight, FootLayers))
        {
            Vector3 pos = lr.point;
            pos.y += TerrainOffset;
            Quaternion rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, lr.normal), lr.normal);
            animator.SetIKPosition(AvatarIKGoal.LeftFoot, pos);
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, rotation);
            leftWeight = PositionWeight;
            leftRotationWeight = RotationWeight;
        }
        else
        {
            leftWeight = Mathf.Lerp(leftWeight, 0, deltaTime * 4);
            leftRotationWeight = Mathf.Lerp(leftRotationWeight, 0, deltaTime * 4);
        }

        RaycastHit rr;
        if (Physics.SphereCast(RightFeed.position + (Vector3.up * FootHeight), Radious, (Vector3.down * FootHeight), out rr, FootDownOffset * FootHeight, FootLayers))
        {
            Vector3 pos = rr.point;
            pos.y += TerrainOffset;
            Quaternion rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, rr.normal), rr.normal);
            animator.SetIKPosition(AvatarIKGoal.RightFoot, pos);
            animator.SetIKRotation(AvatarIKGoal.RightFoot, rotation);
            rightWeight = PositionWeight;
            rightRotationWeight = RotationWeight;
        }
        else
        {
            rightWeight = Mathf.Lerp(rightWeight, 0, deltaTime * 4);
            rightRotationWeight = 0;
        }

        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftRotationWeight);
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightRotationWeight);
    }

    /// <summary>
    /// 
    /// </summary>
    void ResetWeightIK()
    {
        LeftHand = 0;
        RighHand = 0;
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.0f);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0.0f);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
    }

    /// <summary>
    /// 
    /// </summary>
    public Transform HeadLookTarget
    {
        get
        {
            if (m_headTarget == null)
            {
                m_headTarget = Target;
                IsCustomHeadTarget = false;
            }
            return m_headTarget;
        }
        set
        {
            if (value == null)
            {
                m_headTarget = Target;
                IsCustomHeadTarget = false;
            }
            else
            {
                m_headTarget = value;
                IsCustomHeadTarget = true;
            }
        }
    }

    /// <summary>
    /// If the player in an state where the arms should be controlled by IK
    /// </summary>
    private bool inPointMode
    {
        get
        {
            return (PlayerSync.FPState != PlayerFPState.Running && PlayerSync.FPState != PlayerFPState.Reloading);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    private Transform LeftHandTarget
    {
        get
        {
            if (Application.isPlaying)
            {
                if (PlayerSync != null && PlayerSync.CurrenGun != null)
                {
                    CompareLeftArmTarget(PlayerSync.CurrenGun.LeftHandPosition);
                    return PlayerSync.CurrenGun.LeftHandPosition;
                }
            }
            else//called from an editor script to simulate IK in editor
            {
                if (PlayerSync != null && PlayerSync.m_PlayerAnimation != null && PlayerSync.m_PlayerAnimation.EditorSelectedGun)
                {
                    if (m_leftArmRef == null) m_leftArmRef = playerReferences.leftArmTarget.transform;

                    CompareLeftArmTarget(PlayerSync.m_PlayerAnimation.EditorSelectedGun.LeftHandPosition);
                    return PlayerSync.m_PlayerAnimation.EditorSelectedGun.LeftHandPosition;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="laTarget"></param>
    private void CompareLeftArmTarget(Transform laTarget)
    {
        if (oldLHTarget != laTarget)
        {
            oldLHTarget = laTarget;
            while (playerReferences.leftArmTarget.sourceCount > 0)
                playerReferences.leftArmTarget.RemoveSource(0);

            if (playerReferences.leftArmTarget != null && oldLHTarget != null)
            {
                playerReferences.leftArmTarget.transform.position = oldLHTarget.position;
                playerReferences.leftArmTarget.transform.rotation = oldLHTarget.rotation;
                playerReferences.leftArmTarget.AddSource(new UnityEngine.Animations.ConstraintSource()
                {
                    sourceTransform = oldLHTarget,
                    weight = 1
                });
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 
    /// </summary>
    private void OnDrawGizmos()
    {
        if (animator == null) { animator = GetComponent<Animator>(); }
        if (m_headTransform == null) { m_headTransform = animator.GetBoneTransform(HumanBodyBones.Head); }
        Gizmos.color = Color.yellow;
        Vector3 hf = m_headTransform.TransformPoint(AimSightPosition);
        Gizmos.DrawLine(m_headTransform.position, hf);
        Gizmos.DrawSphere(hf, 0.03f);

    }
#endif

    private bl_PlayerReferences m_playerReferences;
    private bl_PlayerReferences playerReferences
    {
        get
        {
            if (m_playerReferences == null) m_playerReferences = transform.GetComponentInParent<bl_PlayerReferences>();
            return m_playerReferences;
        }
    }

    private bl_PlayerNetwork PlayerSync
    {
        get
        {
            if (playerReferences != null)
                return playerReferences.playerNetwork;

            return null;
        }
    }
}