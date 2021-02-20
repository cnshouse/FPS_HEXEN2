using UnityEngine;

namespace MFPS.Addon.Customizer
{
    public class bl_AttachmentsButtons : MonoBehaviour
    {

        public bl_AttachButton ActiveButton;

        [Header("Buttons")]
        public bl_AttachButton Barrel;
        public bl_AttachButton Optics;
        public bl_AttachButton Feeder;
        public bl_AttachButton Cylinder;
        public bl_AttachButton CamoButton;

        private bl_Customizer cacheCustomizer;

        public void Init(bl_Customizer customizer)
        {
            cacheCustomizer = customizer;
            if (Barrel != null && cacheCustomizer.Positions.BarrelPosition != null)
                Barrel.transform.position = cacheCustomizer.Positions.BarrelPosition.position;
            if (Optics != null && cacheCustomizer.Positions.OpticPosition != null)
                Optics.transform.position = cacheCustomizer.Positions.OpticPosition.position;
            if (Feeder != null && cacheCustomizer.Positions.FeederPosition != null)
                Feeder.transform.position = cacheCustomizer.Positions.FeederPosition.position;
            if (Cylinder != null && cacheCustomizer.Positions.CylinderPosition != null)
                Cylinder.transform.position = cacheCustomizer.Positions.CylinderPosition.position;

            Feeder.gameObject.SetActive(customizer.Attachments.Foregrips.Count > 0);

            Active(false);
        }

        public void Active(bool active)
        {
            if (Barrel != null)
                Barrel.GetComponent<Renderer>().enabled = active;
            if (Optics != null)
                Optics.GetComponent<Renderer>().enabled = active;
            if (Feeder != null)
                Feeder.GetComponent<Renderer>().enabled = active;
            if (Cylinder != null)
                Cylinder.GetComponent<Renderer>().enabled = active;
            if (CamoButton != null)
                CamoButton.GetComponent<Renderer>().enabled = active;
        }

        public void LookAt(Transform t)
        {
            if (Barrel != null)
                Barrel.transform.LookAt(t);
            if (Optics != null)
                Optics.transform.LookAt(t);
            if (Feeder != null)
                Feeder.transform.LookAt(t);
            if (Cylinder != null)
                Cylinder.transform.LookAt(t);
            if (CamoButton != null)
                CamoButton.transform.LookAt(t);
        }

        public void SetActiveButton(bl_AttachButton t)
        {

            if (t == null)
            {
                if (ActiveButton != null)
                {
                    ActiveButton.Active(false);
                    ActiveButton = null;
                }
                return;
            }

            if (ActiveButton != null)
            {
                ActiveButton.Active(false);
            }

            ActiveButton = t;
            ActiveButton.Active(true);
        }
    }
}