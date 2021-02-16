using System;
using UnityEngine;
using System.Collections.Generic;

namespace MFPS.PlayerController
{
    [Serializable]
    public class MouseLook
    {
        #region Public members
        public bool clampVerticalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public bool useSmoothing = true;
        public bool lerpMovement = false;
        public float smoothTime = 5f;
        public float framesOfSmoothing = 5f;
        #endregion

        #region Private members
        private Quaternion m_CharacterTargetRot, m_CameraTargetRot;
        private bool InvertVertical, InvertHorizontal;
        private float verticalRotation, horizontalRotation;
        private float sensitivity, aimSensitivity = 3f;
        private Quaternion verticalOffset = Quaternion.identity;
        private Transform m_CameraTransform, m_CharacterBody;
        private List<float> rotArrayX = new List<float>();
        private float rotAverageX;
        private List<float> rotArrayY = new List<float>();
        private float rotAverageY;
        private bool ClampHorizontal = false;
        private Vector2 horizontalClamp = new Vector2(-360,360);
        #endregion

        public float CurrentSensitivity { get; set; } = 3;
        public bool onlyCameraTransform { get; set; } = false;

        /// <summary>
        ///  Initialize the camera controller with the character initial rotation.
        /// </summary>
        public void Init(Transform character, Transform camera, bl_GunManager gm)
        {
            m_CameraTransform = camera;
            m_CharacterBody = character;
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
            FetchSettings();
            CurrentSensitivity = sensitivity;
        }

