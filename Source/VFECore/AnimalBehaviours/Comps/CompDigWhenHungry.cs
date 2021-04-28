﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace AnimalBehaviours
{
    public class CompDigWhenHungry : ThingComp
    {
        public int stopdiggingcounter = 0;
        private Effecter effecter;

        public CompProperties_DigWhenHungry Props
        {
            get
            {
                return (CompProperties_DigWhenHungry)this.props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            Pawn pawn = this.parent as Pawn;
            if (AnimalBehaviours_Settings.flagDigWhenHungry&&(pawn.Map != null) && (pawn.Awake()) && 
                ((pawn.needs.food.CurLevelPercentage < pawn.needs.food.PercentageThreshHungry) ||
                (Props.digAnywayEveryXTicks && this.parent.IsHashIntervalTick(Props.timeToDigForced))))
            {
                if (pawn.Position.GetTerrain(pawn.Map).affordances.Contains(TerrainAffordanceDefOf.Diggable))
                {
                    if (stopdiggingcounter <= 0)
                    {
                        if (Props.acceptedTerrains != null)
                        {
                            if (Props.acceptedTerrains.Contains(pawn.Position.GetTerrain(pawn.Map).defName))
                            {
                                Thing newcorpse;
                                if (Props.isFrostmite)
                                {
                                    PawnKindDef wildman = PawnKindDef.Named("WildMan");
                                    Faction faction = FactionUtility.DefaultFactionFrom(wildman.defaultFactionType);
                                    Pawn newPawn = PawnGenerator.GeneratePawn(wildman, faction);
                                    newcorpse = GenSpawn.Spawn(newPawn, pawn.Position, pawn.Map, WipeMode.Vanish);
                                    newcorpse.Kill(null, null);
                                   
                                }
                                else { 
                                    ThingDef newThing = ThingDef.Named(this.Props.customThingToDig);
                                    newcorpse = GenSpawn.Spawn(newThing, pawn.Position, pawn.Map, WipeMode.Vanish);
                                    newcorpse.stackCount = this.Props.customAmountToDig;
                                }
                                if (Props.spawnForbidden)
                                {
                                    newcorpse.SetForbidden(true);
                                }
                                
                                if (this.effecter == null)
                                {
                                    this.effecter = EffecterDefOf.Mine.Spawn();
                                }
                                this.effecter.Trigger(pawn, newcorpse);

                            }
                        } else
                        {
                            Thing newcorpse;
                            if (Props.isFrostmite)
                            {
                                PawnKindDef wildman = PawnKindDef.Named("WildMan");
                                Faction faction = FactionUtility.DefaultFactionFrom(wildman.defaultFactionType);
                                Pawn newPawn = PawnGenerator.GeneratePawn(wildman, faction);
                                newcorpse = GenSpawn.Spawn(newPawn, pawn.Position, pawn.Map, WipeMode.Vanish);
                                newcorpse.Kill(null, null);

                            }
                            else
                            {
                                ThingDef newThing = ThingDef.Named(this.Props.customThingToDig);
                                newcorpse = GenSpawn.Spawn(newThing, pawn.Position, pawn.Map, WipeMode.Vanish);
                                newcorpse.stackCount = this.Props.customAmountToDig;
                            }
                            if (Props.spawnForbidden)
                            {
                                newcorpse.SetForbidden(true);
                            }
                            if (this.effecter == null)
                            {
                                this.effecter = EffecterDefOf.Mine.Spawn();
                            }
                            this.effecter.Trigger(pawn, newcorpse);
                        }
                                


                         

                        stopdiggingcounter = Props.timeToDig;
                    }
                    stopdiggingcounter--;
                }
            }
        }
    }
}

