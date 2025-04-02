using HarmonyLib;
using RimWorld;
using Verse;

namespace Multiplayer.Client.AsyncTime
{

    [HarmonyPatch(typeof(CompCauseGameCondition), nameof(CompCauseGameCondition.EnforceConditionOn))]
    static class MapConditionCauserMapTime
    {
        // __state is null when there is no async comp on the map or when the map is already in context
        static void Prefix(Map map, ref AsyncTimeComp __state)
        {
            if (Multiplayer.Client == null) return;
            if (!Multiplayer.GameComp.asyncTime) return;

            __state = map?.AsyncTime();
            if (__state is { isInContext: true })
            {
                __state = null;
                return;
            }

            __state?.PreContext();
        }

        static void Finalizer(AsyncTimeComp __state)
        {
            // We only call PostContext when we've called PreContext in the prefix
            __state?.PostContext();
        }
    }

    [HarmonyPatch(typeof(GameCondition), nameof(GameCondition.Expired), MethodType.Getter)]
    static class GameConditionExpired
    {
        static void Prefix(GameCondition __instance, ref TimeSnapshot? __state)
        {
            if (Multiplayer.Client == null) return;
            if (!Multiplayer.GameComp.asyncTime) return;
            __state = TimeSnapshot.GetAndSetFromMap(__instance.SingleMap);
        }

        static void Postfix(TimeSnapshot? __state) => __state?.Set();
    }

    [HarmonyPatch(typeof(GameCondition), nameof(GameCondition.TicksLeft), MethodType.Getter)]
    static class GameConditionTicksLeft
    {
        static void Prefix(GameCondition __instance, ref TimeSnapshot? __state)
        {
            if (Multiplayer.Client == null) return;
            if (!Multiplayer.GameComp.asyncTime) return;
            __state = TimeSnapshot.GetAndSetFromMap(__instance.SingleMap);
        }

        static void Postfix(TimeSnapshot? __state) => __state?.Set();
    }
}
