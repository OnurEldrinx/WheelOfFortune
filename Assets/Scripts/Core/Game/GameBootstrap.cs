using System.Collections.Generic;
using Core.Risk;
using Core.State;
using Core.Wheel;
using Data;
using Infra;
using Systems;
using UI;
using UnityEngine;

namespace Core.Game
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("Scene Refs")] 
        [SerializeField] private ZoneService zoneService;
        [SerializeField] private HUDView hudView; 
        [SerializeField] private WheelView wheelView;
        [SerializeField] private EventBus eventBus;

        [Header("Wheel Config Data")] 
        [SerializeField] private WheelConfig bronzeWheel;
        [SerializeField] private WheelConfig silverWheel;
        [SerializeField] private WheelConfig goldWheel;

        // Services
        private RewardBank rewardBank;
        private RewardInventory rewardInventory;
        private WheelConfigFactory wheelFactory;
        private RiskPolicy riskPolicy;
        private GameStateMachine fsm;
        private HUDPresenter hudPresenter;

        private Dictionary<WheelType, WheelConfig> wheelConfigs = new();
        
        private void Awake()
        {
            wheelConfigs = new Dictionary<WheelType, WheelConfig>
            {
                { WheelType.Bronze, bronzeWheel },
                { WheelType.Silver, silverWheel },
                { WheelType.Gold, goldWheel }
            };
            
            // Core services
            rewardBank = new RewardBank();
            rewardInventory = new RewardInventory();
            riskPolicy = new RiskPolicy(new SafeZoneStrategy(), new SuperZoneStrategy(), new DefaultRiskStrategy());
            wheelFactory = new WheelConfigFactory(wheelConfigs)
                .AddModifier(new ScaleRewardsByTier());

            // Presenter (MVP)
            hudPresenter = new HUDPresenter(hudView, eventBus);

            // FSM
            fsm = new GameStateMachine(eventBus, zoneService, rewardBank, rewardInventory, wheelView, wheelFactory,
                riskPolicy, hudPresenter);
            
            // Bind through code because presenter is not a MonoBehaviour.
            var binder = FindObjectOfType<ButtonBinder>(true);
            if (binder != null) binder.Bind(hudPresenter);
        }

        private void Start()
        {
            // Init
            wheelView.UpdateConfig(bronzeWheel);
            fsm.Enter<IdleState>();
        }
    }
}