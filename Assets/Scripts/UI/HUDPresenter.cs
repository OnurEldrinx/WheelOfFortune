using System.Collections.Generic;
using Core.Game;
using Data;
using Infra;
using Systems;
using UnityEngine;

namespace UI
{
    public interface ICommand
    {
        void Execute();
    }

    public sealed class SpinCommand : ICommand
    {
        private readonly System.Action action;

        public SpinCommand(System.Action a)
        {
            action = a;
        }

        public void Execute() => action?.Invoke();
    }

    public sealed class CashOutCommand : ICommand
    {
        private readonly System.Action action;

        public CashOutCommand(System.Action a)
        {
            action = a;
        }

        public void Execute() => action?.Invoke();
    }

    public sealed class GiveUpCommand : ICommand
    {
        private readonly System.Action action;

        public GiveUpCommand(System.Action a)
        {
            action = a;
        }
        
        public void Execute() => action?.Invoke();

    }
    
    public sealed class HUDPresenter
    {
        private readonly HUDView view;
        private readonly EventBus bus;
        private ICommand spinCmd;
        private ICommand cashOutCmd;
        private ICommand giveUpCmd;
        private ZoneService zones;

        public HUDPresenter(HUDView view, EventBus bus)
        {
            this.view = view;
            this.bus = bus;
        }

        public void Bind(ICommand spin, ICommand cashOut,ICommand giveUp,ZoneService z)
        {
            spinCmd = spin;
            cashOutCmd = cashOut;
            giveUpCmd = giveUp;
            zones = z;
        }

        public void ShowTotals(int zone, bool canCashOut)
        {
            view.SetCashOutVisible(canCashOut);
        }

        public void ShowInventory(List<RewardInventory.Entry> entries)
        {
            view.SetInventory(entries);
        }

        public void SetButtonsEnabled(bool on) => view.SetButtonsEnabled(on);

        public void NotifyLost()
        {
            Debug.Log("Lost: hit bomb");
            view.SetFailViewVisibility(true);
        }

        public void NotifyRestart()
        {
            Debug.Log("Restart");
            SetButtonsEnabled(true);
            view.SetFailViewVisibility(false);
        }

        public void NotifyCollected(RewardInventory rewardInventory) =>
            view.collectedRewardsShowcaseView.BuildShowcaseView(rewardInventory.Snapshot());

        public void NotifyRewardFlowAnimation(SliceDef rewardSlice)
        {
            var targetRect = view.inventoryView.imageRefMap[rewardSlice.id.Trim()].GetComponent<RectTransform>();
            UIRewardFlow.PlayFromUI(view.wheelView.wheelImageRT,targetRect,rewardSlice.icon,count:10,duration:0.75f,startScale:0f,endScale:1f);
        }

        public void OnSpinPressed()
        {
            spinCmd?.Execute();
        }

        public void OnCashOutPressed()
        {
            if (zones != null && (zones.IsSafeZone(zones.CurrentZone) || zones.IsSuperZone(zones.CurrentZone)))
                cashOutCmd?.Execute();
        }

        public void OnGiveUpPressed()
        {
            giveUpCmd?.Execute();
        }
        
    }
}