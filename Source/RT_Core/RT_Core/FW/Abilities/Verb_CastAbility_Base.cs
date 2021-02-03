﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RT_Core
{
    public class Verb_CastAbility_Base : Verb_CastAbility
    {
        public override bool Available()
        {
            return ability.CanCast && base.Available();
        }

        public override void Reset()
        {
            base.Reset();
            if (ability is Ability_Base ab)
            {
                ab.Reset();
            }
        }

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            return targ == Caster || base.CanHitTargetFrom(root, targ);
        }

        protected override bool TryCastShot()
        {
            if (ability is Ability_Base ba)
            {
                if (!ba.CanApplyOn(CurrentTarget) && !ba.CanApplyOn(CurrentDestination))
                {
                    //Should be applicable on either the target or the destination.
                    return false;
                }
            }

            if (ability.CanCast)
            {
                return base.TryCastShot();
            }
            else
            {
                return false;
            }
        }
    }

    public class Verb_CastAbility_EnergyAbsorb : Verb_CastAbility_Base
    {
        public override TargetingParameters targetParams
        {
            get
            {
                var targetingParameters = new TargetingParameters();
                targetingParameters.validator = ((TargetInfo targ) => 
                targ.Thing is Corpse corpse && !Utils.blackListRaces.Contains(corpse.InnerPawn.def) && corpse.GetRotStage() == RotStage.Fresh && corpse.Age < GenDate.TicksPerDay * 3
                || targ.Thing is Pawn victim && victim.IsPrisoner && !victim.Downed && victim.GetComp<CompPrisonerFeed>().canBeEaten
                || targ.Thing is Pawn victim2 && !Utils.blackListRaces.Contains(victim2.def) && victim2.RaceProps.Animal && victim2.Faction != this.CasterPawn.Faction && victim2.BodySize <= 4f);
                return targetingParameters;
            }
        }
    }
}
