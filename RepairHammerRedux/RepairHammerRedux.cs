#if DEBUG
using Harmony;
#endif
using MSCLoader;
using UnityEngine;

namespace RepairHammerRedux
{
    public class RepairHammerRedux : Mod
    {
        public override string ID => "splatpope.RepairHammerRedux"; //Your mod ID (unique)
        public override string Name => "RepairHammerRedux"; //You mod name
        public override string Author => "splatpope"; //Your Username
        public override string Version => "1.1"; //Version
        public override string Description => "A remake of the RepairHammer mod for recent versions of My Summer Car (i.e. from 2023 onwards)."; //Short description of your mod

        public SettingsSlider repairRadius;
        public SettingsSlider repairFactor;

        public override void ModSetup()
        {
#if DEBUG 
            var harmony = HarmonyInstance.Create("com.splatpope.repairhammerredux");
            harmony.PatchAll();
#endif
            BodyFixer.mod_instance = this;
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
        }

        public override void ModSettings()
        {
            Settings.AddHeader(this, "Repair coefficients");
            this.repairRadius = Settings.AddSlider(this, "repair_radius", "Repair radius", 0.1f, 1f, 0.5f);
            this.repairFactor = Settings.AddSlider(this, "repair_factor", "Repair factor", 0.1f, 1f, 0.5f);
        }

        private void Mod_OnLoad()
        {
            ModConsole.Log("Initializing repair hammer...");

            GameObject.Find("SATSUMA(557kg, 248)/DeformLogic").AddComponent<DeformableUpdater>();
            GameObject.Find("PLAYER").transform.Find("Pivot/AnimPivot/Camera/FPSCamera/FPSCamera/Sledgehammer").gameObject.AddComponent<BodyFixer>();
        }
    }
}
