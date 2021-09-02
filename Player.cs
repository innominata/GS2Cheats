namespace GalacticScaleCheats
{
    public partial class GS2Cheats
    {
        //    playerOptions.Add(GSUI.Checkbox("Unlimited Energy".Translate(), false, "unlimitedEnergy", null, "Never run out of core power".Translate()));
        //    playerOptions.Add(GSUI.Checkbox("Unlock Sail".Translate(), false, "unlockSail", null, "Start game able to fly".Translate()));
        //    playerOptions.Add(GSUI.Checkbox("Unlock Warp".Translate(), false, "unlockWarp", null, "Start game able to warp".Translate()));
        //    playerOptions.Add(GSUI.Checkbox("Always Warp".Translate(), false, "alwaysWarp", null, "Never require warpers to warp".Translate()));
        //    playerOptions.Add(GSUI.Checkbox("Boost Walk Speed".Translate(), false, "boostWalkSpeed", null, "Walk faster".Translate()));
        //    playerOptions.Add(GSUI.Checkbox("Boost Sail Speed".Translate(), false, "boostSailSpeed", null, "Sail faster".Translate()));
        //    playerOptions.Add(GSUI.Checkbox("Boost Warp Speed".Translate(), false, "boostWarpSpeed", null, "Warp faster".Translate()));
        void MechaUpdate()
        {
            var mecha = GameMain.mainPlayer.mecha;
            if (preferences.GetBool("unLimitedEnergy", false))
            {
                mecha.coreEnergy = mecha.coreEnergyCap;
            }
            //if (preferences.GetBool("unlockSail", false))
            //{
            //    //GameMain
            //}
        }
    }
}