        /// <summary>
        /// Updates the character and camera rotation based on the player input.
        /// </summary>
        public void LookRotation(Transform character, Transform camera, Transform ladder = null)
        {
#if MFPSM
            if (bl_UtilityHelper.isMobile)
            {
                CameraRotation(character, camera);
                return;
            }
#endif

            if (!bl_RoomMenu.Instance.isCursorLocked)
                return;

            if (ladder == null)
            {
                horizontalRotation = bl_GameInput.MouseX * CurrentSensitivity;
                horizontalRotation = (InvertHorizontal) ? (horizontalRotation * -1f) : horizontalRotation;

                if (useSmoothing)
                {
                    rotArrayX.Add(horizontalRotation);
                    if ((float)rotArrayX.Count >= framesOfSmoothing)
                    {
                        rotArrayX.RemoveAt(0);
                    }
                    for (int i = 0; i < rotArrayX.Count; i++)
                    {
                        rotAverageX += rotArrayX[i];
                    }
                    rotAverageX /= rotArrayX.Count;
                }
                else rotAverageX = horizontalRotation;

                m_CharacterTargetRot *= Quaternion.Euler(0f, rotAverageX, 0f);
            }
            else
            {
                Vector3 direction = ladder.forward;
                direction.y = 0;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                m_CharacterTargetRot = Quaternion.Slerp(m_CharacterTargetRot, lookRotation, Time.deltaTime * 5);
            }

            verticalRotation = bl_GameInput.MouseY * CurrentSensitivity;
            verticalRotation = (InvertVertical) ? (verticalRotation * -1f) : verticalRotation;
            if (useSmoothing)
            {
                rotArrayY.Add(verticalRotation);
                if ((float)rotArrayY.Count >= framesOfSmoothing)
                {
                    rotArrayY.RemoveAt(0);
                }
                for (int j = 0; j < rotArrayY.Count; j++)
                {
                    rotAverageY += rotArrayY[j];
                }
                rotAverageY /= rotArrayY.Count;
            }
            else rotAverageY = verticalRotation;

            if (!onlyCameraTransform)
                m_CameraTargetRot *= Quaternion.Euler(-rotAverageY, 0f, 0f);
            else
                m_CameraTargetRot *= Quaternion.Euler(-rotAverageY, horizontalRotation, 0f);

            if (clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            if (ClampHorizontal)
            {
                var re = m_CharacterTargetRot.eulerAngles;
                re.y = bl_UtilityHelper.Clamp360Angle(re.y, horizontalClamp.x, horizontalClamp.y);
                m_CharacterTargetRot = Quaternion.Euler(re);
            }

            if (lerpMovement)
            {
                if (character != null && !onlyCameraTransform) { character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot, smoothTime * Time.deltaTime); }
                if (camera != null) { camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot * verticalOffset, smoothTime * Time.deltaTime); }
            }
            else
            {
                if (!onlyCameraTransform) { character.localRotation = m_CharacterTargetRot; }
                camera.localRotation = m_CameraTargetRot * verticalOffset;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetVerticalOffset(float amount)
        {
            verticalOffset = Quaternion.Euler(amount, 0, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        public void CombineVerticalOffset()
        {
            m_CameraTargetRot *= verticalOffset;
            verticalOffset = Quaternion.identity;
            if (clampVerticalRotation)
            {
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);
            }
        }

        /// <summary>
        /// Don't rotate the character body but only the Camera/Head
        /// </summary>
        public void UseOnlyCameraRotation()
        {
            onlyCameraTransform = true;
        }

        /// <summary>
        /// Port the Current Camera Rotation to separate the vertical and horizontal rotation in the body and head
        /// horizontal rotation for the body and vertical for the camera/head
        /// That should only be called when OnlyCameraRotation was used before.
        /// </summary>
        public void PortBodyOrientationToCamera()
        {
            onlyCameraTransform = false;
            Vector3 direction = Vector3.zero;
            direction.y = m_CameraTransform.eulerAngles.y;
            m_CharacterBody.rotation = Quaternion.Euler(direction);

            direction = Vector3.zero;
            direction.x = m_CameraTransform.localEulerAngles.x;
            m_CameraTransform.localRotation = Quaternion.Euler(direction);
            m_CharacterTargetRot = m_CharacterBody.localRotation;
            m_CameraTargetRot = m_CameraTransform.localRotation;
        }

        /// <summary>
        /// Forces the character to look at a position in the world.
        /// </summary>
        public void LookAt(Transform reference, bool extrapolate = true)
        {
            m_CharacterTargetRot = Quaternion.Euler(0f, reference.eulerAngles.y, 0f);
            Quaternion relative = Quaternion.Inverse(Quaternion.identity) * reference.rotation;
            m_CameraTargetRot = Quaternion.Euler(relative.eulerAngles.x, 0, 0);

            if (extrapolate)
            {
                m_CharacterBody.localRotation = m_CharacterTargetRot;
                m_CameraTransform.localRotation = m_CameraTargetRot;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void FetchSettings()
        {
            sensitivity = (float)bl_MFPS.Settings.GetSettingOf("Sensitivity");
            aimSensitivity = (float)bl_MFPS.Settings.GetSettingOf("Aim Sensitivity");
            InvertHorizontal = (bool)bl_MFPS.Settings.GetSettingOf("MouseH Invert");
            InvertHorizontal = (bool)bl_MFPS.Settings.GetSettingOf("MouseV Invert");
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnAimChange(bool isAiming)
        {
            CurrentSensitivity = isAiming ? sensitivity * aimSensitivity : sensitivity;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClampHorizontalRotation(float min, float max)
        {
            horizontalClamp = new Vector2(min, max);
            ClampHorizontal = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void UnClampHorizontal() => ClampHorizontal = false;

#if MFPSM
        void CameraRotation(Transform character, Transform camera)
        {
            Vector2 input = bl_TouchPad.Instance.GetInput(CurrentSensitivity);
            input.x = !InvertHorizontal ? input.x : (input.x * -1f);
            input.y = !InvertVertical ? (input.y * -1f) : input.y;

            m_CharacterTargetRot *= Quaternion.Euler(0f, input.x, 0f);
            m_CameraTargetRot *= Quaternion.Euler(input.y, 0f, 0f);

            if (clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            camera.localRotation = m_CameraTargetRot;
            character.localRotation = m_CharacterTargetRot;
        }
#endif

        public float VerticalAngle => m_CameraTransform.localEulerAngles.x;

        Quaternion ClampRotationAroundXAxis(Quaternion q) => bl_UtilityHelper.ClampRotationAroundAxis(q, MinimumX, MaximumX, UnityEngine.Animations.Axis.X);
        Quaternion ClampRotationAroundYAxis(Quaternion q) => bl_UtilityHelper.ClampRotationAroundAxis(q, horizontalClamp.x, horizontalClamp.y, UnityEngine.Animations.Axis.Y);
    }
}