using System.Collections.Generic;
using Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public sealed class InventoryView : MonoBehaviour
    {
        [SerializeField] private Transform contentRoot;      
        [SerializeField] private GameObject entryPrefab;        

        private readonly List<GameObject> pool = new();
        public readonly Dictionary<string,Image> imageRefMap = new();
        public void Render(List<RewardInventory.Entry> entries)
        {
            EnsurePool(entries.Count);
            int i=0;
            imageRefMap.Clear();
            foreach (var e in entries)
            {
                var go = pool[i++];
                go.SetActive(true);
                var img = go.GetComponentInChildren<Image>(true);
                var txt = go.GetComponentInChildren<TMP_Text>(true);
                if (img)
                {
                    imageRefMap.TryAdd(e.Id.Trim(), img);
                    img.sprite = e.Icon;
                }
                if (txt) txt.text = "x" + e.Count;
            }
            for (; i<pool.Count; i++) pool[i].SetActive(false);
        }

        private void EnsurePool(int needed)
        {
            while (pool.Count < needed)
            {
                var go = Instantiate(entryPrefab, contentRoot);
                pool.Add(go);
            }
        }
    }
}
