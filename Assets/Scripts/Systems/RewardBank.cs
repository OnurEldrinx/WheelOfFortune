using Data;
using UnityEngine;

namespace Systems
{
    public sealed class RewardBank
    {
        public int Total { get; private set; }
        public void Apply(SliceDef slice)
        {
            switch (slice.type)
            {
                case SliceType.Reward: Total += slice.value; break;
                case SliceType.Multiplier: Total = Mathf.RoundToInt(Total * Mathf.Max(1, slice.value)); break;
            }
        }
        public void Reset() => Total = 0;
    }
}
