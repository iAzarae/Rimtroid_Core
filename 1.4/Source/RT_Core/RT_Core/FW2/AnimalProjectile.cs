﻿using System;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace RT_Core
{
    public class AnimalProjectile : Projectile
    {
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = base.Map;
            base.Impact(hitThing);
            BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(this.launcher, hitThing, this.intendedTarget.Thing, ThingDef.Named("Gun_Autopistol"), this.def, this.targetCoverDef);
            Find.BattleLog.Add(battleLogEntry_RangedImpact);
            if (hitThing != null)
            {
                DamageDef damageDef = this.def.projectile.damageDef;
                float amount = (float)base.DamageAmount;
                float armorPenetration = base.ArmorPenetration;
                float y = this.ExactRotation.eulerAngles.y;
                Thing launcher = this.launcher;
                DamageInfo dinfo = new DamageInfo(damageDef, amount, armorPenetration, y, launcher, null, null, DamageInfo.SourceCategory.ThingOrUnknown, this.intendedTarget.Thing);
                hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
                if (hitThing is Pawn pawn && pawn.stances != null && pawn.BodySize <= this.def.projectile.StoppingPower + 0.001f)
                {
                    pawn.stances.stagger.StaggerFor(95);
                }

                if (this.def.defName== "RT_FrostWeb")
                {
                    DamageInfo dinfo2 = new DamageInfo(DamageDefOf.Frostbite, amount/2, armorPenetration, y, launcher, null, null, DamageInfo.SourceCategory.ThingOrUnknown, this.intendedTarget.Thing);
                    hitThing.TakeDamage(dinfo2).AssociateWithLog(battleLogEntry_RangedImpact);



                }
                if (this.def.defName == "RT_FireWeb")
                {
                    DamageInfo dinfo2 = new DamageInfo(DamageDefOf.Burn, amount / 2, armorPenetration, y, launcher, null, null, DamageInfo.SourceCategory.ThingOrUnknown, this.intendedTarget.Thing);
                    hitThing.TakeDamage(dinfo2).AssociateWithLog(battleLogEntry_RangedImpact);



                }
                if (this.def.defName == "RT_AcidicWeb")
                {
                    DamageInfo dinfo2 = new DamageInfo(DefDatabase<DamageDef>.GetNamed("RT_AcidSpit", true), amount / 2, armorPenetration, y, launcher, null, null, DamageInfo.SourceCategory.ThingOrUnknown, this.intendedTarget.Thing);
                    hitThing.TakeDamage(dinfo2).AssociateWithLog(battleLogEntry_RangedImpact);



                }
                if (this.def.defName == "RT_ExplodingWeb")
                {
                    DamageInfo dinfo2 = new DamageInfo(DamageDefOf.Bomb, amount / 2, armorPenetration, y, launcher, null, null, DamageInfo.SourceCategory.ThingOrUnknown, this.intendedTarget.Thing);
                    hitThing.TakeDamage(dinfo2).AssociateWithLog(battleLogEntry_RangedImpact);



                }
            }
            else
            {
                SoundDefOf.BulletImpact_Ground.PlayOneShot(new TargetInfo(base.Position, map, false));
                FleckMaker.Static(this.ExactPosition, map, FleckDefOf.ShotHit_Dirt, 1f);
                if (base.Position.GetTerrain(map).takeSplashes)
                {
                    FleckMaker.WaterSplash(this.ExactPosition, map, Mathf.Sqrt((float)base.DamageAmount) * 1f, 4f);
                }
            }
        }
    }
}

