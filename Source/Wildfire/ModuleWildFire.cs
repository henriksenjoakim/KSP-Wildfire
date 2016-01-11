using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace wildfire
{
    public class ModuleWildfire : PartModule
    {
        [KSPField(isPersistant = true)]
        public bool isOnFire = false;

        [KSPField(isPersistant = true)]
        public bool massCheckedOnce = false;

        [KSPField(isPersistant = true)]
        public bool massApplied = false;

        [KSPField(isPersistant = true)]
        public bool autoExtinguisherIsOn = false;

        [KSPField(isPersistant = true)]
        public bool isDecupler = false;

        [KSPField(isPersistant = true)]
        public bool isHeatshield = false;

        public float chanceOfFire = 99;
        public double baseRiskOfFireOverHeat = 10;
        public double baseRiskOfFireSpread = 10;
        public double baseRiskOfFireExplosions = 20;
        public double baseRiskOfFireBumping = 10;
        public double riskOfFireOverHeat;
        public double riskOfFireSpread;
        public double riskOfFireExplosions;
        public double riskOfFireBumping;
        public float currentWeatherType;
        public bool warningSoundShouldSound = false;
        public bool hasFireExtinguisher = false;
        public double previousTemp = 1;
        public double tempThreshold;
        //public float originalMass;
        
        public Part cachedParent;
        public Stack<Part> cachedChildren = new Stack<Part>();
        
        public void cacheParts()
        {
            excludeParts();
            if (this.part.parent != cachedParent)
            {
                cachedParent = this.part.parent;
            }
            if (this.part.children.Count != 0)
            {
                foreach (Part cpp in this.part.children)
                {
                    if (cpp.parent == this.part)
                    {
                        cachedChildren.Push(cpp);
                        //does this need to be stopped?
                    }    
                }
            }
        }

        /* //To be removed
        private void checkMass()
        {
            if (massCheckedOnce == false)
            {
                originalMass = this.part.mass;
                massCheckedOnce = true;
            }
        }*/

        private void checkExtinguisherStatus()
        {
            float partCounter = 0;
            float totalParts = this.part.vessel.parts.Count;
            float fireExtinguisherCount = 0;
            float autoActivatedCount = 0;
            foreach (Part p in this.vessel.Parts)
            {
                if (p.Modules.Contains("ModuleFireExtinguisher"))
                {
                    fireExtinguisherCount += 1;
                    var pp = p.Modules.OfType<ModuleFireExtinguisher>().Single();
                    pp = p.FindModulesImplementing<ModuleFireExtinguisher>().First();
                    if (pp.autoActivated)
                    {
                        autoActivatedCount += 1;
                    }
                    if (partCounter == totalParts)
                    {
                        if (fireExtinguisherCount > 0)
                        {
                            hasFireExtinguisher = true;
                            /*
                            if (massCheckedOnce && !massApplied)
                            {
                                this.part.mass = Convert.ToSingle(Math.Floor(this.part.mass + 1.1));
                            }*/
                            if (autoActivatedCount > 0)
                            {
                                autoExtinguisherIsOn = true;
                            } 
                            else
                            {
                                autoExtinguisherIsOn = false;
                            }
                        }
                        else
                        {
                            /*
                            if (massCheckedOnce && massApplied)
                            {
                                this.part.mass = originalMass;
                            }
                             */
                            hasFireExtinguisher = false;
                            autoExtinguisherIsOn = false;
                        }
                        fireExtinguisherCount = 0;
                        partCounter = 0;
                        autoActivatedCount = 0;
                    }
                }
            }
        }

        private void extinguisher()
        {
            if (isOnFire && hasFireExtinguisher && autoExtinguisherIsOn)
            {
                double totalWater = 0;
                float counter = 0;
                float totalParts = this.part.vessel.parts.Count;
                foreach (Part p in this.part.vessel.parts)
                {
                    if (p.Resources["Water"].amount > 0)
                    {
                        totalWater += p.Resources["Water"].amount;
                    }
                    if (counter == totalParts)
                    {
                        if (totalWater >= this.part.mass)
                        {
                            if (chanceOfFire < 50)
                            {
                                this.part.RequestResource("Water", this.part.mass);
                                isOnFire = false;
                                //insert soundFX
                            }
                        }
                        totalWater = 0;
                        counter = 0;
                    }
                }
            }
        }

        //Calculate for OVERHEATING
        private void checkOverheat()
        {
            //Debug.Log("WF: Checking Overheat");
            if (!isOnFire && !isHeatshield)
            {
                //Debug.Log("WF: Check ifOnFire" +tempThreshold);
                tempThreshold = ((this.part.skinTemperature / this.part.skinMaxTemp) * 100);
                if (tempThreshold >= 70)
                {
                    //Debug.Log("WF: CheckFireChance");
                    if (chanceOfFire <= riskOfFireOverHeat)
                    {
                        //Debug.Log("WF: Initiate isonfire");
                        isOnFire = true;
                    }
                }
            }
        }

        //Calculate for ADJACENT FIRE
        private void spreadCheck()
        {
            if (!isOnFire && !isHeatshield)
            {
                if (this.part.parent != null)
                {
                    var pp = part.parent.Modules.OfType<ModuleWildfire>().Single();
                    pp = part.parent.FindModulesImplementing<ModuleWildfire>().First();
                    if (pp.isOnFire == true)
                    {
                        if (chanceOfFire <= riskOfFireSpread)
                        {
                            //Debug.Log("WF: Inherit WF from parent");
                            isOnFire = true;
                        }
                    }
                }

                foreach (Part cp in this.part.children)
                {
                    //double tempThresholdChildren = ((cp.skinTemperature / cp.skinMaxTemp) * 100);
                    var cpm = cp.Modules.OfType<ModuleWildfire>().Single();
                    cpm = cp.FindModulesImplementing<ModuleWildfire>().First();
                    if (cpm.isOnFire == true && chanceOfFire <= riskOfFireSpread)
                    {
                        //Debug.Log("WF: Inherit WF from children");
                        isOnFire = true;
                    }
                }
            }
        }

        //Calculate for COLLISIONS
        private void onCollision(Part p, Collision c)
        {
            if (this.part == p && c.relativeVelocity.magnitude > (this.part.crashTolerance * 0.9) && chanceOfFire <= riskOfFireBumping && !isHeatshield)
            {
                isOnFire = true;
            }
        }

        //Calculate for PART LOSS
        private void onPartDie(Part p)
        {
            cacheParts(); 
            if (p == cachedParent && !isHeatshield && !isDecupler)
            {
                var pm = p.Modules.OfType<ModuleWildfire>().Single();
                pm = p.FindModulesImplementing<ModuleWildfire>().First();
                double riskOfFireExplosionsFinal = (riskOfFireExplosions * pm.riskOfFireExplosions); //wtf
                if (chanceOfFire <= riskOfFireExplosionsFinal)
                {
                    //Debug.Log("WF: Inherit WF from parent EXPLOSION");
                    isOnFire = true;
                }
            }
           
            foreach (Part pt in cachedChildren)
            {

                if (p == pt && !isHeatshield && !isDecupler)
                {
                    var pm = p.Modules.OfType<ModuleWildfire>().Single();
                    pm = p.FindModulesImplementing<ModuleWildfire>().First();
                    double riskOfFireExplosionsFinal = (riskOfFireExplosions * pm.riskOfFireExplosions); //wtf
                    if (chanceOfFire <= riskOfFireExplosionsFinal)
                    {
                        //Debug.Log("WF: Inherit WF from children EXPLOSION");
                        isOnFire = true;
                    }
                }
            }
        }

        //Fire function
        private void combust()
        {
            if (isOnFire)
            {
                this.part.skinTemperature = previousTemp + (this.part.skinMaxTemp / 500);
                previousTemp = this.part.skinTemperature;
            }
            if (!isOnFire)
            {
                previousTemp = this.part.skinTemperature;
            }
        }

        //Ticker
        float timerCurrent = 0f;
        float timerTotal = 2f;

        private void tickRate()
        {
            timerCurrent += Time.deltaTime;
            if (timerCurrent >= timerTotal)
            {
                //Debug.Log("WF: Tick " +chanceOfFire +isOnFire);
                timerCurrent -= timerTotal;
                chanceOfFire = UnityEngine.Random.Range(0, 100);
                checkOverheat();
                spreadCheck();
                extinguisher();
                visualQue();
            }
        }

        private void excludeParts()
        {
            //heatshields
            if (this.part.Modules.Contains("ModuleDecouple") || this.part.Modules.Contains("ModuleAnchoredDecoupler"))
            {
                isHeatshield = true;
            }
            if (this.part.Modules.Contains("ModuleHeatshield"))
            {
                isHeatshield = true;
            }
        }

        /*
         //UNDER CONSTRUCTION
         
        private void breakingCheck()
        {
            //breakingforce breaking torque
        }
        

        private void groundControlRiskReduction()
        {
            //add antenna
            KerbalRoster r = HighLogic.CurrentGame.CrewRoster;
            foreach (ProtoCrewMember k in r.Crew)
            {
                if (k.trait.Contains("Engineer"))
                {
                    //general
                }
                if (k.trait.Contains("Pilot"))
                {
                    //atmospheric flight
                }
                if (k.trait.Contains("Scientest"))
                {
                    //spaceflight
                }
            }
        }

        private void activeCrewRiskReduction()
        {
            if (this.part.vessel.GetCrewCount() != 0)
            {
                foreach (ProtoCrewMember pcm in this.part.vessel.GetVesselCrew())
                {
                    if (pcm.trait.Contains("Engineer"))
                    {
                        //get
                    }
                    if (pcm.trait.Contains("Pilot"))
                    {
                        //get
                    }
                    if (pcm.trait.Contains("Scientest"))
                    {
                        //get
                    }
                }
            }
        }
     
        private void atmoRisk()
        {  
            //add space risk?
            double UT = (Planetarium.GetUniversalTime());
            double baseTime = 0;
            double timeForChange = 10800;
            if (baseTime == 0)
            {
                currentWeatherType = UnityEngine.Random.Range(0, 3);
            }

            if ((baseTime + timeForChange) < UT)
            {
                baseTime = UT;
                currentWeatherType = UnityEngine.Random.Range(0, 3);
            }
        }
        */

        /*
        //WIP
        private void typeRisk()
        {
            if (this.part.Modules.Contains("ModuleCommand"))
            {
                var p = this.part.Modules.OfType<ModuleCommand>().Single();
                p = this.part.FindModulesImplementing<ModuleCommand>().First();
                if (p.minimumCrew > 0)
                {
                    //add risk
                }
            }
            if (this.part.Modules.Contains("ModuleCommand"))
            {

            }
        }
        */

        private void riskCalculation()
        {
            double addedRisk = 0;
            /*
            double riskMultiplier = 1;

            //weather risk
            CelestialBody CB = this.part.vessel.mainBody;
            if (CB.atmosphere == true)
            {
                if (this.part.vessel.altitude < CB.atmosphereDepth)
                {
                    //insert risk for weather etc.
                    riskMultiplier -= (currentWeatherType*5);
                    if (CB.atmosphereContainsOxygen)
                    {
                        riskMultiplier += 0.2;
                    }
                }
            }*/

            //check for fules
            if (this.part.Resources.Count > 0)
            {
                foreach (PartResource r in this.part.Resources)
                {
                    if (r.resourceName.Contains("Oxidizer") && r.amount != 0)
                    {
                        addedRisk += (r.amount / r.maxAmount * 20);
                    }
                    if (r.resourceName.Contains("LiquidFuel") && r.amount != 0)
                    {
                        addedRisk += (r.amount / r.maxAmount * 10);
                    }
                    if (r.resourceName.Contains("MonoPropellant") && r.amount != 0)
                    {
                        addedRisk += (r.amount / r.maxAmount * 20);
                    }
                }
            }

            //inject final risk assesment into part
            riskOfFireOverHeat = (baseRiskOfFireOverHeat + addedRisk);
            riskOfFireSpread = (baseRiskOfFireSpread + addedRisk);
            riskOfFireExplosions = (baseRiskOfFireExplosions + addedRisk);
            riskOfFireBumping = (baseRiskOfFireBumping + addedRisk);
        }

        //Visuals
        private void visualQue()
        {
            if (isOnFire)
            {
                bool lightIsOn = false;
                if (!lightIsOn)
                {
                    this.part.SetHighlightDefault();
                    this.part.SetHighlightColor(Color.red);
                    this.part.SetHighlightType(Part.HighlightType.AlwaysOn);
                    this.part.SetHighlight(true, false);
                    lightIsOn = true;
                }
                else
                {
                    this.part.SetHighlightDefault();
                    lightIsOn = false;
                }
            }
            else
            {
                this.part.SetHighlightDefault();
            }
        }

        public void onVesselWasModified(Vessel v)
        {
            //checkMass();
            cacheParts();
        }
        public void onLaunch()
        {
            cacheParts();
        }
        public void onScenceChange()
        {
            //checkMass();
            cacheParts();
        }
        public void onCollision(EventReport data)
        {
            cacheParts();
        }
        public void onVesselWillDestroy(Vessel v)
        {
            cacheParts();
            if (v == FlightGlobals.ActiveVessel)
            {
                //forgot
            }
        }
        public void onVesselLoaded(Vessel v)
        {
            //checkMass();
            cacheParts();
        }
        public void onVesselChange(Vessel v)
        {
            //checkMass();
            cacheParts();
        }
        public void onPartUndock(Part p)
        {
            cacheParts();
        }
        public void onStageSeparation(EventReport data)
        {
            cacheParts();
        }
        public void onStageActivate(int i)
        {
            cacheParts();
        }
        public void onSplashDamage(EventReport data)
        {
            cacheParts();
        }
        public void onPartUnpack(Part p)
        {
            cacheParts();
        }
        public void onPartJointBreak(PartJoint pj)
        {
            cacheParts();
        }
        public void onPartExplode(GameEvents.ExplosionReaction er)
        {
            cacheParts();
        }
        public void onPartDestroyed(Part p)
        {
            cacheParts();
        }
        public void onOverheat(EventReport data)
        {
            cacheParts();
        }
        public void onLevelWasLoaded(GameScenes gs)
        {
            //checkMass();
            cacheParts();
        }
        public void onLaunch(EventReport data)
        {
            cacheParts();
        }
        public void onJointBreak(EventReport data)
        {
            cacheParts();
        }
        public void onEditiorShipModified(ShipConstruct data)
        {
            cacheParts();
        }
        public void onCrash(EventReport data)
        {
            cacheParts();
        }

        public override void OnStart(PartModule.StartState state)
        {
            //checkMass();
            cacheParts();
            GameEvents.onCrash.Add(onCrash);
            GameEvents.onEditorShipModified.Add(onEditiorShipModified);
            GameEvents.onJointBreak.Add(onJointBreak);
            GameEvents.onLaunch.Add(onLaunch);
            GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded);
            GameEvents.onOverheat.Add(onOverheat);
            GameEvents.onPartExplode.Add(onPartExplode);
            GameEvents.onPartJointBreak.Add(onPartJointBreak);
            GameEvents.onPartUnpack.Add(onPartUnpack);
            GameEvents.onSplashDamage.Add(onSplashDamage);
            GameEvents.onStageActivate.Add(onStageActivate);
            GameEvents.onStageSeparation.Add(onStageSeparation);
            GameEvents.onPartUndock.Add(onPartUndock);
            GameEvents.onVesselChange.Add(onVesselChange);
            GameEvents.onVesselLoaded.Add(onVesselLoaded);
            GameEvents.onVesselWillDestroy.Add(onVesselWillDestroy);
            GameEvents.onCollision.Add(onCollision);
            GameEvents.onVesselWasModified.Add(onVesselWasModified);
            GameEvents.onPartDie.Add(onPartDie);
            base.OnStart(state);
        }

        public override void OnUpdate()
        {
            tickRate();
            combust();
        }

    }
}
