using System;
using System.Collections.Generic;
using Core.Game;
using Core.Risk;
using Core.Wheel;
using Data;
using DG.Tweening;
using Infra;
using Systems;
using UI;
using UnityEngine;

namespace Core.State
{
    public interface IGameState
    {
        void Enter();
        void Exit();
    }

    public sealed class GameStateMachine
    {
        private readonly EventBus bus;
        private readonly ZoneService zones;
        private readonly RewardBank rewards;
        private readonly RewardInventory inventory;
        private readonly WheelView wheel;
        private readonly WheelConfigFactory factory;
        private readonly RiskPolicy risk;
        private readonly HUDPresenter hud;
        private IGameState current;

        public GameStateMachine(
            EventBus bus, ZoneService zones, RewardBank rewards, RewardInventory inventory,
            WheelView wheel, WheelConfigFactory factory, RiskPolicy risk, HUDPresenter hud)
        {
            this.bus = bus;
            this.zones = zones;
            this.rewards = rewards;
            this.inventory = inventory;
            this.wheel = wheel;
            this.factory = factory;
            this.risk = risk;
            this.hud = hud;
        }

        public void Enter<T>(WheelConfig cfg = null) where T : IGameState => Set(Create<T>(cfg));

        private void Set(IGameState next)
        {
            current?.Exit();
            current = next;
            current.Enter();
        }

        private T Create<T>(WheelConfig cfg) where T : IGameState
        {
            if (typeof(T) == typeof(IdleState))
                return (T)(IGameState)new IdleState(this, zones, rewards, inventory, hud, factory,wheel);
            if (typeof(T) == typeof(ReadyState))
                return (T)(IGameState)new ReadyState(this, zones, rewards, inventory, hud, factory, risk, wheel);
            if (typeof(T) == typeof(SpinningState))
                return (T)(IGameState)new SpinningState(this, zones, rewards, inventory, hud, wheel, factory, cfg);
            if (typeof(T) == typeof(LostState))
                return (T)(IGameState)new LostState(this, zones, rewards, inventory, hud);
            if (typeof(T) == typeof(CollectedState))
                return (T)(IGameState)new CollectedState(this, zones, rewards, inventory, hud);
            throw new NotSupportedException(typeof(T).Name);
        }
    }

    // ---------- Concrete States ----------
    public sealed class IdleState : IGameState
    {
        private readonly GameStateMachine fsm;
        private readonly ZoneService z;
        private readonly RewardBank r;
        private readonly RewardInventory inv;
        private readonly HUDPresenter hud;
        private readonly WheelConfigFactory factory;
        private readonly WheelView wheel;
        public IdleState(GameStateMachine fsm, ZoneService z, RewardBank r, RewardInventory inv,
            HUDPresenter hud,WheelConfigFactory factory, WheelView wheel)
        {
            this.fsm = fsm;
            this.z = z;
            this.r = r;
            this.inv = inv;
            this.hud = hud;
            this.factory = factory;
            this.wheel = wheel;
        }

        public void Enter()
        {
            z.Reset();
            r.Reset();
            inv.Reset();
            hud.ShowTotals(z.CurrentZone, canCashOut: z.IsSafeZone(z.CurrentZone) || z.IsSuperZone(z.CurrentZone));
            hud.ShowInventory(inv.Snapshot());
            var cfg = factory.BuildFor(z);
            wheel.UpdateConfig(cfg);
            hud.Bind(
                new SpinCommand(() => fsm.Enter<SpinningState>()), 
                new CashOutCommand(() => fsm.Enter<CollectedState>()), 
                new GiveUpCommand(()=>fsm.Enter<IdleState>()),
                z);
        }

        public void Exit()
        {
        }
    }

    public sealed class ReadyState : IGameState
    {
        private readonly GameStateMachine fsm;
        private readonly ZoneService z;
        private readonly RewardBank r;
        private readonly RewardInventory inv;
        private readonly HUDPresenter hud;
        private readonly WheelConfigFactory factory;
        private readonly RiskPolicy risk;
        private readonly WheelView wheel;

        public ReadyState(GameStateMachine fsm, ZoneService z, RewardBank r, RewardInventory inv,
            HUDPresenter hud, WheelConfigFactory factory, RiskPolicy risk, WheelView wheel)
        {
            this.fsm = fsm;
            this.z = z;
            this.r = r;
            this.inv = inv;
            this.hud = hud;
            this.factory = factory;
            this.risk = risk;
            this.wheel = wheel;
        }

