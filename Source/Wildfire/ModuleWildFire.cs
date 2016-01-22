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
        public double baseRisk = 1;
        public double overheatRiskMultiplier = 1.2;
        public double spreadRiskMultiplier = 1.1;
        public double explosionRiskMultiplier = 1.2;
        public double bumpingRiskMultiplier = 1.1;
        public double splashDamageRiskMultiplier = 1.1;
        public double jointRotationRiskMultiplier = 1.1;
        public double riskOfFireOverHeat;
        public double riskOfFireSpread;
        public double riskOfFireExplosions;
        public double riskOfFireBumping;
        public double riskOfFireSplashDamage;
        public double riskOfFireJointRotation;
        public double vesselElectricCharge = 0;
        public double vesselOxidizer = 0;
        public double vesselLiquidFuel = 0;
        public double vesselMonoprop = 0;
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
        public bool hasDecoupler = false;
        public bool hasRTG = false;
        public bool hasFuelCell = false;
        public bool isPaused = false;
        public bool isOnFire = false;
        public int parent = 0;
        public bool hasCrossfeed = false;   
        public bool hasOxidizerLine = false;
        public bool hasMonoPropLine = false;
        public bool hasLiquidFuelLine = false;
        public int hasPotentialLiquidFuelLineChild = 0;
        public int hasPotentialOxidizerLineChild = 0;
        public int hasPotentialMonopropLineChild = 0;
        public bool hasPotentialLiquidFuelLineParent = false;
        public bool hasPotentialOxidizerLineParent = false;
        public bool hasPotentialMonopropLineParent = false;
        public bool creakingSoundPlaying = false;
        public bool overRotation = false;
        public double riskSubstractionMultiplier = 1;
        public bool sprinklerActvated = false;
        public bool isWheel = false;
        public bool cooldown = false;
        public bool breakOK = true;
        public double highestBend;

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
            //Debug.Log("WF: " + this.part.name + " is caching connected parts");
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
                    this.part.RequestResource("ElectricCharge", 0.0005);
                }
            }
        }
        
        //Extra hazards
        private void extraHazards()
        {
            if (!isOnFire) return;
            if (isOnFire)
            {
                if (this.part.Modules.Contains("ModuleDecouple"))
                {
                    var pm = this.part.Modules.OfType<ModuleDecouple>().Single();
                    pm = this.part.FindModulesImplementing<ModuleDecouple>().First();
                    float dice = UnityEngine.Random.Range(0, 100);
                    if (dice == 1)
                    {
                        pm.Decouple();
                    }
                }
                if (this.part.Modules.Contains("ModuleAnchoredDecoupler"))
                {
                    var pm = this.part.Modules.OfType<ModuleAnchoredDecoupler>().Single();
                    pm = this.part.FindModulesImplementing<ModuleAnchoredDecoupler>().First();
                    float dice = UnityEngine.Random.Range(0, 100);
                    if (dice == 1)
                    {
                        pm.Decouple();
                    }
                }
            }
        }

        //Extinguishers
        private void extinguisher()
        {
            if (!isOnFire) return;
            if (isOnFire)
            {
                //Random luck
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
                if (!inOxygenAtmo && !hasOxidizer && (!hasOxidizerLine && vesselOxidizer == 0) && !hasCabin && !hasSolidFuel && (!hasMonoPropLine && vesselMonoprop == 0) && !hasMonoprop && !hasElectricCharge && !hasFuelCell)
                {
                    float dice3 = UnityEngine.Random.Range(0, 100);
                    if (dice < 90)
                    {
                        douse();
                        vacuumAudio.Play();
                    }
                }

                //Not carrying anything particulary combustable
                if (!hasOxidizer && (!hasOxidizerLine && vesselOxidizer == 0) && !hasCabin && !hasSolidFuel && (!hasMonoPropLine && vesselMonoprop == 0) && !hasMonoprop && (!hasLiquidFuelLine && vesselLiquidFuel == 0) && !hasLiquidFuel && !hasElectricCharge && !hasRTG && !hasDecoupler && !hasFuelCell)
                {
                    float dice3 = UnityEngine.Random.Range(0, 100);
                    if (dice < 60)
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
                if (tempThreshold >= 70 && isHeatshield == false)
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
                    if (pp.isOnFire == true && isHeatshield == false)
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
                    if (cpm.isOnFire == true && isHeatshield == false)
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

        
        //Check for SPLASH DAMAGE
        public void onSplashDamage(EventReport er)
        {
            float dice = UnityEngine.Random.Range(0, 100);
            if (er.origin == this.part && dice < riskOfFireSplashDamage && isHeatshield == false)
            {
                isOnFire = true;
            }
        }

        /*
        //Check for COLLISIONS
        public void OnCollisionEnter(Collision c)
        {
            if (1 == 1)
            {
                Debug.Log("WF:" + this.part.name + ", Tol:" + ((this.part.crashTolerance * 10) ) + " M: " + c.relativeVelocity.magnitude);
                float dice = UnityEngine.Random.Range(0, 100);
                if (c.relativeVelocity.magnitude > (this.part.crashTolerance * 0.9) && dice <= riskOfFireBumping && isHeatshield == false && isWheel == false)
                {
                    //Debug.Log("WF:" + this.part.name + ", Tol:" + (this.part.crashTolerance * 0.9) + " M: " + c.relativeVelocity.magnitude);
                    //isOnFire = true;
                }
            }
        }
        
        private float collisionTimerCurrent = 0f;
        private float collisionTimerTotal = 2f;
        public void fOnCollisionStay(Collision c)
        {            
            collisionTimerCurrent += Time.deltaTime;
            if (collisionTimerCurrent >= collisionTimerTotal)
            {
                collisionTimerCurrent -= collisionTimerTotal;
                float dice = UnityEngine.Random.Range(0, 100);
                if (c.relativeVelocity.magnitude > (this.part.crashTolerance * 10) && dice <= riskOfFireBumping && isHeatshield == false && isWheel == false)
                {
                    Debug.Log("WF: IS COLISSION Stay" +this.part.name);
                    isOnFire = true;
                }
            }
        }
        */
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
                    if (dice <= riskOfFireExplosions && isHeatshield == false)
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
                this.part.skinTemperature = previousTemp + (this.part.skinMaxTemp / (10000 / (totalAddedRisk / 2)));
                previousTemp = this.part.skinTemperature;
                if (fireAudio != null)
                {
                    if (!fireAudio.isPlaying)
                    {
                        fireAudio.Play();
                    }
                }
                this.part.RequestResource("ElectricCharge", 0.001);

                if (hasLiquidFuel | (hasLiquidFuelLine && vesselLiquidFuel > 0))
                {
                    this.part.RequestResource("LiquidFuel", 0.05);
                }
                if (hasMonoprop | (hasMonoPropLine && vesselMonoprop > 0))
                {
                    this.part.RequestResource("MonoPropellant", 0.05);
                }
                if (hasOxidizer | (hasOxidizerLine && vesselOxidizer > 0))
                {
                    this.part.RequestResource("Oxidizer", 0.05);
                }
                if (hasSolidFuel)
                {
                    this.part.RequestResource("SolidFuel", 0.05);
                }
            }
        }

        //Douse fire function
        public void douse()
        {
            isOnFire = false;
            previousTemp = this.part.skinTemperature;
            if (fireAudio != null)
            {
                if (fireAudio.isPlaying && fireAudio != null)
                {
                    fireAudio.Stop();
                }
            }
        }
    
        //UNDER CONSTRUCTION      

        public float creakingSoundVolume = 1;
        Quaternion partRot;
        Quaternion parentRot;

        //[KSPField(guiActive = true, guiActiveEditor = false, isPersistant = false, guiName = "X")]
        public double offsetX;

        //[KSPField(guiActive = true, guiActiveEditor = false, isPersistant = false, guiName = "Y")]
        public double offsetY;

        //[KSPField(guiActive = true, guiActiveEditor = false, isPersistant = false, guiName = "Z")]
        public double offsetZ;

        //[KSPField(guiActive = true, guiActiveEditor = false, isPersistant = false, guiName = "W")]
        public double offsetW;

        public double prevOffsetX;
        public double prevOffsetY;
        public double prevOffsetZ;
        public double prevOffsetW;

        //[KSPField(guiActive = true, guiActiveEditor = false, isPersistant = false, guiName = "tX")]
        public double timeOffsetX;

        //[KSPField(guiActive = true, guiActiveEditor = false, isPersistant = false, guiName = "tY")]
        public double timeOffsetY;

        //[KSPField(guiActive = true, guiActiveEditor = false, isPersistant = false, guiName = "tZ")]
        public double timeOffsetZ;

        //[KSPField(guiActive = true, guiActiveEditor = false, isPersistant = false, guiName = "tW")]
        public double timeOffsetW;

        private float hightlighterTimerCurrent = 0f;
        private float highlighterTimerTotal = 1f;
        public bool highlighterEnabled = false;
        private float breakingTimerCurrent = 0f;
        private float breakingTimerTotal = 1f;

        public void breakingStatup()
        {
            if (!this.part.vessel.HoldPhysics && breakOK == false)
            {

                partRot = this.part.transform.localRotation * this.part.orgRot.Inverse();
                if (this.part.parent == null)
                {
                    parentRot = this.part.transform.localRotation * this.part.orgRot.Inverse();
                }
                else
                {
                    if (this.part.parent.PhysicsSignificance == 1)
                    {
                        parentRot = this.part.transform.localRotation * this.part.orgRot.Inverse();
                    }
                    else
                    {
                        parentRot = this.part.parent.transform.localRotation * this.part.parent.orgRot.Inverse();
                    }
                }
                offsetX = Math.Abs(parentRot.x - partRot.x);
                offsetY = Math.Abs(parentRot.y - partRot.y);
                offsetZ = Math.Abs(parentRot.z - partRot.z);
                offsetW = Math.Abs(parentRot.w - partRot.w);
                timeOffsetX = Math.Abs(offsetX - prevOffsetX);
                timeOffsetY = Math.Abs(offsetY - prevOffsetY);
                timeOffsetZ = Math.Abs(offsetZ - prevOffsetZ);
                timeOffsetW = Math.Abs(offsetW - prevOffsetW);

                prevOffsetX = offsetX;
                prevOffsetY = offsetY;
                prevOffsetZ = offsetZ;
                prevOffsetW = offsetW;

            }
        }

        

        private void breakingCheck()
        {
            if (breakOK == true)
            if (!this.part.vessel.HoldPhysics)
            {
                partRot = this.part.transform.localRotation * this.part.orgRot.Inverse();               
                if (this.part.parent == null)
                {
                    parentRot = this.part.transform.localRotation * this.part.orgRot.Inverse();
                }
                else
                {
                    if (this.part.parent.PhysicsSignificance == 1)
                    {
                        parentRot = this.part.transform.localRotation * this.part.orgRot.Inverse();
                    }
                    else
                    {
                        parentRot = this.part.parent.transform.localRotation * this.part.parent.orgRot.Inverse();

                        offsetX = Math.Abs(parentRot.x - partRot.x);
                        offsetY = Math.Abs(parentRot.y - partRot.y);
                        offsetZ = Math.Abs(parentRot.z - partRot.z);
                        offsetW = Math.Abs(parentRot.w - partRot.w);
                        timeOffsetX = Math.Abs(offsetX - prevOffsetX);
                        timeOffsetY = Math.Abs(offsetY - prevOffsetY);
                        timeOffsetZ = Math.Abs(offsetZ - prevOffsetZ);
                        timeOffsetW = Math.Abs(offsetW - prevOffsetW);

                        
                        if (timeOffsetX > 0.004 | timeOffsetY > 0.004 | timeOffsetZ > 0.004)
                        {
                            highestBend = (Math.Max(Math.Max(timeOffsetX, timeOffsetY), timeOffsetZ));
                            creakingSoundVolume = 3;
                            creakingSoundPlaying = true;
                            if ((timeOffsetX > 0.005 && timeOffsetX < 0.1) | (timeOffsetY > 0.005 && timeOffsetY < 0.1) | (timeOffsetZ > 0.005 && timeOffsetZ < 0.1))
                            {
                                creakingSoundVolume = 5;
                                highlighterEnabled = true;
                                if (cooldown == false)
                                {
                                    float dice = UnityEngine.Random.Range(0, 100);
                                    if (dice <= riskOfFireJointRotation && !this.part.vessel.HoldPhysics)
                                    {
                                        isOnFire = true;
                                        cooldown = true;
                                    }
                                } 
                            }
                        }
                        else
                        {
                            creakingSoundPlaying = false;
                        }
                        prevOffsetX = offsetX;
                        prevOffsetY = offsetY;
                        prevOffsetZ = offsetZ;
                        prevOffsetW = offsetW;
                    }
                }
                if (highlighterEnabled == true)
                {
                    this.part.SetHighlightDefault();
                    this.part.SetHighlightColor(Color.blue);
                    this.part.SetHighlightType(Part.HighlightType.AlwaysOn);
                    this.part.SetHighlight(true, false);
                    hightlighterTimerCurrent += Time.deltaTime;
                    if (hightlighterTimerCurrent >= highlighterTimerTotal)
                    {
                        hightlighterTimerCurrent -= highlighterTimerTotal;
                        this.part.SetHighlightDefault();
                        highlighterEnabled = false;
                    }
                }           
            }            
        }
        
        
        public void coolDownTimer()
        {
            if (cooldown)
            {
                breakingTimerCurrent += Time.deltaTime;
                if (breakingTimerCurrent >= breakingTimerTotal)
                {
                    breakingTimerCurrent -= breakingTimerTotal;
                    cooldown = false;
                }
            }
        }

        /*
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

        public float riskTimerCurrent = 0f;
        public float riskTimerTotal = 10f;
 
        private void riskReductionCountdown()
        {
            if (!sprinklerActvated) return;
            if (sprinklerActvated)
            {
                riskSubstractionMultiplier = 0.5;
                riskTimerCurrent += Time.deltaTime;
                if (riskTimerCurrent >= riskTimerTotal)
                {
                    riskTimerCurrent -= riskTimerTotal;
                    riskSubstractionMultiplier = 1;
                    sprinklerActvated = false;
                }
            }
        }

        private void launchpadRiskReduction()
        {
            if (!this.part.vessel.LandedOrSplashed) return;
            if (this.part.vessel.LandedOrSplashed)
            {
                foreach (Part p in this.part.vessel.parts)
                {
                    if (p.Modules.Contains("ModuleSprinkler"))
                    {
                        var pm = p.Modules.OfType<ModuleSprinkler>().Single();
                        pm = p.FindModulesImplementing<ModuleSprinkler>().First();
                        if (pm.isSafe == true)
                        {
                            sprinklerActvated = true;
                        }
                    }
                }
            }
        }

        double dynamicPressureRisk;
        double gforceRisk;

        private void riskCalculation()
        {
            double addedRisk = 0;
            double riskMultiplier = 1;

            dynamicPressureRisk = 1 + ((this.part.vessel.srf_velocity.sqrMagnitude * this.part.vessel.atmDensity / 2) * 0.000002);
          
            gforceRisk = 1 + (this.part.vessel.geeForce * 0.02);

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

            //Add risk for decoupelrs
            if (this.part.Modules.Contains("ModuleDecouple") || this.part.Modules.Contains("ModuleAnchoredDecoupler"))
            {
                hasDecoupler = true;
                addedRisk += 5;
            }
            else
            {
                hasDecoupler = false;
            }

            //Add risk for RTG
            if (this.part.name.Contains("rtg"))
            {
                hasRTG = true;
                addedRisk += 10;
            }
            else
            {
                hasRTG = false;
            }

            //Add risk for fuel cell
            if (this.part.name.Contains("FuelCell") || this.part.name.Contains("FuelCellArray"))
            {
                hasFuelCell = true;
                addedRisk += 3;
            }
            else
            {
                hasFuelCell = false;
            }

            //Add risk for ISRU
            if (this.part.name.Contains("ISRU"))
            {            
                addedRisk += 4;
            }

            //Add risk for Goo
            if (this.part.name.Contains("GooExperiment"))
            {
                addedRisk += UnityEngine.Random.Range(0, 10);
            }

            //Add risk for materials
            if (this.part.name.Contains("science_module"))
            {
                addedRisk += 3;
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
            double electricChargeRisk = 0;
            if (this.part.Resources.Count > 0)
            {
                foreach (PartResource r in this.part.Resources)
                {
                    if (r.resourceName.Contains("ElectricCharge") && r.amount > 0)
                    {
                        hasElectricCharge = true;
                        electricChargeRisk = (r.amount / r.maxAmount * 5);
                        riskMultiplier += (r.amount / r.maxAmount * 0.2);
                    }
                    else
                    {
                        hasElectricCharge = false;
                    }
                    if (r.resourceName.Contains("Oxidizer") && r.amount > 0)
                    {
                        
                        hasOxidizer = true;
                        oxidizerRisk += (r.amount / r.maxAmount * 15);
                        riskMultiplier += (r.amount / r.maxAmount * 0.2);
                    }
                    else
                    {
                        hasOxidizer = false;
                    }
                    if (r.resourceName.Contains("LiquidFuel") && r.amount > 0)
                    {
                        hasLiquidFuel = true;
                        liquidFuelRisk += (r.amount / r.maxAmount * 10);
                    }
                    else
                    {
                        hasLiquidFuel = false;
                    }
                    if (r.resourceName.Contains("MonoPropellant") && r.amount > 0)
                    {
                        hasMonoprop = true;
                        monoPropellantRisk += (r.amount / r.maxAmount * 15);
                        riskMultiplier += (r.amount / r.maxAmount * 0.2);
                    }
                    else
                    {
                        hasMonoprop = false;
                    }
                    if (r.resourceName.Contains("SolidFuel") && r.amount > 0)
                    {
                        hasSolidFuel = true;
                        solidFuelRisk += (r.amount / r.maxAmount * 15);
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
                electricChargeRisk = 0;
            }

            //Add risk for fuel lines
            vesselElectricCharge = 0;
            vesselOxidizer = 0;
            vesselLiquidFuel = 0;
            vesselMonoprop = 0;
            foreach (Part p in this.part.vessel.parts)
            {
                if (p.Resources.Count > 0)
                {
                    foreach (PartResource pr in p.Resources)
                    {
                        if (pr.resourceName.Contains("ElectricCharge") && pr.amount > 0)
                        {
                            vesselElectricCharge += pr.amount;
                        }
                        if (pr.resourceName.Contains("Oxidizer") && pr.amount > 0)
                        {
                            vesselOxidizer += pr.amount;
                        }
                        if (pr.resourceName.Contains("LiquidFuel") && pr.amount > 0)
                        {
                            vesselLiquidFuel += pr.amount;
                        }
                        if (pr.resourceName.Contains("MonoPropellant") && pr.amount > 0)
                        {
                            vesselMonoprop += pr.amount;
                        }
                    }
                }
            }
            
            if (hasMonoPropLine && vesselMonoprop > 0)
            {
                addedRisk += 5;
                riskMultiplier += 0.05;
            }
            if (hasOxidizerLine && vesselOxidizer > 0)
            {
                addedRisk += 5;
                riskMultiplier += 0.05;
            }
            if (hasLiquidFuelLine && vesselLiquidFuel > 0)
            {
                addedRisk += 3;
                riskMultiplier += 0.03;
            }
            if (vesselElectricCharge > 0)
            {
                addedRisk += 5;
                riskMultiplier += 0.05;
            }

            //Final calculations           
            riskOfFireOverHeat = (((((baseRisk + addedRisk) * riskMultiplier) * overheatRiskMultiplier) * gforceRisk) * dynamicPressureRisk) * riskSubstractionMultiplier;
            riskOfFireSpread = (((((baseRisk + addedRisk) * riskMultiplier) * spreadRiskMultiplier) * gforceRisk) * dynamicPressureRisk) * riskSubstractionMultiplier;
            riskOfFireExplosions = (((((baseRisk + addedRisk) * riskMultiplier) * explosionRiskMultiplier) * gforceRisk) * dynamicPressureRisk) * riskSubstractionMultiplier;
            riskOfFireBumping = (((((baseRisk + addedRisk) * riskMultiplier) * bumpingRiskMultiplier) * gforceRisk) * dynamicPressureRisk) * riskSubstractionMultiplier;
            riskOfFireSplashDamage = ((((((baseRisk + addedRisk) / 2) * riskMultiplier) * splashDamageRiskMultiplier) * gforceRisk) * dynamicPressureRisk) * riskSubstractionMultiplier;
            riskOfFireJointRotation = ((((((baseRisk + addedRisk)) * riskMultiplier) * jointRotationRiskMultiplier) * gforceRisk) * dynamicPressureRisk) * riskSubstractionMultiplier;
            totalAddedRisk = Math.Floor((((((baseRisk + addedRisk) * riskMultiplier) * explosionRiskMultiplier) * gforceRisk) * dynamicPressureRisk) * riskSubstractionMultiplier);
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
            fireLight.intensity = 0f;
            fireLight.range = 0f;
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
                    if (inOxygenAtmo || hasOxidizer || hasCabin || hasMonoprop || (hasMonoPropLine && vesselMonoprop > 0) || (hasOxidizerLine && vesselOxidizer > 0))
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
            if (/*this.part.Modules.Contains("ModuleAsteroid") ||*/ this.part.Modules.Contains("CModuleStrut") || this.part.Modules.Contains("CModuleFuelLine") || this.part.Modules.Contains("KerbalSeat") || this.part.PhysicsSignificance == 1)
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
            if (this.part.fuelCrossFeed)
            {
                hasCrossfeed = true;
            }
            if (this.part.Modules.Contains("ModuleWheel") | this.part.Modules.Contains("ModuleLandingGear"))
            {
                isWheel = true;
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
                extraHazards();
                breakingStatup();
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
            breakOK = false;
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
                breakingCheck();
                coolDownTimer();
                runVisualFX();
                extinguisherFX();
                powerUsage();
                launchpadRiskReduction();
                riskReductionCountdown();
                checkParts();
            }
        }

        public override void OnStart(PartModule.StartState state)
        {
            if (state == StartState.Editor || state == StartState.None) return;
            excludeParts();
            if (!isExcluded)
            {
                timerCurrent = UnityEngine.Random.Range(0f, 2f);
                setupVisualFx();
                setupAudio();
                checkExtinguisherStatus(); 
                riskCalculation();
                cacheParts();
                GameEvents.onPartDie.Add(onPartDie);
                GameEvents.onGamePause.Add(onGamePause);
                GameEvents.onGameUnpause.Add(onGameUnpause);
                GameEvents.onVesselWillDestroy.Add(onVesselWillDestroy);
                GameEvents.onSplashDamage.Add(onSplashDamage);
                //GameEvents.onLaunch.Add(onLaunch);
                //GameEvents.onPartUnpack.Add(onPartUnpack);
                //GameEvents.onFlightReady.Add(onFlightReady);
                //GameEvents.onVesselGoOffRails.Add(onVesselGoOffRails);
                //GameEvents.onVesselGoOnRails.Add(onVesselGoOnRails);
                //GameEvents.onCrash.Add(onCrash);
                //GameEvents.onEditorShipModified.Add(onEditiorShipModified);
                //GameEvents.onJointBreak.Add(onJointBreak);
                //GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded);
                //GameEvents.onPartExplode.Add(onPartExplode);
                //GameEvents.onPartJointBreak.Add(onPartJointBreak);                
                //GameEvents.onStageSeparation.Add(onStageSeparation);
                //GameEvents.onPartUndock.Add(onPartUndock);
                //GameEvents.onVesselChange.Add(onVesselChange);
                //GameEvents.onVesselLoaded.Add(onVesselLoaded);
                //GameEvents.onVesselWasModified.Add(onVesselWasModified);   
                
            }
            base.OnStart(state);
        }
    }
}
