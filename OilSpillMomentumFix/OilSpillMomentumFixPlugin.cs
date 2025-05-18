using BepInEx;
using EntityStates.Chef;
using RoR2BepInExPack.Utilities;
using System.Diagnostics;

namespace OilSpillMomentumFix
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(R2API.R2API.PluginGUID)]
    public class OilSpillMomentumFixPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Gorakh";
        public const string PluginName = "OilSpillMomentumFix";
        public const string PluginVersion = "1.0.1";

        static OilSpillMomentumFixPlugin _instance;
        internal static OilSpillMomentumFixPlugin Instance => _instance;

        static readonly FixedConditionalWeakTable<OilSpillBase, OilSpillExtraData> _oilSpillExtraDataLookup = new FixedConditionalWeakTable<OilSpillBase, OilSpillExtraData>();

        class OilSpillExtraData
        {
            public float OriginalAirControl;
        }

        void Awake()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            SingletonHelper.Assign(ref _instance, this);

            Log.Init(Logger);

            On.EntityStates.Chef.OilSpillBase.OnEnter += OilSpillBase_OnEnter;
            On.EntityStates.Chef.OilSpillBase.OnExit += OilSpillBase_OnExit;

            stopwatch.Stop();
            Log.Message_NoCallerPrefix($"Initialized in {stopwatch.Elapsed.TotalMilliseconds:F0}ms");
        }

        void OnDestroy()
        {
            SingletonHelper.Unassign(ref _instance, this);
        }

        static void OilSpillBase_OnEnter(On.EntityStates.Chef.OilSpillBase.orig_OnEnter orig, OilSpillBase self)
        {
            if (self.characterMotor)
            {
                OilSpillExtraData oilSpillExtraData = _oilSpillExtraDataLookup.GetOrCreateValue(self);
                oilSpillExtraData.OriginalAirControl = self.characterMotor.airControl;
            }

            orig(self);
        }

        static void OilSpillBase_OnExit(On.EntityStates.Chef.OilSpillBase.orig_OnExit orig, OilSpillBase self)
        {
            orig(self);

            if (_oilSpillExtraDataLookup.TryGetValue(self, out OilSpillExtraData oilSpillExtraData))
            {
                if (self.characterMotor && oilSpillExtraData.OriginalAirControl > 0f)
                {
                    self.characterMotor.airControl = oilSpillExtraData.OriginalAirControl;
                }

                _oilSpillExtraDataLookup.Remove(self);
            }
        }
    }
}
