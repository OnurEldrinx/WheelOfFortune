using System.Collections.Generic;
using Core.Game;
using Data;
using UnityEngine;

namespace Core.Wheel
{
    public interface IWheelModifier
    {
        void Apply(int zone, ZoneService zones, WheelConfig cfg);
    }

    public sealed class WheelConfigFactory
    {
        private readonly Dictionary<WheelType, WheelConfig> wheelConfigs;
        private readonly List<IWheelModifier> modifiers = new();

        public WheelConfigFactory(Dictionary<WheelType, WheelConfig> wheelConfigs)
        {
            this.wheelConfigs = wheelConfigs;
        }

        public WheelConfigFactory AddModifier(IWheelModifier m)
        {
            modifiers.Add(m);
            return this;
        }

        public WheelConfig BuildFor(ZoneService zones)
        {
            var zone = zones.CurrentZone;
            WheelConfig prototype;

            if (zones.IsSafeZone(zone))
            {
                var newConfig = wheelConfigs[WheelType.Silver];
                prototype = newConfig;
            }
            else if (zones.IsSuperZone(zone))
            {
                var newConfig = wheelConfigs[WheelType.Gold];
                prototype = newConfig;
            }
            else
            {
                var newConfig = wheelConfigs[WheelType.Bronze];
                prototype = newConfig;
            }

            var cfg = Object.Instantiate(prototype); // clone
            //var cloned = new Data.WheelConfig { spinDuration = cfg.spinDuration, slices = new System.Collections.Generic.List<Data.SliceDef>(cfg.slices) };
            var cloned = ScriptableObject.CreateInstance<WheelConfig>();
            cloned.spinDuration = cfg.spinDuration;
            cloned.slices = new List<SliceDef>(cfg.slices);
            cloned = cfg;
            foreach (var m in modifiers) m.Apply(zone, zones, cloned);
            return cloned;
        }
    }

    public sealed class ScaleRewardsByTier : IWheelModifier
    {
        private readonly float step;
        private readonly int tierSpan;

        public ScaleRewardsByTier(float step = 0.15f, int tierSpan = 5)
        {
            this.step = step;
            this.tierSpan = tierSpan;
        }

        public void Apply(int zone, ZoneService zones, WheelConfig cfg)
        {
            float mul = 1f + Mathf.Floor((zone - 1) / (float)tierSpan) * step;
            for (int i = 0; i < cfg.slices.Count; i++)
            {
                var s = cfg.slices[i];
                if (s.type == SliceType.Reward || s.type == SliceType.Multiplier)
                {
                    s.value = Mathf.RoundToInt(s.value * mul);
                    cfg.slices[i] = s;
                }
            }
        }
    }
}