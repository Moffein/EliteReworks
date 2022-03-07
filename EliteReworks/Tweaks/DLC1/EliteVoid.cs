using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine;

namespace EliteReworks.Tweaks.DLC1
{
    public class EliteVoid
    {
        public static void Setup()
        {
            //TODO: Why doesn't this work?
            Debug.Log("Attempting void hook");
            //Remove vanilla on-hit effect
            /*IL.RoR2.GlobalEventManager.OnHitAll += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                     x => x.MatchLdsfld(typeof(DLC1Content.Buffs), "EliteVoid")
                    );
                //c.Remove();
                //c.Emit<EliteReworksPlugin>(OpCodes.Ldsfld, nameof(EliteReworksPlugin.EmptyBuff));
            };*/
        }
    }
}
