﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MVCF.Comps;
using MVCF.ModCompat;
using MVCF.Utilities;
using RimWorld;
using Verse;

namespace MVCF.Features.PatchSets;

public class PatchSet_Base : PatchSet
{
    private static readonly MethodInfo UsableVerbMI = AccessTools.Method(typeof(BreachingUtility), "UsableVerb");

    public override IEnumerable<Patch> GetPatches()
    {
        yield return Patch.Prefix(AccessTools.Method(typeof(Verb), nameof(Verb.OrderForceTarget)),
            AccessTools.Method(GetType(), nameof(Prefix_OrderForceTarget)));
        yield return Patch.Prefix(AccessTools.PropertyGetter(typeof(Verb), nameof(Verb.EquipmentSource)),
            AccessTools.Method(GetType(), nameof(Prefix_EquipmentSource)));
        yield return Patch.Postfix(AccessTools.Method(typeof(BreachingUtility), nameof(BreachingUtility.FindVerbToUseForBreaching)),
            AccessTools.Method(GetType(), nameof(FindVerbToUseForBreaching)));
        yield return Patch.Postfix(AccessTools.Method(typeof(SlaveRebellionUtility), "CanApplyWeaponFactor"),
            AccessTools.Method(GetType(), nameof(CanApplyWeaponFactor)));
        yield return Patch.Prefix(AccessTools.Method(typeof(Targeter), "GetTargetingVerb"), AccessTools.Method(GetType(), nameof(Prefix_GetTargetingVerb)));
    }

    public static bool Prefix_GetTargetingVerb(Pawn pawn, Targeter __instance, ref Verb __result)
    {
        if (pawn.Manager(false) is not { } man) return true;
        __result = man.AllVerbs.FirstOrDefault(verb => verb.verbProps == __instance.targetingSource.GetVerb.verbProps);
        return false;
    }

    public static bool Prefix_OrderForceTarget(LocalTargetInfo target, Verb __instance)
    {
        if (__instance.verbProps.IsMeleeAttack || !__instance.CasterIsPawn)
            return true;
        if (MVCF.IsIgnoredMod(__instance.EquipmentSource == null
                ? __instance.HediffCompSource?.parent?.def?.modContentPack?.Name
                : __instance.EquipmentSource.def?.modContentPack?.Name)) return true;
        var man = __instance.CasterPawn.Manager();
        if (man == null) return true;
        var mv = __instance.Managed(false);
        if (mv != null) mv.Enabled = true;

        if (mv != null && !mv.SetTarget(target)) return false;

        if (DualWieldCompat.Active && __instance.CasterPawn.GetOffHand() is { } eq && eq.TryGetComp<CompEquippable>().PrimaryVerb is { } verb &&
            verb == __instance) return true;

        MVCF.Log("Changing CurrentVerb of " + __instance.CasterPawn + " to " + __instance, LogLevel.Important);
        man.CurrentVerb = __instance;

        return true;
    }

    public static bool Prefix_EquipmentSource(ref ThingWithComps __result, Verb __instance)
    {
        if (__instance == null) // Needed to work with A Rimworld of Magic, for some reason
        {
            Log.Warning("[MVCF] Instance in patch is null. This is not supported.");
            __result = null;
            return false;
        }

        switch (__instance.DirectOwner)
        {
            case Comp_VerbGiver giver:
                __result = giver.parent;
                return false;
            case HediffComp_VerbGiver _:
                __result = null;
                return false;
            case Pawn pawn:
                __result = pawn;
                return false;
            case VerbManager vm:
                __result = vm.Pawn;
                return false;
        }

        return true;
    }

    public static void CanApplyWeaponFactor(ref bool __result, Pawn pawn)
    {
        if (!__result && (pawn.Manager()?.AllVerbs.Except(pawn.verbTracker.AllVerbs).Any() ?? false)) __result = true;
    }

    public static void FindVerbToUseForBreaching(ref Verb __result, Pawn pawn)
    {
        if (__result == null && pawn.Manager() is VerbManager man)
            __result = man.AllVerbs.FirstOrDefault(v => (bool)UsableVerbMI.Invoke(null, new object[] { v }) && v.verbProps.ai_IsBuildingDestroyer);
    }
}