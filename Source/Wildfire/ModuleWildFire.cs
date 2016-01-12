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
        public bool autoExtinguisherIsOn = false;

        [KSPField(isPersistant = true)]
        public bool isDecoupler = false;

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
        public double previousTemp = 1;
        public double tempThreshold;
        public float currentWeatherType;
        public bool warningSoundShouldSound = false;
        public bool hasFireExtinguisher = false;

        private GameObject smokeFx;
        private GameObject fireFx;
        private GameObject sparkFx;

        AudioSource fireAudio;
        AudioSource extinguishAudio;

        public Part cachedParent;
        public Stack<Part> cachedChildren = new Stack<Part>();
        
        public void cacheParts()
        {
            //excludeParts();
            riskCalculation();
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
                    if (p.Resources["LiquidLuck"].amount > 0)
                    {
                        totalWater += p.Resources["LiquidLuck"].amount;
                    }
                    if (counter == totalParts)
                    {
                        if (totalWater >= this.part.mass)
                        {
                            if (chanceOfFire < 50)
                            {
                                this.part.RequestResource("LiquidLuck", this.part.mass);
                                extinguishAudio.Play(); //hmm?
                                isOnFire = false;
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
            if (!isOnFire)
            {
                tempThreshold = ((this.part.skinTemperature / this.part.skinMaxTemp) * 100);
                if (tempThreshold >= 70)
                {
                    riskCalculation();
                    if (chanceOfFire <= riskOfFireOverHeat)
                    {
                        isOnFire = true;
                    }
                }
            }
        }

        //Calculate for ADJACENT FIRE
        private void spreadCheck()
        {
            if (!isOnFire)
            {
                if (this.part.parent != null)
                {
                    var pp = part.parent.Modules.OfType<ModuleWildfire>().Single();
                    pp = part.parent.FindModulesImplementing<ModuleWildfire>().First();
                    if (pp.isOnFire == true)
                    {
                        riskCalculation();
                        if (chanceOfFire <= riskOfFireSpread)
                        {
                            isOnFire = true;
                        }
                    }
                }

                foreach (Part cp in this.part.children)
                {
                    //double tempThresholdChildren = ((cp.skinTemperature / cp.skinMaxTemp) * 100);
                    var cpm = cp.Modules.OfType<ModuleWildfire>().Single();
                    cpm = cp.FindModulesImplementing<ModuleWildfire>().First();
                    if (cpm.isOnFire == true)
                    {
                        riskCalculation();
                        if (chanceOfFire <= riskOfFireSpread)
                        {
                            //Debug.Log("WF: Inherit WF from children");
                            isOnFire = true;
                        }
                    }
                }
            }
        }

        //Calculate for COLLISIONS
        private void onCollision(Part p, Collision c)
        {
            riskCalculation();
            if (this.part == p && c.relativeVelocity.magnitude > (this.part.crashTolerance * 0.9) && chanceOfFire <= riskOfFireBumping)
            {
                isOnFire = true;
            }
        }

        //Calculate for PART LOSS
        private void onPartDie(Part p)
        {
            //cacheParts(); 
            soundHandler();
            riskCalculation();
            if (p == cachedParent)
            {
                var pm = p.Modules.OfType<ModuleWildfire>().Single();
                pm = p.FindModulesImplementing<ModuleWildfire>().First();
                double riskOfFireExplosionsFinal = (riskOfFireExplosions + (pm.riskOfFireExplosions/2)); 
                if (chanceOfFire <= riskOfFireExplosionsFinal)
                {
                    //Debug.Log("WF: Inherit WF from parent EXPLOSION");
                    isOnFire = true;
                }
            }
           
            foreach (Part pt in cachedChildren)
            {
                if (p == pt)
                {
                    var pm = p.Modules.OfType<ModuleWildfire>().Single();
                    pm = p.FindModulesImplementing<ModuleWildfire>().First();
                    double riskOfFireExplosionsFinal = (riskOfFireExplosions + (pm.riskOfFireExplosions / 2)); 
                    if (chanceOfFire <= riskOfFireExplosionsFinal)
                    {
                        isOnFire = true;
                    }
                }
            }
        }

        //Fire function
        private void combust()
        {
            bool toggler = false;
            if (isOnFire)
            { 
                this.part.skinTemperature = previousTemp + (this.part.skinMaxTemp / 500);
                previousTemp = this.part.skinTemperature;
                if (!toggler)
                {
                    smoke();
                    toggler = true;
                }
            }
            if (!isOnFire)
            {
                previousTemp = this.part.skinTemperature;
                if (toggler)
                {
                    smoke();
                    toggler = false;
                }
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
            if (this.part.Modules.Contains("ModuleDecouple") || this.part.Modules.Contains("ModuleAnchoredDecoupler"))
            {
                isDecoupler = true;
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
        */
        private void activeCrewRiskReduction()
        {
            float crewCounter = 0;
            double expRiskReduction = 0;

            if (this.part.vessel.GetCrewCount() != 0)
            {
                foreach (ProtoCrewMember pcm in this.part.vessel.GetVesselCrew())
                {
                    crewCounter += 1;

                    if (pcm.experienceLevel == 0)
                    {
                        expRiskReduction = 1;
                    }
                    if (pcm.experienceLevel == 1)
                    {
                        expRiskReduction = 0.01;
                    }
                    if (pcm.experienceLevel == 2)
                    {
                        expRiskReduction = 0.02;
                    }
                    if (pcm.experienceLevel == 3)
                    {
                        expRiskReduction = 0.03;
                    }
                    if (pcm.experienceLevel == 4)
                    {
                        expRiskReduction = 0.04;
                    }
                    if (pcm.experienceLevel == 5)
                    {
                        expRiskReduction = 0.05;
                    }
                    if (pcm.isBadass)
                    {
                        expRiskReduction *= 2;
                    }
                    /*
                    if (pcm.trait.Contains("Engineer"))
                    {
                        //improve improve ability to douse
                    }
                    if (pcm.trait.Contains("Pilot"))
                    {
                        //reduce risk of fire
                    }
                    if (pcm.trait.Contains("Scientest"))
                    {
                        //get
                    }*/

                    if (crewCounter == this.part.vessel.GetCrewCount())
                    {
                        crewCounter = 0;
                    }
                }
            }
        }
     /*
        private void weatherRisk()
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

        //WIP
        private void ModuleTypeRisk()
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

        public bool visibleFire = false;

        private void riskCalculation()
        {
            double addedRisk = 0;
            double riskMultiplier = 1;

            //atmospheric risk
            CelestialBody CB = this.part.vessel.mainBody;
            if (CB.atmosphere == true)
            {
                if (this.part.vessel.altitude < CB.atmosphereDepth)
                {
                    //Weather Risk
                    //riskMultiplier -= (currentWeatherType*5);
                    //Atmospheric Risk
                    if (CB.atmosphereContainsOxygen)
                    {
                        riskMultiplier += 0.2;
                        visibleFire = true;
                    }
                    else
                    {
                        visibleFire = false;
                    }
                }
            }

            //check for fules
            float resourceCounter = 0;
            double oxidizerRisk = 0;
            double liquidFuelRisk = 0;
            double monoPropellantRisk = 0;

            if (this.part.Resources.Count > 0)
            {
                foreach (PartResource r in this.part.Resources)
                {
                    resourceCounter += 1;
                    if (r.resourceName.Contains("Oxidizer") && r.amount > 0)
                    {
                        oxidizerRisk = (r.amount / r.maxAmount * 20);
                    }
                    if (r.resourceName.Contains("LiquidFuel") && r.amount > 0)
                    {
                        liquidFuelRisk = (r.amount / r.maxAmount * 10);
                    }
                    if (r.resourceName.Contains("MonoPropellant") && r.amount > 0)
                    {
                        monoPropellantRisk = (r.amount / r.maxAmount * 20);
                    }
                    if (resourceCounter == this.part.Resources.Count)
                    {
                        addedRisk = (monoPropellantRisk + liquidFuelRisk + oxidizerRisk);
                        resourceCounter = 0;
                        oxidizerRisk = 0;
                        liquidFuelRisk = 0;
                        monoPropellantRisk = 0;
                    }
                }
            }

            riskOfFireOverHeat = (baseRiskOfFireOverHeat + addedRisk) * riskMultiplier;
            riskOfFireSpread = (baseRiskOfFireSpread + addedRisk) * riskMultiplier;
            riskOfFireExplosions = (baseRiskOfFireExplosions + addedRisk) * riskMultiplier;
            riskOfFireBumping = (baseRiskOfFireBumping + addedRisk) * riskMultiplier;
            addedRisk = 0;
            riskMultiplier = 1;
        }
        
        //Visuals
        private void visualQue()
        {
            bool toggler = false;
            if (isOnFire)
            {
                this.part.SetHighlightDefault();
                this.part.SetHighlightColor(Color.red);
                this.part.SetHighlightType(Part.HighlightType.AlwaysOn);
                this.part.SetHighlight(true, false);
                soundHandler();               
                toggler = true;
            }
            if (!isOnFire && toggler == true)
            {
                this.part.SetHighlightDefault();
                soundHandler();              
                toggler = false;
            }
        }

        private void particleFx()
        {
            fireFx = (GameObject)GameObject.Instantiate(UnityEngine.Resources.Load("Effects/fx_exhaustFlame_yellow"));
            //fireFx.transform.parent = this.part.transform;
            fireFx.transform.position = this.part.transform.position;
            fireFx.particleEmitter.localVelocity = Vector3.zero;
            fireFx.particleEmitter.useWorldSpace = true;
            fireFx.particleEmitter.emit = false;
            fireFx.particleEmitter.minEnergy = 0;
            fireFx.particleEmitter.minEmission = 0;

            smokeFx = (GameObject)GameObject.Instantiate(UnityEngine.Resources.Load("Effects/fx_smokeTrail_light"));
            //smokeFx.transform.parent = this.part.transform;
            smokeFx.transform.position = this.part.transform.position;
            smokeFx.particleEmitter.localVelocity = Vector3.zero;
            smokeFx.particleEmitter.useWorldSpace = true;
            smokeFx.particleEmitter.emit = false;
            smokeFx.particleEmitter.minEnergy = 0;
            smokeFx.particleEmitter.minEmission = 0;

            sparkFx = (GameObject)GameObject.Instantiate(UnityEngine.Resources.Load("Effects/fx_exhaustSparks_flameout"));
            //sparkFx.transform.parent = this.part.transform;
            sparkFx.transform.position = this.part.transform.position;
            sparkFx.particleEmitter.localVelocity = Vector3.zero;
            sparkFx.particleEmitter.useWorldSpace = true;
            sparkFx.particleEmitter.emit = false;
            sparkFx.particleEmitter.minEnergy = 0;
            sparkFx.particleEmitter.minEmission = 0;
        }
        
        private void smoke()
        {
            if (isOnFire)
            {
                double temperatureRatio = (this.part.skinTemperature / this.part.skinMaxTemp) * 100;
                smokeFx.transform.position = this.part.transform.position;
                smokeFx.particleEmitter.maxEnergy = Convert.ToSingle(Math.Floor(temperatureRatio / 20));
                smokeFx.particleEmitter.maxEmission = Convert.ToSingle(Math.Floor(temperatureRatio / 20));
                smokeFx.particleEmitter.maxSize = this.part.transform.localScale.sqrMagnitude / 100000;
                smokeFx.particleEmitter.Emit();  

                sparkFx.transform.position = this.part.transform.position;
                sparkFx.particleEmitter.maxEnergy = Convert.ToSingle(Math.Floor(temperatureRatio / 20));
                sparkFx.particleEmitter.maxEmission = Convert.ToSingle(Math.Floor(temperatureRatio / 20));
                sparkFx.particleEmitter.maxSize = this.part.transform.localScale.sqrMagnitude / 100000;
                sparkFx.particleEmitter.Emit();

                if (temperatureRatio >= 50)
                {
                    fireFx.transform.position = this.part.transform.position;
                    fireFx.particleEmitter.maxEnergy = Convert.ToSingle(Math.Floor(temperatureRatio / 80));
                    fireFx.particleEmitter.maxEmission = Convert.ToSingle(Math.Floor(temperatureRatio / 20));
                    fireFx.particleEmitter.minEnergy = Convert.ToSingle(Math.Floor(temperatureRatio / 160));
                    fireFx.particleEmitter.minEmission = Convert.ToSingle(Math.Floor(temperatureRatio / 40));
                    fireFx.particleEmitter.maxSize = this.part.transform.localScale.sqrMagnitude / 100000;
                    fireFx.particleEmitter.Emit();
                }
            }
            else
            {
                smokeFx.particleEmitter.emit = false;
                fireFx.particleEmitter.emit = false;
                sparkFx.particleEmitter.emit = false;
            }
        }

        private void soundFx()
        {
            fireAudio = this.part.gameObject.AddComponent<AudioSource>();
            fireAudio.volume = GameSettings.SHIP_VOLUME;
            fireAudio.clip = GameDatabase.Instance.GetAudioClip("NANA/Wildfire/Sounds/BurningSound");
            fireAudio.loop = true;
            fireAudio.dopplerLevel = 0;
            fireAudio.Stop();

            extinguishAudio = this.part.gameObject.AddComponent<AudioSource>();
            extinguishAudio.volume = GameSettings.SHIP_VOLUME;
            extinguishAudio.clip = GameDatabase.Instance.GetAudioClip("NANA/Wildfire/Sounds/ExtinguishSound");
            extinguishAudio.loop = false;
            extinguishAudio.dopplerLevel = 0;
            extinguishAudio.Stop();
        }

        public void soundHandler()
        {
            if (isOnFire && !fireAudio.isPlaying)
            {
                fireAudio.Play();
            }
            else if (isOnFire && fireAudio.isPlaying)
            {
                //donothing
            }
            else
            {
                fireAudio.Stop();
            }
        }

        //shortify this
        public void onVesselWasModified(Vessel v)
        {
            cacheParts();
        }
        public void onLaunch()
        {
            cacheParts();
        }
        public void onScenceChange()
        {
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
            cacheParts();
        }
        public void onVesselChange(Vessel v)
        {
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
            if (state == StartState.Editor) return;
        
            particleFx();
            soundFx();
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
