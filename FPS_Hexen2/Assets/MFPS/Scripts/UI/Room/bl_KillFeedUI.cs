using UnityEngine;
using MFPS.Internal.Structures;
using MFPS.Runtime.UI.Bindings;

namespace MFPS.Runtime.UI
{
    public class bl_KillFeedUI : MonoBehaviour
    {
        public int numberOfPooledPrefabs = 6;
        public Transform KillfeedPanel;
        public GameObject KillfeedPrefab;

        private bl_KillFeedUIBinding[] pool;
        private int currentPooled = 0;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            PreparePool();
        }

        /// <summary>
        /// 
        /// </summary>
        void PreparePool()
        {
            if (pool != null) return;

            pool = new bl_KillFeedUIBinding[numberOfPooledPrefabs];
            for (int i = 0; i < numberOfPooledPrefabs; i++)
            {
                var obj = Instantiate(KillfeedPrefab) as GameObject;
                obj.transform.SetParent(KillfeedPanel, false);
                pool[i] = obj.GetComponent<bl_KillFeedUIBinding>();
                obj.SetActive(false);
            }
            KillfeedPrefab.SetActive(false);
        }

        /// <summary>
        /// Global notification (on right corner) when kill someone
        /// </summary>
        public void SetKillFeed(KillFeed feed)
        {
            if (!bl_UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.KillFeed)) return;

            if (pool == null) PreparePool();

            var newkillfeed = pool[currentPooled];
            newkillfeed.Init(feed);
            newkillfeed.transform.SetParent(KillfeedPanel, false);
            newkillfeed.transform.SetAsFirstSibling();

            currentPooled = (currentPooled + 1) % numberOfPooledPrefabs;
        }

        private static bl_KillFeedUI _instance;
        public static bl_KillFeedUI Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<bl_KillFeedUI>(); }
                return _instance;
            }
        }
    }
}