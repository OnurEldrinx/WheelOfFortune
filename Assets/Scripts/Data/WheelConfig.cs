using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Wheel/Config")]
    public class WheelConfig : ScriptableObject
    {
        public string wheelName;
        public WheelType wheelType;
        public Sprite wheelSprite;
        public Sprite wheelIndicatorSprite;
        public string wheelInfo;
        public List<SliceDef> slices = new();
        public float spinDuration = 2.5f;
        public Color themeColor;
    }

    [Serializable]
    public struct SliceDef
    {
        public string id;
        public SliceType type;
        public int value;
        public Sprite icon;
    }

    public enum SliceType { Reward, Bomb, Multiplier }
    public enum WheelType { Bronze, Silver, Gold }
}
