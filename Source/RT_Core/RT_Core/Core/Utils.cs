﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RT_Core
{
    public static class Utils
    {
        public static bool IsMetroid(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_AlphaMetroid ||
                pawn.def == RT_DefOf.RT_GammaMetroid || 
                pawn.def == RT_DefOf.RT_ZetaMetroid ||
                pawn.def == RT_DefOf.RT_OmegaMetroid ||
                pawn.def == RT_DefOf.RT_QueenMetroid)
            {
                return true;
            }
            return false;
        }
        public static bool IsBanteeMetroid(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_BanteeMetroid)
            {
                return true;
            }
            return false;
        }
        public static bool IsMetroidLarvae(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_MetroidLarvae)
            {
                return true;
            }
            return false;
        }
        public static bool IsAlphaMetroid(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_AlphaMetroid)
            {
                return true;
            }
            return false;
        }
        public static bool IsGammaMetroid(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_GammaMetroid)
            {
                return true;
            }
            return false;
        }
        public static bool IsZetaMetroid(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_ZetaMetroid)
            {
                return true;
            }
            return false;
        }
        public static bool IsOmegaMetroid(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_OmegaMetroid)
            {
                return true;
            }
            return false;
        }
        public static bool IsQueenMetroid(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_QueenMetroid)
            {
                return true;
            }
            return false;
        }
        public static bool IsStuntableMetroid(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_MetroidLarvae||
                pawn.def == RT_DefOf.RT_AlphaMetroid ||
                pawn.def == RT_DefOf.RT_GammaMetroid ||
                pawn.def == RT_DefOf.RT_ZetaMetroid ||
                pawn.def == RT_DefOf.RT_OmegaMetroid)
            {
                return true;
            }
            return false;
        }
        public static bool IsValuedMetroid(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_BanteeMetroid ||
                pawn.def == RT_DefOf.RT_GammaMetroid)
            {
                return true;
            }
            return false;
        }
        public static bool IsDevaluedMetroid(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_MetroidLarvae ||
                pawn.def == RT_DefOf.RT_AlphaMetroid)
            {
                return true;
            }
            return false;
        }
        public static bool IsNeutralValueMetroid(this Pawn pawn)
        {
            if (pawn.def == RT_DefOf.RT_ZetaMetroid ||
                pawn.def == RT_DefOf.RT_OmegaMetroid)
            {
                return true;
            }
            return false;
        }
    }
    public class CompProperties_ApplyHediff_UseEffect : CompProperties_Usable
    {
        public HediffDef hediffDef;
        public bool allowNonColonists;

        public CompProperties_ApplyHediff_UseEffect()
        {
            compClass = typeof(CompApplyHediff_InstallImplant);
        }
    }
}
