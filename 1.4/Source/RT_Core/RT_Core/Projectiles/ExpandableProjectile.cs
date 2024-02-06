﻿using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RT_Rimtroid
{

	[HarmonyPatch(typeof(Projectile), "Launch", new Type[]
		{ 
			typeof(Thing), typeof(Vector3), typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(ProjectileHitFlags),  typeof(bool), typeof(Thing), typeof(ThingDef)
	})]
	public static class Projectile_Launch_Patch
	{
		public static void Postfix(Projectile __instance, Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, Thing equipment = null, ThingDef targetCoverDef = null)
		{
			if (__instance is ExpandableProjectile expandableProjectile && expandableProjectile.Def.reachMaxRangeAlways)
			{
				expandableProjectile.SetDestinationToMax(equipment);
			}
		}
	}

	public class ExpandableProjectile : Bullet
	{
		private int curDuration;
		private Vector3 startingPosition;
		private Vector3 prevPosition;
		private int curProjectileIndex;
		private int curProjectileFadeOutIndex;
		private bool stopped;
		private float maxRange;
		public void SetDestinationToMax(Thing equipment)
        {
			maxRange = equipment.TryGetComp<CompEquippable>().PrimaryVerb.verbProps.range;
			var origin2 = new Vector3(this.launcher.TrueCenter().x, 0, this.launcher.TrueCenter().z);
			var destination2 = new Vector3(destination.x, 0, destination.z);
			var distance = Vector3.Distance(origin2, destination2);
			var distanceDiff = maxRange - distance;
			var normalized = (destination2 - origin2).normalized;
			this.destination += normalized * distanceDiff;
			var cell = this.destination.ToIntVec3();
			if (!cell.InBounds(this.Map))
			{
				var nearestCell = CellRect.WholeMap(this.Map).EdgeCells.OrderBy(x => x.DistanceTo(cell)).First();
				this.destination = nearestCell.ToVector3();
			}
			this.ticksToImpact = Mathf.CeilToInt(StartingTicksToImpact);
		}
		public new virtual int DamageAmount
		{
			get
			{
				return Def.projectile.GetDamageAmount(weaponDamageMultiplier);
			}
		}
		public bool IsMoving
		{
			get
			{
				if (!stopped && this.DrawPos != prevPosition)
				{
					prevPosition = this.DrawPos;
					return true;
				}
				return false;
			}
		}
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				startingPosition = this.Position.ToVector3Shifted();
				startingPosition.y = 0;
			}
		}

		private int prevTick;
		public bool doFinalAnimations;
		public ExpandableProjectileDef Def => base.def as ExpandableProjectileDef;
		private Material ProjectileMat
		{
			get
			{
				if (!this.Def.graphicData.texPathFadeOut.NullOrEmpty() && (!doFinalAnimations || this.Def.lifeTimeDuration - curDuration > this.Def.graphicData.MaterialsFadeOut.Length - 1))
				{
					var material = this.Def.graphicData.Materials[curProjectileIndex];
					if (prevTick != Find.TickManager.TicksAbs && Find.TickManager.TicksAbs - this.TickFrameRate >= prevTick)
					{
						if (curProjectileIndex == this.Def.graphicData.Materials.Length - 1)
							curProjectileIndex = 0;
						else curProjectileIndex++;
						prevTick = Find.TickManager.TicksAbs;
					}
					return material;
				}
				else
				{
					var material = this.Def.graphicData.MaterialsFadeOut[curProjectileFadeOutIndex];
					if (prevTick != Find.TickManager.TicksAbs && Find.TickManager.TicksAbs - this.TickFrameRate >= prevTick)
					{
						if (this.Def.graphicData.MaterialsFadeOut.Length - 1 != curProjectileFadeOutIndex)
						{
							curProjectileFadeOutIndex++;
						}
						prevTick = Find.TickManager.TicksAbs;
					}
					return material;
				}
			}
		}

		public override void Draw()
		{
			DrawProjectile();
		}

		public int TickFrameRate
		{
			get
			{
				if (!doFinalAnimations)
				{
					return this.Def.tickFrameRate;
				}
				else if (this.Def.finalTickFrameRate > 0)
				{
					return this.Def.finalTickFrameRate;
				}
				return this.Def.tickFrameRate;
			}
		}

		public bool pawnMoved;
		public Vector3 StartingPosition
		{
			get
			{
				if (!(this.launcher is Pawn))
                {
					this.startingPosition = this.launcher.OccupiedRect().CenterVector3;
				}
				else if (!pawnMoved && this.launcher is Pawn pawn && !pawn.Dead)
				{
					if (pawn.pather.MovingNow)
					{
						pawnMoved = true;
					}
					else
					{
						this.startingPosition = pawn.OccupiedRect().CenterVector3;
					}
				}
				return this.startingPosition;
			}
		}

		public Vector3 curPosition;
		public Vector3 CurPosition
		{
			get
			{
				if (stopped)
                {
					return curPosition;
                }
				else if (this.Def.reachMaxRangeAlways)
                {
					var origin2 = new Vector3(this.launcher.TrueCenter().x, 0, this.launcher.TrueCenter().z);
					var curPos = this.DrawPos;
					var distance = Vector3.Distance(origin2, curPos);
					var distanceDiff = maxRange - distance;
					if (distanceDiff < 0)
                    {
						if (!stopped)
                        {
							StopMotion();
                        }
						return curPosition;
					}
					else
                    {
						return this.DrawPos;
					}
				}
				else
				{
					return this.DrawPos;
				}
			}
		}
		public void DrawProjectile()
		{
			var currentPos = CurPosition;
			currentPos.y = 0;
			var startingPosition = StartingPosition;
			startingPosition.y = 0;
            var destination = new Vector3(this.destination.x, this.destination.y, this.destination.z)
            {
                y = 0
            };

            Quaternion quat = Quaternion.LookRotation(currentPos - startingPosition);
			Vector3 pos = (startingPosition + currentPos) / 2f;
			pos.y = 10;
			pos += Quaternion.Euler(0, (startingPosition - currentPos).AngleFlat(), 0) * Def.startingPositionOffset;

			var distance = Vector3.Distance(startingPosition, currentPos) * Def.totalSizeScale;
			var distanceToTarget = Vector3.Distance(startingPosition, destination);
			var widthFactor = distance / distanceToTarget;

			var vec = new Vector3(distance * Def.widthScaleFactor * widthFactor, 0f, distance * Def.heightScaleFactor);
			Matrix4x4 matrix = default;
			matrix.SetTRS(pos, quat, vec);
			Graphics.DrawMesh(MeshPool.plane10, matrix, ProjectileMat, 0);
		}
		public static float GetOppositeAngle(float angle)
        {
			return (angle + 180f) % 360f;
		}
		public static Direction8Way Direction8WayFromAngle(float angle)
		{
			if (angle >= 337.5f || angle < 22.5f)
			{
				return Direction8Way.North;
			}
			if (angle < 67.5f)
			{
				return Direction8Way.NorthEast;
			}
			if (angle < 112.5f)
			{
				return Direction8Way.East;
			}
			if (angle < 157.5f)
			{
				return Direction8Way.SouthEast;
			}
			if (angle < 202.5f)
			{
				return Direction8Way.South;
			}
			if (angle < 247.5f)
			{
				return Direction8Way.SouthWest;
			}
			if (angle < 292.5f)
			{
				return Direction8Way.West;
			}
			return Direction8Way.NorthWest;
		}

		public static IntVec3 IntVec3FromDirection8Way(Direction8Way source)
		{
			switch (source)
			{
				case Direction8Way.North: return IntVec3.North;
				case Direction8Way.NorthEast: return IntVec3.NorthEast;
				case Direction8Way.East: return IntVec3.East;
				case Direction8Way.SouthEast: return IntVec3.SouthEast;
				case Direction8Way.South: return IntVec3.South;
				case Direction8Way.SouthWest: return IntVec3.SouthWest;
				case Direction8Way.West: return IntVec3.West;
				case Direction8Way.NorthWest: return IntVec3.NorthWest;
				default: return IntVec3.Invalid;
			}
		}

		[TweakValue("0RM", 0, 10)] private static float widthTweak = 2.5f;
		public static HashSet<IntVec3> GetProjectileLine(ExpandableProjectileDef projectileDef, Vector3 curPosition, Vector3 startingPosition, Vector3 end, Map map)
		{
			HashSet<IntVec3> positions = new HashSet<IntVec3>();
			if (projectileDef.fixedShape != null)
            {
				var widthCurve = projectileDef.fixedShape.widthCurve;
				var resultingLine = new ShootLine(startingPosition.ToIntVec3(), end.ToIntVec3());
				var points = resultingLine.Points().ToList();
				startingPosition.y = 0;
				end.y = 0;

				var angle = startingPosition.AngleToFlat(end) + 180f;
				Log.Clear();
				var level = 0;
				for (var i = 0; i < points.Count; i++)
				{
					var startCell = points[i];
					if (startCell.DistanceTo(startingPosition.ToIntVec3()) >= projectileDef.minDistanceToAffect)
                    {
						var curBatch = new List<IntVec3>
                        {
                            startCell
                        };
						var width = widthCurve.Evaluate(level);
						if (width > 0)
						{
							int num = 1;
							while (curBatch.Count < width)
							{
								if (curBatch.Count < width)
								{
									var directionWay = Direction8WayFromAngle(angle);
									var facingCell = IntVec3FromDirection8Way(directionWay);
									Log.Message("angle: " + angle + ", direction way: " + directionWay + " - facing cell: " + facingCell);
									curBatch.Add(startCell + (facingCell * num));
								}
								if (curBatch.Count < width)
								{
									var oppositeAngle = GetOppositeAngle(angle);
									var directionWay = Direction8WayFromAngle(oppositeAngle);
									var facingCell = IntVec3FromDirection8Way(directionWay);
									Log.Message("Opposite angle: " + oppositeAngle + ", direction way: " + directionWay + " - facing cell: " + facingCell);
									curBatch.Add(startCell + (facingCell * num));
								}
								num++;
							}
							positions.AddRange(curBatch);
						}
						level++;
					}
				}
			}
			else
            {
				var resultingLine = new ShootLine(startingPosition.ToIntVec3(), end.ToIntVec3());
				var points = resultingLine.Points();
				foreach (var cell in points)
                {
					foreach (var adjCell in GenRadial.RadialCellsAround(cell, 5f, true))
					{
						positions.Add(cell);
						map.debugDrawer.FlashCell(cell);
					}
				}
				var currentPos = curPosition;
				currentPos.y = 0;
				startingPosition.y = 0;
				var destination = new Vector3(currentPos.x, currentPos.y, currentPos.z);

				Vector3 pos = (startingPosition + currentPos) / 2f;
				pos.y = 10;
				pos += Quaternion.Euler(0, (startingPosition - currentPos).AngleFlat(), 0) * projectileDef.startingPositionOffset;

				var distance = Vector3.Distance(startingPosition, currentPos) * projectileDef.totalSizeScale;
				var distanceToTarget = Vector3.Distance(startingPosition, currentPos);
				var widthFactor = distance / distanceToTarget;

				var width = distance * projectileDef.widthScaleFactor * widthFactor;
				if (projectileDef.minWidth > 0 && projectileDef.minWidth > width)
                {
					width = projectileDef.minWidth;
				}
				var height = distance * projectileDef.heightScaleFactor;
				var centerOfLine = pos.ToIntVec3();
				var startPosition = startingPosition.ToIntVec3();
				var endPosition = curPosition.ToIntVec3();
				var radius = height > width ? height : width;
				//Log.Message("width: " + width + " - radius: " + radius);
				if (points.Any())
				{
					foreach (var cell in GenRadial.RadialCellsAround(startingPosition.ToIntVec3(), radius, true))
					{
						if (centerOfLine.DistanceToSquared(endPosition) >= cell.DistanceToSquared(endPosition) && startPosition.DistanceTo(cell) > projectileDef.minDistanceToAffect)
						{
							var nearestCell = points.MinBy(x => x.DistanceToSquared(cell));
							if ((width / height) > nearestCell.DistanceToSquared(cell))
							{
								positions.Add(cell);
							}
						}
					}

					foreach (var cell in points)
					{
						var startCellDistance = startPosition.DistanceTo(cell);
						if (startCellDistance > projectileDef.minDistanceToAffect && startCellDistance <= startPosition.DistanceTo(endPosition))
						{
							positions.Add(cell);
						}
					}
				}
			}
			return positions;
		}

		private void StopMotion()
        {
			if (!stopped)
            {
				stopped = true;
				curPosition = this.DrawPos;
				this.destination = this.curPosition;
			}
		}
		public override void Tick()
		{
			base.Tick();
			if (Find.TickManager.TicksGame % this.Def.tickDamageRate == 0)
			{
				var projectileLine = GetProjectileLine(this.Def, CurPosition, StartingPosition, DrawPos, this.Map);
				foreach (var pos in projectileLine)
				{
					DoDamage(pos);
				}
			}
			if (!doFinalAnimations && (!IsMoving || pawnMoved))
			{
				doFinalAnimations = true;
				var finalAnimationDuration = this.Def.lifeTimeDuration - this.Def.graphicData.MaterialsFadeOut.Length;
				if (finalAnimationDuration > curDuration)
				{
					//curDuration = finalAnimationDuration;
				}
				if (!this.Def.reachMaxRangeAlways && pawnMoved)
				{
					StopMotion();
				}
			}
			if (Find.TickManager.TicksGame % this.TickFrameRate == 0 && Def.lifeTimeDuration > 0)
			{
				curDuration++;
				if (curDuration > Def.lifeTimeDuration)
				{
					Log.Message("curDuration: " + curDuration + " - def.lifeTimeDuration: " + Def.lifeTimeDuration);
					this.Destroy();
				}
			}
		}
		public virtual void DoDamage(IntVec3 pos)
		{

		}

		protected bool customImpact;

		public List<Thing> hitThings;
		protected virtual void Impact(Thing hitThing)
		{
			if (Def.stopWhenHit && !stopped && !customImpact)
			{
				StopMotion();
			}
			if (hitThings == null) hitThings = new List<Thing>();
			if (this.Def.dealsDamageOnce && hitThings.Contains(hitThing))
            {
				return;
            }
			hitThings.Add(hitThing);
			Map map = base.Map;
			IntVec3 position = base.Position;
			BattleLogEntry_RangedImpact battleLogEntry_RangedImpact;
			if (equipmentDef == null)
			{
				battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(launcher, hitThing, intendedTarget.Thing, ThingDef.Named("Gun_Autopistol"), Def, targetCoverDef);
			}
			else
			{
				battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(launcher, hitThing, intendedTarget.Thing, equipmentDef, Def, targetCoverDef);
			}
			Find.BattleLog.Add(battleLogEntry_RangedImpact);
			this.NotifyImpact(hitThing, map, position);
			if (hitThing != null && (!Def.disableVanillaDamageMethod || customImpact && Def.disableVanillaDamageMethod))
			{
				DamageInfo dinfo = new DamageInfo(Def.projectile.damageDef, this.DamageAmount, base.ArmorPenetration, ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing);
				hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
                if (hitThing is Pawn pawn && pawn.stances != null && pawn.BodySize <= Def.projectile.StoppingPower + 0.001f)
                {
                    pawn.stances.stagger.StaggerFor(95);
                }
                if (Def.projectile.extraDamages != null)
				{
					foreach (ExtraDamage extraDamage in Def.projectile.extraDamages)
					{
						if (Rand.Chance(extraDamage.chance))
						{
							DamageInfo dinfo2 = new DamageInfo(extraDamage.def, extraDamage.amount, extraDamage.AdjustedArmorPenetration(), ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing);
							hitThing.TakeDamage(dinfo2).AssociateWithLog(battleLogEntry_RangedImpact);
						}
					}
				}

				if (this.Def.stopWhenHitAt.Contains(hitThing.def.defName))
                {
					if (!stopped)
                    {
						StopMotion();
					}
                }
			}
		}

		private void NotifyImpact(Thing hitThing, Map map, IntVec3 position)
		{
			BulletImpactData bulletImpactData = default;
			bulletImpactData.bullet = this;
			bulletImpactData.hitThing = hitThing;
			bulletImpactData.impactPosition = position;
			BulletImpactData impactData = bulletImpactData;
			try
			{
				hitThing?.Notify_BulletImpactNearby(impactData);
			}
			catch { };
			int num = 9;
			for (int i = 0; i < num; i++)
			{
				IntVec3 c = position + GenRadial.RadialPattern[i];
				if (!c.InBounds(map))
				{
					continue;
				}
				List<Thing> thingList = c.GetThingList(map);
				for (int j = 0; j < thingList.Count; j++)
				{
					if (thingList[j] != hitThing)
					{
						try
                        {
							thingList[j].Notify_BulletImpactNearby(impactData);
                        }
						catch { };
					}
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref startingPosition, "startingPosition");
			Scribe_Values.Look(ref doFinalAnimations, "doFinalAnimations");
			Scribe_Values.Look(ref pawnMoved, "pawnMoved");
			Scribe_Values.Look(ref curDuration, "curDuration");
			Scribe_Values.Look(ref curProjectileIndex, "curProjectileIndex");
			Scribe_Values.Look(ref curProjectileFadeOutIndex, "curProjectileFadeOutIndex");
			Scribe_Values.Look(ref prevTick, "prevTick");
			Scribe_Values.Look(ref prevPosition, "prevPosition");
			Scribe_Values.Look(ref stopped, "stopped");
			Scribe_Values.Look(ref curPosition, "curPosition");
			Scribe_Values.Look(ref maxRange, "maxRange");
		}
	}
}
