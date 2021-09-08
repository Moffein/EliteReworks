using RoR2;
using UnityEngine;

namespace EliteReworks.Tweaks
{
    public static class ResizeElites
    {
        public static float t1Size = 1.25f;
        public static float t2Size = 1.5f;
        //Todo: Find out how to distinguish T1 from T2
        //Todo: Find out how to make large elites not sink into the floor
        //Todo: Add check to only show up on AI controlled characters
        public static void CharacterBody_Start(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            if (self.isElite)
            {
                float scaling = t1Size;
                self.transform.localScale *= scaling;
            }
        }
    }
}