        public void Enter()
        {
            hud.ShowTotals(z.CurrentZone, canCashOut: z.IsSafeZone(z.CurrentZone) || z.IsSuperZone(z.CurrentZone));
            hud.ShowInventory(inv.Snapshot());
            var cfg = factory.BuildFor(z);
            var animate = z.IsSafeZone(z.CurrentZone) || z.IsSuperZone(z.CurrentZone) ||
                          z.IsSafeZone(z.CurrentZone - 1) || z.IsSuperZone(z.CurrentZone - 1);
            wheel.UpdateConfig(cfg, animate);
            hud.Bind(
                new SpinCommand(() => fsm.Enter<SpinningState>(cfg)),
                new CashOutCommand(() => fsm.Enter<CollectedState>()), 
                new GiveUpCommand(()=>fsm.Enter<IdleState>()),
                z);
        }

        public void Exit()
        {
        }
    }

    public sealed class SpinningState : IGameState
    {
        private readonly GameStateMachine fsm;
        private readonly ZoneService z;
        private readonly RewardBank r;
        private readonly RewardInventory inv;
        private readonly HUDPresenter hud;
        private readonly WheelView wheel;
        private readonly WheelConfigFactory factory;
        private readonly IRandom rng = new DefaultRandom();
        private float rotationValue;
        private WheelConfig currentConfig;

        public SpinningState(GameStateMachine fsm, ZoneService z, RewardBank r, RewardInventory inv,
            HUDPresenter hud, WheelView wheel, WheelConfigFactory factory, WheelConfig currentConfig)
        {
            this.fsm = fsm;
            this.z = z;
            this.r = r;
            this.inv = inv;
            this.hud = hud;
            this.wheel = wheel;
            this.factory = factory;
            this.currentConfig = currentConfig;
        }

        public void Enter()
        {
            hud.SetButtonsEnabled(false);
            if (!currentConfig)
            {
                currentConfig = factory.BuildFor(z);
                wheel.UpdateConfig(currentConfig);
            }

            currentConfig.slices =
                new List<SliceDef>(Utils.ShiftArray(currentConfig.slices.ToArray(), (int)(rotationValue / 45f)));
            var idx = rng.Range(0, currentConfig.slices.Count);
            Debug.Log($"{currentConfig.slices[idx].id},Index={idx}");
            rotationValue = wheel.SpinTo(idx, currentConfig.spinDuration, () => OnStop(currentConfig, idx));
        }

        private void OnStop(WheelConfig cfg, int idx)
        {
            var slice = cfg.slices[idx];
            if (slice.type == SliceType.Bomb)
            {
                r.Reset();
                inv.Reset();
                hud.ShowInventory(inv.Snapshot());
                fsm.Enter<LostState>();
            }
            else
            {
                r.Apply(slice);
                inv.Add(slice);
                hud.ShowInventory(inv.Snapshot());
                z.Advance();
                hud.ShowTotals(z.CurrentZone, canCashOut: z.IsSafeZone(z.CurrentZone) || z.IsSuperZone(z.CurrentZone));
                hud.SetButtonsEnabled(true);
                DOVirtual.DelayedCall(0.25f, () =>
                {
                    hud.NotifyRewardFlowAnimation(slice);
                    fsm.Enter<ReadyState>();
                });
                
                
            }
        }

        public void Exit()
        {
        }
    }

    public sealed class LostState : IGameState
    {
        private readonly GameStateMachine fsm;
        private readonly ZoneService z;
        private readonly RewardBank r;
        private readonly RewardInventory inv;
        private readonly HUDPresenter hud;

        public LostState(GameStateMachine fsm, ZoneService z, RewardBank r, RewardInventory inv,
            HUDPresenter hud)
        {
            this.fsm = fsm;
            this.z = z;
            this.r = r;
            this.inv = inv;
            this.hud = hud;
        }

        public void Enter()
        {
            hud.NotifyLost();
        }

        public void Exit()
        {
            hud.NotifyRestart();
        }
    }

    public sealed class CollectedState : IGameState
    {
        private readonly GameStateMachine fsm;
        private readonly ZoneService z;
        private readonly RewardBank r;
        private readonly RewardInventory inv;
        private readonly HUDPresenter hud;

        public CollectedState(GameStateMachine fsm, ZoneService z, RewardBank r, RewardInventory inv,
            HUDPresenter hud)
        {
            this.fsm = fsm;
            this.z = z;
            this.r = r;
            this.inv = inv;
            this.hud = hud;
        }

        public void Enter()
        {
            hud.NotifyCollected(inv);
            r.Reset();
            inv.Reset();
            hud.ShowInventory(inv.Snapshot());
            fsm.Enter<IdleState>();
        }

        public void Exit()
        {
        }
    }
}