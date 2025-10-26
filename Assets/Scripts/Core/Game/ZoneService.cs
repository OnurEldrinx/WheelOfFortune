using System;
using UnityEngine;

namespace Core.Game
{ 
    public sealed class ZoneService : MonoBehaviour
    {
        [SerializeField] private int startZone = 1;
        public int CurrentZone { get; private set; }

        public event Action<int> ZoneChanged;
        
        void OnEnable() => SetCurrent(startZone,false);

        public bool IsSafeZone(int zone) => zone % 5 == 0 && zone % 30 != 0;
        public bool IsSuperZone(int zone) => zone % 30 == 0;
        public void Advance() => SetCurrent(CurrentZone + 1);
        public void Reset() => SetCurrent(startZone);
        
        private void SetCurrent(int zone, bool notify = true)
        {
            CurrentZone = Mathf.Max(1, zone);
            if (notify) ZoneChanged?.Invoke(CurrentZone);
        }
        
        public int GetNextSafeZone(int fromZone, bool strictlyGreater = true)
        {
            return NextMultipleOf(5, fromZone, strictlyGreater);
        }

        public int GetNextSuperZone(int fromZone, bool strictlyGreater = true)
        {
            return NextMultipleOf(30, fromZone, strictlyGreater);
        }

        private int NextMultipleOf(int n, int from, bool strictlyGreater)
        {
            if (from < 1) from = 1;
            if (!strictlyGreater && from % n == 0) return from;
            return ((from / n) + 1) * n;
        }
        
    }
}
