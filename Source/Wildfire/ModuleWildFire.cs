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
        public bool hasFireExtinguisher = false;
        public bool autoExtinguisherIsOn = false;
        public bool isDecoupler = false;
        public bool isHeatshield = false;
        public bool isExcluded = true;
        public bool hasMonoPropLine = false;
        public double baseRiskOfFireOverHeat = 20;
        public double baseRiskOfFireSpread = 10;
        public double baseRiskOfFireExplosions = 20;
        public double baseRiskOfFireBumping = 10;
        public double riskOfFireOverHeat;
        public double riskOfFireSpread;
        public double riskOfFireExplosions;
        public double riskOfFireBumping;
        public double previousTemp = 1;
        public bool inOxygenAtmo = false;
        public bool inAtmosphere = false;
        public bool hasOxidizer = false;
        public bool hasMonoprop = false;
        public bool hasCabin = false;
        public bool hasEngine = false;
        public bool hasSolidFuel = false;
        public bool hasLiquidFuel = false;
        public bool hasElectricCharge = false;
        public bool isPaused = false;
        public bool isOnFire = false;
        public int parent = 0;

        [KSPField(guiActive = true, guiActiveEditor = false, isPersistant = false, guiName = "Risk")]
        public double totalAddedRisk = 0;

        private GameObject smokeFx;
        private GameObject fireFx;
        private GameObject sparkFx;
        private GameObject extinguisherFx;
        private Light fireLight;
        private Color lightColorYellow = new Color(240, 184, 49);
        private Color lightColorRed = new Color(237, 49, 12);
        private Color lightColorWhite = new Color(255, 255, 255);

        public AudioSource fireAudio;
        public AudioSource extinguishAudio;
        public AudioSource hissAudio;
        public AudioSource vacuumAudio;

        public List<Part> cachedParts;
       
        //For use with onPartDie
        public void checkParts()
        {
            if (cachedParts.Count == (this.part.children.Count + parent)) return;
            cacheParts();
        }

        public void cacheParts()
        {
            Debug.Log("WF: " + this.part.name + " is caching connected parts");
            cachedParts = new List<Part>();
            if (this.part.parent != null)
            {
                cachedParts.Add(this.part.parent);
                parent = 1;
            }
            else
            {
                parent = 0;
            }
            if (this.part.children.Count > 0)
            {
                foreach (Part p in this.part.children)
                {
                    cachedParts.Add(p);
                }
            } 
        }
        
        //Check if extinguisher is attached
        private void checkExtinguisherStatus()
        {
            int extinguisherCount = 0;
            int autoExtinguisherCount = 0;
            foreach (Part p in this.part.vessel.parts)
            {              
                if (p.Modules.Contains("ModuleFireExtinguisher"))
                {
                    extinguisherCount += 1;
                    var pp = p.Modules.OfType<ModuleFireExtinguisher>().Single();
                    pp = p.FindModulesImplementing<ModuleFireExtinguisher>().First();
                    if (pp.autoActivated)
                    {
                        autoExtinguisherCount += 1;
                    } 
                }
            }
            if (extinguisherCount > 0)
            {
                hasFireExtinguisher = true;
                if (autoExtinguisherCount > 0)
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
            extinguisherCount = 0;
            autoExtinguisherCount = 0;
        }

        //Extinguisher power usage
        private void powerUsage()
        {
            if (!hasFireExtinguisher) return;
            if (hasFireExtinguisher)
            {
                if (autoExtinguisherIsOn)
                {
                    this.part.RequestResource("ElectricCharge", 0.01);
                }
            }
        }
        
        //Extinguishers
        private void extinguisher()
        {
            if (!isOnFire) return;
            if (isOnFire)
            {
                //Any random luck
                float dice = UnityEngine.Random.Range(0, 100);
                if (dice == 1)
                {
                    douse();
                    vacuumAudio.Play();
                }

                //Submerged in water
                double dice2 = Convert.ToDouble(UnityEngine.Random.Range(0, 100) / 100);
                if (hasCabin && hasOxidizer && hasMonoprop && hasSolidFuel && this.part.WaterContact)
                {
                    if ((this.part.submergedPortion / 2) > dice2)
                    {
                        douse();
                        hissAudio.Play();
                    }
                }
                else
                {
                    if (this.part.submergedPortion > dice2 && this.part.WaterContact)
                    {
                        douse();
                        hissAudio.Play();
                    }
                }

                //In vacuum
                if (!inOxygenAtmo && !hasOxidizer && !hasCabin && !hasSolidFuel && !hasMonoPropLine && !hasMonoprop)
                {
                    float dice3 = UnityEngine.Random.Range(0, 100);
                    if (dice < 50)
                    {
                        douse();
                        vacuumAudio.Play();
                    }
                }

                //Not carrying anything particulary combustable
                if (!hasOxidizer && !hasCabin && !hasSolidFuel && !hasMonoPropLine && !hasMonoprop && !hasLiquidFuel && !hasLiquidFuel && !hasElectricCharge)
                {
                    float dice3 = UnityEngine.Random.Range(0, 100);
                    if (dice < 5)
                    {
                        douse();
                        vacuumAudio.Play();
                    }
                }

                //Fire extinguisher part function
                //Idea: integrate LF mod to dump oxygen instead.
                if (hasFireExtinguisher && autoExtinguisherIsOn)
                {
                    double totalWater = 0;
                    double totalCharge = 0;
                    foreach (Part p in this.part.vessel.parts)
                    {
                        if (p.Resources.Contains("LqdCO2") && p.Resources["LqdCO2"].amount > 0)
                        {
                            totalWater += p.Resources["LqdCO2"].amount;
                        }
                        if (p.Resources.Contains("ElectricCharge") && p.Resources["ElectricCharge"].amount > 0)
                        {
                            totalCharge += p.Resources["ElectricCharge"].amount;
                        }
                    }
                    if (totalWater >= (this.part.mass * 20) && totalCharge >= (this.part.mass * 10))
                    {
                        this.part.RequestResource("LqdCO2", (this.part.mass * 20));
                        this.part.RequestResource("ElectricCharge", (this.part.mass * 10));
                        extinguishAudio.Play();
                        int dice3 = UnityEngine.Random.Range(0, 100);
                        if (dice3 <= 80)
                        {
                            douse();
                        }
                    }
                    totalWater = 0;
                    totalCharge = 0;
                }
            }
        }

        private void ignitors()
        {
            if (!isOnFire)
            {
                //Check for overheating
                double tempThreshold = ((this.part.skinTemperature / this.part.skinMaxTemp) * 100);
                if (tempThreshold >= 70)
                {
                    float dice = UnityEngine.Random.Range(0, 100);
                    if (dice <= riskOfFireOverHeat)
                    {
                        isOnFire = true;
                    }
                }

                //Check for spreading from adjacent parts
                if (this.part.parent != null)
                {
                    var pp = part.parent.Modules.OfType<ModuleWildfire>().Single();
                    pp = part.parent.FindModulesImplementing<ModuleWildfire>().First();
                    if (pp.isOnFire == true)
                    {
                        float dice = UnityEngine.Random.Range(0, 100);
                        if (dice <= riskOfFireSpread)
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
                        float dice = UnityEngine.Random.Range(0, 100);
                        if (dice <= riskOfFireSpread)
                        {
                            isOnFire = true;
                        }
                    }
                }
            }
        }   

        //Check for COLLISIONS
        public void fOnCollisionEnter(Collision c)
        {
            float dice = UnityEngine.Random.Range(0, 100);       
            if (c.relativeVelocity.magnitude > (this.part.crashTolerance * 0.9) && dice <= riskOfFireBumping)
            {                
                isOnFire = true;
            }
        }

        //Check for PART LOSS
        private void onPartDie(Part p)
        {          
            foreach (Part pt in cachedParts)
            {       
                if (p == pt)
                {                   
                    var pm = pt.Modules.OfType<ModuleWildfire>().Single();
                    pm = pt.FindModulesImplementing<ModuleWildfire>().First();
                    double riskOfFireExplosionsFinal = (riskOfFireExplosions + (pm.riskOfFireExplosions / 2));
                    float dice = UnityEngine.Random.Range(0, 100);
                    if (dice <= riskOfFireExplosions)
                    {
                        isOnFire = true;
                    }
                }
            }
        }

        //Fire function
        private void combust()
        {
            if (isOnFire == false) return;
            if (isOnFire == true) 
            { 
                this.part.skinTemperature = previousTemp + (this.part.skinMaxTemp / (16000 / totalAddedRisk));
                previousTemp = this.part.skinTemperature;
                if (fireAudio != null)
                {
                    if (!fireAudio.isPlaying)
                    {
                        fireAudio.Play();
                    }
                }        
            }
        }

        //Douse fire function
        public void douse()
        {
            isOnFire = false;
            previousTemp = this.part.skinTemperature;
            if (fireAudio.isPlaying && fireAudio != null)
            {
                fireAudio.Stop();
            }
        }
    
        //UNDER CONSTRUCTION
        /*
        [KSPField(guiActive = true, guiActiveEditor = false, isPersistant = false, guiName = "Test")]
        public float initial;

        //Notwroking
        private void breakingCheck()
        {
            float initial = this.part.attachJoint.Joint.axis.x;
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
                    }

                    if (crewCounter == this.part.vessel.GetCrewCount())
                    {
                        crewCounter = 0;
                    }
                }
            }
        }

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
        //Ideas:
          //decupler explosives
         //fuel cell risk
         //rtg risk
           //Weather Risk
           //riskMultiplier -= (currentWeatherType*5);
           //Atmospheric Risk
         //solar flare chance
      //explosion light
        */
        
        //Calculate Risk of part
        //Make more realistic
        private void riskCalculation()
        {
            double addedRisk = 0;
            double riskMultiplier = 1;

            //Add risk for fuel fuel lines
            //Only Monoprop functioning at the moment
            if (hasMonoPropLine)
            {
                addedRisk += 5;
                riskMultiplier += 0.05;
            }

            //Add risk for cabin
            if (this.part.CrewCapacity > 0)
            {
                hasCabin = true;
                riskMultiplier += 0.2;
            }
            else
            {
                hasCabin = false;
            }

            //Add risk for engine
            if (this.part.Modules.Contains("ModuleEnginesFX") || this.part.Modules.Contains("ModuleEngines"))
            {
                hasEngine = true;
                addedRisk += 10;
            }
            else
            {
                hasEngine = false;
            }

            //Add atmospheric risk
            CelestialBody CB = this.part.vessel.mainBody;
            if (CB.atmosphere == true)
            {
                if (this.part.vessel.altitude < CB.atmosphereDepth)
                {
                    inAtmosphere = true;
                    if (CB.atmosphereContainsOxygen)
                    {
                        riskMultiplier += 0.2;
                        inOxygenAtmo = true;
                    }
                    else
                    {
                        inOxygenAtmo = false;
                    }
                }
                else
                {
                    inAtmosphere = false;
                }
            }

            //Check for resource containers
            double oxidizerRisk = 0;
            double liquidFuelRisk = 0;
            double monoPropellantRisk = 0;
            double solidFuelRisk = 0;
            //double electricChargeRisk = 0;
            if (this.part.Resources.Count > 0)
            {
                foreach (PartResource r in this.part.Resources)
                {
                    if (r.resourceName.Contains("ElectricCharge") && r.amount > 0)
                    {
                        hasElectricCharge = true;
                        //electricChargeRisk = (r.amount / r.maxAmount * 10);
                        riskMultiplier += (r.amount / r.maxAmount * 0.05);
                    }
                    else
                    {
                        hasElectricCharge = false;
                    }
                    if (r.resourceName.Contains("Oxidizer") && r.amount > 0)
                    {
                        
                        hasOxidizer = true;
                        oxidizerRisk += (r.amount / r.maxAmount * 20);
                        riskMultiplier += (r.amount / r.maxAmount * 0.2);
                    }
                    else
                    {
                        hasOxidizer = false;
                    }
                    if (r.resourceName.Contains("LiquidFuel") && r.amount > 0)
                    {
                        hasLiquidFuel = true;
                        liquidFuelRisk += (r.amount / r.maxAmount * 20);
                    }
                    else
                    {
                        hasLiquidFuel = false;
                    }
                    if (r.resourceName.Contains("MonoPropellant") && r.amount > 0)
                    {
                        hasMonoprop = true;
                        monoPropellantRisk += (r.amount / r.maxAmount * 20);
                        riskMultiplier += (r.amount / r.maxAmount * 0.2);
                    }
                    else
                    {
                        hasMonoprop = false;
                    }
                    if (r.resourceName.Contains("SolidFuel") && r.amount > 0)
                    {
                        hasSolidFuel = true;
                        solidFuelRisk += (r.amount / r.maxAmount * 10);
                        riskMultiplier += (r.amount / r.maxAmount * 0.1);
                    }
                    else
                    {
                        hasSolidFuel = false;
                    }
                }
                addedRisk += (monoPropellantRisk + liquidFuelRisk + oxidizerRisk + solidFuelRisk);
                oxidizerRisk = 0;
                liquidFuelRisk = 0;
                monoPropellantRisk = 0;
                solidFuelRisk = 0;
            }

            //Final calculations           
            riskOfFireOverHeat = (baseRiskOfFireOverHeat + addedRisk) * riskMultiplier;
            riskOfFireSpread = (baseRiskOfFireSpread + addedRisk) * riskMultiplier;
            riskOfFireExplosions = (baseRiskOfFireExplosions + addedRisk) * riskMultiplier;
            riskOfFireBumping = (baseRiskOfFireBumping + addedRisk) * riskMultiplier;
            totalAddedRisk = (baseRiskOfFireExplosions + addedRisk) * riskMultiplier;
            addedRisk = 0;
            riskMultiplier = 1;
        }

        //Set up visual effects
        private void setupVisualFx()
        {
            fireFx = (GameObject)GameObject.Instantiate(UnityEngine.Resources.Load("Effects/fx_exhaustFlame_yellow"));
            fireFx.transform.position = this.part.transform.position;
            fireFx.particleEmitter.localVelocity = Vector3.zero;
            fireFx.particleEmitter.useWorldSpace = true;
            fireFx.particleEmitter.emit = false;

            smokeFx = (GameObject)GameObject.Instantiate(UnityEngine.Resources.Load("Effects/fx_smokeTrail_light"));
            smokeFx.transform.position = this.part.transform.position;
            smokeFx.particleEmitter.localVelocity = Vector3.zero;
            smokeFx.particleEmitter.useWorldSpace = true;
            smokeFx.particleEmitter.emit = false;
            smokeFx.particleEmitter.minEnergy = 5;
            smokeFx.particleEmitter.minEmission = 5;

            sparkFx = (GameObject)GameObject.Instantiate(UnityEngine.Resources.Load("Effects/fx_exhaustSparks_flameout"));
            sparkFx.transform.position = this.part.transform.position;
            sparkFx.particleEmitter.localVelocity = Vector3.zero;
            sparkFx.particleEmitter.useWorldSpace = true;
            sparkFx.particleEmitter.emit = false;
            sparkFx.particleEmitter.minEnergy = 5;
            sparkFx.particleEmitter.minEmission = 5;

            extinguisherFx = (GameObject)GameObject.Instantiate(UnityEngine.Resources.Load("Effects/fx_exhaustFlame_white_tiny"));
            extinguisherFx.transform.position = this.part.transform.position;
            extinguisherFx.particleEmitter.localVelocity = Vector3.zero;
            extinguisherFx.particleEmitter.useWorldSpace = true;
            extinguisherFx.particleEmitter.emit = false;

            fireLight = sparkFx.AddComponent<Light>();
            fireLight.type = LightType.Point;
            fireLight.shadows = LightShadows.Hard;
            fireLight.enabled = false;
        }
        
        //Run visual effects
        private void runVisualFX()
        {
            if (!isOnFire) return;
            if (isOnFire)
            {
                //Idea: add bubbles for underwater
                double temperatureRatio = (this.part.skinTemperature / this.part.skinMaxTemp) * 100;

                sparkFx.transform.position = this.part.transform.position;
                sparkFx.particleEmitter.maxEnergy = Convert.ToSingle(Math.Floor(temperatureRatio / 400));
                sparkFx.particleEmitter.maxEmission = Convert.ToSingle(Math.Floor(temperatureRatio / 400));
                sparkFx.particleEmitter.maxSize = this.part.transform.localScale.sqrMagnitude / 100000;
                sparkFx.particleEmitter.Emit();
                
                if (inAtmosphere)
                {
                    smokeFx.transform.position = this.part.transform.position;
                    smokeFx.particleEmitter.maxEnergy = Convert.ToSingle(Math.Floor(temperatureRatio / 400));
                    smokeFx.particleEmitter.maxEmission = Convert.ToSingle(Math.Floor(temperatureRatio / 400));
                    smokeFx.particleEmitter.maxSize = this.part.transform.localScale.sqrMagnitude / 100000;
                    smokeFx.particleEmitter.Emit();  
                }
                else
                {
                    smokeFx.particleEmitter.emit = false;
                }

                if (temperatureRatio >= 50)
                {
                    if (inOxygenAtmo || hasOxidizer || hasCabin || hasMonoPropLine)
                    {
                        fireFx.transform.position = this.part.transform.position;
                        fireFx.particleEmitter.maxEnergy = Convert.ToSingle(Math.Floor(temperatureRatio / 60));
                        fireFx.particleEmitter.maxEmission = Convert.ToSingle(Math.Floor(temperatureRatio / 18));
                        fireFx.particleEmitter.minEnergy = Convert.ToSingle(Math.Floor(temperatureRatio / 140));
                        fireFx.particleEmitter.minEmission = Convert.ToSingle(Math.Floor(temperatureRatio / 35));
                        fireFx.particleEmitter.maxSize = this.part.transform.localScale.sqrMagnitude / 80000;
                        fireFx.particleEmitter.Emit();

                        fireLight.color = Color.Lerp(lightColorRed, lightColorYellow, UnityEngine.Random.Range(0f, 1f));
                    } 
                    else
                    {
                        fireLight.color = Color.Lerp(lightColorWhite, lightColorYellow, UnityEngine.Random.Range(0f, 1f));
                        fireFx.particleEmitter.emit = false;
                    }
                }
            }
            else
            {
                smokeFx.particleEmitter.emit = false;
                fireFx.particleEmitter.emit = false;
                sparkFx.particleEmitter.emit = false;
                
                if (fireLight != null)
                {
                    fireLight.enabled = false;
                }
            }
        }

        //Run extinguisher effect
        private void extinguisherFX()
        {
            if (!extinguishAudio.isPlaying || !hissAudio.isPlaying) return;
            if (extinguishAudio.isPlaying || hissAudio.isPlaying)
            {
                extinguisherFx.transform.position = this.part.transform.position;
                extinguisherFx.particleEmitter.maxEnergy = Convert.ToSingle(Math.Floor(this.part.mass * 15));
                extinguisherFx.particleEmitter.maxEmission = Convert.ToSingle(Math.Floor(this.part.mass * 60));
                extinguisherFx.particleEmitter.minEnergy = Convert.ToSingle(Math.Floor(this.part.mass * 8));
                extinguisherFx.particleEmitter.minEmission = Convert.ToSingle(Math.Floor(this.part.mass * 30));
                extinguisherFx.particleEmitter.maxSize = this.part.transform.localScale.sqrMagnitude / 10000;
                extinguisherFx.particleEmitter.Emit();
            }
            else
            {
                extinguisherFx.particleEmitter.emit = false;
            }
        }

        //Setup audio
        private void setupAudio()
        {
            fireAudio = gameObject.AddComponent<AudioSource>();
            fireAudio.volume = GameSettings.SHIP_VOLUME / 3;
            fireAudio.clip = GameDatabase.Instance.GetAudioClip("NANA/Wildfire/Sounds/BurningSound");
            fireAudio.loop = true;
            fireAudio.Stop();

            extinguishAudio = gameObject.AddComponent<AudioSource>();
            extinguishAudio.volume = GameSettings.SHIP_VOLUME / 3;
            extinguishAudio.clip = GameDatabase.Instance.GetAudioClip("NANA/Wildfire/Sounds/ExtinguishSound");
            extinguishAudio.loop = false;
            extinguishAudio.Stop();

            hissAudio = gameObject.AddComponent<AudioSource>();
            hissAudio.volume = GameSettings.SHIP_VOLUME / 3;
            hissAudio.clip = GameDatabase.Instance.GetAudioClip("NANA/Wildfire/Sounds/HissSound");
            hissAudio.loop = false;
            hissAudio.Stop();

            vacuumAudio = gameObject.AddComponent<AudioSource>();
            vacuumAudio.volume = GameSettings.SHIP_VOLUME / 3;
            vacuumAudio.clip = GameDatabase.Instance.GetAudioClip("NANA/Wildfire/Sounds/VacuumSound");
            vacuumAudio.loop = false;
            vacuumAudio.Stop();
        }

        //Exclude parts
        private void excludeParts()
        {
            if (this.part.Modules.Contains("ModuleAsteroid") || this.part.Modules.Contains("CModuleStrut") || this.part.Modules.Contains("CModuleFuelLine"))
            {
                isExcluded = true;
            }
            else
            {
                isExcluded = false;
            }
            if (this.part.Modules.Contains("ModuleDecouple") || this.part.Modules.Contains("ModuleAnchoredDecoupler"))
            {
                isDecoupler = true;
            }
            if (this.part.Modules.Contains("ModuleHeatshield"))
            {
                isHeatshield = true;
            }
        }

        //Ticker
        private float timerCurrent = 0f;
        private float timerTotal = 2f;
        private void tickHandler()
        {
            timerCurrent += Time.deltaTime;
            if (timerCurrent >= timerTotal)
            {              
                timerCurrent -= timerTotal;
                excludeParts();
                riskCalculation();
                ignitors();
                checkExtinguisherStatus();
                extinguisher();
            }
        }

        public void OnDestroy()
        {
            if (fireAudio != null)
            {
                fireAudio.Stop();
            }
            if (hissAudio != null)
            {
                hissAudio.Stop();
            }
            if (extinguishAudio != null)
            {
                extinguishAudio.Stop();
            }
            if (vacuumAudio != null)
            {
                vacuumAudio.Stop();
            }
            if (fireLight != null)
            {
                fireLight.enabled = false;
            }
            if (smokeFx != null)
            {
                smokeFx.particleEmitter.emit = false;
            }
            if (fireFx != null)
            {
                fireFx.particleEmitter.emit = false;
            }
            if (sparkFx != null)
            {
                sparkFx.particleEmitter.emit = false;
            }
            if (extinguisherFx != null)
            {
                extinguisherFx.particleEmitter.emit = false;
            }
            GameEvents.onGamePause.Remove(onGamePause);
            GameEvents.onGameUnpause.Remove(onGameUnpause);
        }

        public void onGamePause()
        {
            isPaused = true;
            if (fireAudio != null)
            {
                fireAudio.volume = 0;
            }
        }

        public void onGameUnpause()
        {
            isPaused = false;
            if (fireAudio != null)
            {
                fireAudio.volume = GameSettings.SHIP_VOLUME / 3;
            }
        }   
        public void onVesselWillDestroy(Vessel v)
        {
            if (v = this.part.vessel)
            {
                if (fireLight != null)
                {
                    fireLight.enabled = false;
                }
            }
        }

        /*
        //may need
        public void onVesselWasModified(Vessel v)
        {
        }
        public void onVesselLoaded(Vessel v)
        {
        }
        public void onVesselChange(Vessel v)
        {
        }
        public void onPartUndock(Part p)
        {
        }
        public void onStageSeparation(EventReport data)
        {    
        }
        public void onStageActivate(int i)
        {   
        }
        public void onSplashDamage(EventReport data)
        {    
        }
        public void onPartExplode(GameEvents.ExplosionReaction er)
        {       
        }       
        */
        public void FixedUpdate()
        {
            if (!isExcluded)
            {
                tickHandler();
                combust();
                //breakingCheck();
                runVisualFX();
                extinguisherFX();
                powerUsage();
                checkParts();
            }
        }

        public override void OnStart(PartModule.StartState state)
        {
            if (state == StartState.Editor || state == StartState.None) return;
            excludeParts();
            if (!isExcluded)
            {          
                setupVisualFx();
                setupAudio();
                checkExtinguisherStatus(); 
                riskCalculation();
                cacheParts();
                GameEvents.onPartDie.Add(onPartDie);
                GameEvents.onGamePause.Add(onGamePause);
                GameEvents.onGameUnpause.Add(onGameUnpause);
                GameEvents.onVesselWillDestroy.Add(onVesselWillDestroy);
                //GameEvents.onCrash.Add(onCrash);
                //GameEvents.onEditorShipModified.Add(onEditiorShipModified);
                //GameEvents.onJointBreak.Add(onJointBreak);
                //GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded);
                //GameEvents.onPartExplode.Add(onPartExplode);
                //GameEvents.onPartJointBreak.Add(onPartJointBreak);
                //GameEvents.onSplashDamage.Add(onSplashDamage);
                //GameEvents.onStageActivate.Add(onStageActivate);
                //GameEvents.onStageSeparation.Add(onStageSeparation);
                //GameEvents.onPartUndock.Add(onPartUndock);
                //GameEvents.onVesselChange.Add(onVesselChange);
                //GameEvents.onVesselLoaded.Add(onVesselLoaded);
                //GameEvents.onVesselWasModified.Add(onVesselWasModified);    
            }
            base.OnStart(state);
        }

        public override void OnAwake()
        {
            excludeParts();
            base.OnAwake();
        }
    }
}
