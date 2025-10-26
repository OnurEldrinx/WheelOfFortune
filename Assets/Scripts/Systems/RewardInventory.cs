using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.UI;

namespace Systems
{
    public sealed class RewardInventory
    {
        private readonly Dictionary<string, Entry> map = new();

        public struct Entry
        {
            public string Id;
            public Sprite Icon;
            public int Count;
            public Image IconRenderer;
        }

        public void Add(SliceDef slice)
        {
            if (slice.type != SliceType.Reward) return; // Only additive rewards
            var key = string.IsNullOrEmpty(slice.id) ? "_anon" : slice.id;
            if (!map.TryGetValue(key, out var e)) e = new Entry { Id = key, Icon = slice.icon, Count = 0 };
            e.Count += slice.value;
            e.Icon = slice.icon ? slice.icon : e.Icon;
            map[key] = e;
        }

        public void Reset() => map.Clear();
        public List<Entry> Snapshot() => new(map.Values);
    }
}