using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace wildfire
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ModuleFuleLineBuilder : MonoBehaviour
    {
        private void onVesselRollout(ShipConstruct sc)
        {
            Vessel v = FlightGlobals.ActiveVessel;
            checkFuelLines(v);
        }
        private void onVesselLoaded(Vessel v)
        {
            checkFuelLines(v);
        }
        private void onVesselWasModified(Vessel v)
        {
            checkFuelLines(v);
        }
        private void onLevelWasLoaded(GameScenes gs)
        {
            Vessel v = FlightGlobals.ActiveVessel;
            checkFuelLines(v);
        }
        private void onVesselChange(Vessel v)
        {
            if (v.parts.Count > 1 && v.isCommandable && v.IsControllable)
            {
                checkFuelLines(v);
            }
            
        }
        private void onVesselGoOffRails(Vessel v)
        {
            checkFuelLines(v);
        }
        private void onPartUndock(Part p)
        {
            Vessel v = p.vessel;
            checkFuelLines(v);
        }
        private void onPartUnpack(Part p)
        {
            Vessel v = p.vessel;
            checkFuelLines(v);
        }

        public void Start()
        {
            //GameEvents.onVesselWasModified.Add(onVesselWasModified);
            //GameEvents.OnVesselRollout.Add(onVesselRollout);
            //GameEvents.onUndock.Add(onUndock);
            GameEvents.onPartUndock.Add(onPartUndock);
            //GameEvents.onVesselLoaded.Add(onVesselLoaded);
            //GameEvents.onPartUndock.Add(onPartUndock);
            //checkFuelLines(FlightGlobals.ActiveVessel);
            //GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded);
            //GameEvents.onVesselChange.Add(onVesselChange);
            GameEvents.onVesselGoOffRails.Add(onVesselGoOffRails);
            //GameEvents.onPartUnpack.Add(onPartUnpack);
        }

        private void checkFuelLines(Vessel v)
        {
            if (v.isCommandable == false || v.IsControllable == false || v.isEVA) return;
            Debug.Log("Building Fuel Lines for " + v.name + ", Please stand by");
            foreach (Part p in v.parts)
            {
                if (p.Modules.Contains("ModuleWildfire") && p.fuelCrossFeed)
                {
                    var pm = p.Modules.OfType<ModuleWildfire>().Single();
                    pm = p.FindModulesImplementing<ModuleWildfire>().First();
                    //pm = p.Modules.OfType<ModuleWildfire>().Last();
                    if (p.Resources.Contains("MonoPropellant") || p.Modules.Contains("ModuleRCS") || p.Modules.Contains("ModuleDockingNode") || p.Modules.Contains("CModuleFuelLine"))
                    {
                        pm.hasMonoPropLine = true;
                    }
                    if (p.Resources.Contains("LiquidFuel") || p.Modules.Contains("ModuleDockingNode") || p.Modules.Contains("CModuleFuelLine"))
                    {
                        pm.hasLiquidFuelLine = true;
                    }
                    if (p.Resources.Contains("Oxidizer") || p.Modules.Contains("ModuleDockingNode") || p.Modules.Contains("CModuleFuelLine"))
                    {
                        pm.hasOxidizerLine = true;
                    }
                    if (p.Modules.Contains("ModuleEnginesFX"))
                    {
                        var en = p.Modules.OfType<ModuleEnginesFX>().Single();
                        en = p.FindModulesImplementing<ModuleEnginesFX>().First();
                        foreach (Propellant prop in en.propellants)
                        {
                            if (prop.name == "Oxidizer")
                            {
                                pm.hasOxidizerLine = true;
                            }
                            if (prop.name == "LiquidFuel")
                            {
                                pm.hasLiquidFuelLine = true;
                            }
                        }
                    }
                    if (p.Modules.Contains("ModuleEngines"))
                    {
                        var en = p.Modules.OfType<ModuleEngines>().Single();
                        en = p.FindModulesImplementing<ModuleEngines>().First();
                        foreach (Propellant prop in en.propellants)
                        {
                            if (prop.name == "Oxidizer")
                            {
                                pm.hasOxidizerLine = true;
                            }
                            if (prop.name == "LiquidFuel")
                            {
                                pm.hasLiquidFuelLine = true;
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < ((v.parts.Count) + 2); i++)
            {
                foreach (Part p in v.parts)
                {
                    if (p.Modules.Contains("ModuleWildfire") && p.fuelCrossFeed)
                    {
                        var pm = p.Modules.OfType<ModuleWildfire>().Single();
                        pm = p.FindModulesImplementing<ModuleWildfire>().First();
                        if (p.children.Count > 0)
                        {
                            foreach (Part cp in p.children)
                            {
                                var cpm = cp.Modules.OfType<ModuleWildfire>().Single();
                                cpm = cp.FindModulesImplementing<ModuleWildfire>().First();
                                if (cpm.hasMonoPropLine | cpm.hasPotentialMonopropLineChild > 0)
                                {
                                    pm.hasPotentialMonopropLineChild += 1;
                                }
                                if (cpm.hasOxidizerLine | cpm.hasPotentialOxidizerLineChild > 0)
                                {                                   
                                    pm.hasPotentialOxidizerLineChild += 1;
                                }
                                if (cpm.hasLiquidFuelLine | cpm.hasPotentialLiquidFuelLineChild > 0)
                                {
                                    pm.hasPotentialLiquidFuelLineChild += 1;
                                }
                            }
                        }
                        if (p.parent != null)
                        {
                            var pp = p.parent.Modules.OfType<ModuleWildfire>().Single();
                            pp = p.parent.FindModulesImplementing<ModuleWildfire>().First();
                            if (pp.hasMonoPropLine | pp.hasPotentialMonopropLineParent)
                            {
                                pm.hasPotentialMonopropLineParent= true;
                            }
                            if (pp.hasOxidizerLine | pp.hasPotentialOxidizerLineParent)
                            {
                                pm.hasPotentialOxidizerLineParent = true;
                            }
                            if (pp.hasLiquidFuelLine | pp.hasPotentialLiquidFuelLineParent)
                            {
                                pm.hasPotentialLiquidFuelLineParent = true;
                            }
                        }
                        if ((pm.hasPotentialOxidizerLineParent && pm.hasPotentialOxidizerLineChild > 0) | pm.hasPotentialOxidizerLineChild > 1)
                        {
                            pm.hasOxidizerLine = true;
                        }
                        if ((pm.hasPotentialMonopropLineParent && pm.hasPotentialMonopropLineChild > 0) | pm.hasPotentialMonopropLineChild > 1)
                        {
                            pm.hasMonoPropLine = true;
                        }
                        if ((pm.hasPotentialLiquidFuelLineParent && pm.hasPotentialLiquidFuelLineChild > 0) | pm.hasPotentialLiquidFuelLineChild > 1)
                        {
                            pm.hasLiquidFuelLine = true;
                        }
                    }
                }
            }
        }
    }
}
