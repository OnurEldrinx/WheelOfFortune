using Core.Game;

namespace Core.Risk
{
    public interface IRiskStrategy
    {
        bool Applies(int zone, ZoneService zones);
    }

    public sealed class SafeZoneStrategy : IRiskStrategy
    {
        public bool Applies(int zone, ZoneService z) => z.IsSafeZone(zone);
    }

    public sealed class SuperZoneStrategy : IRiskStrategy
    {
        public bool Applies(int zone, ZoneService z) => z.IsSuperZone(zone);
    }

    public sealed class DefaultRiskStrategy : IRiskStrategy
    {
        public bool Applies(int zone, ZoneService z) => !z.IsSafeZone(zone) && !z.IsSuperZone(zone);
    }

    public sealed class RiskPolicy
    {
        private readonly IRiskStrategy safe, super, normal;

        public RiskPolicy(IRiskStrategy safe, IRiskStrategy super, IRiskStrategy normal)
        {
            this.safe = safe;
            this.super = super;
            this.normal = normal;
        }

        public bool IsSafe(int zone, ZoneService z) => safe.Applies(zone, z) || super.Applies(zone, z);
    }
}