using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;

namespace wildfire
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ModuleFuleLineBuilder : MonoBehaviour
    {
        private void onUndock(EventReport er)
        {
            Vessel v = FlightGlobals.ActiveVessel;
            checkFuelLines(v);
        }
        private void onVesselRollout(ShipConstruct sc)
        {
            Vessel v = FlightGlobals.ActiveVessel;
            checkFuelLines(v);
        }
        private void onVesselLoaded(Vessel v)
        {
            checkFuelLines(v);
        }

        public void Start()
        {
            checkFuelLines(FlightGlobals.ActiveVessel);
            GameEvents.onVesselWasModified.Add(checkFuelLines);
            GameEvents.OnVesselRollout.Add(onVesselRollout);
            GameEvents.onUndock.Add(onUndock);
            GameEvents.onVesselLoaded.Add(onVesselLoaded);
        }

        private void checkFuelLines(Vessel v)
        {
            foreach (Part p in v.parts)
            {
                if (p.Modules.Contains("ModuleWildfire"))
                {
                    if (p.Resources.Contains("MonoPropellant") || p.Modules.Contains("ModuleRCS") || p.Modules.Contains("ModuleDockingNode"))
                    {
                        var pm = p.Modules.OfType<ModuleWildfire>().Single();
                        pm = p.FindModulesImplementing<ModuleWildfire>().First();
                        pm.hasMonoPropLine = true;
                    }
                    /*
                    //Notworking
                    if ((p.Modules.OfType<ModuleEnginesFX>().Any(x => x.propellants.Any(y => y.name == "Oxidizer"))) || (p.Modules.OfType<ModuleEngines>().Any(x => x.propellants.Any(y => y.name == "Oxidizer"))) || p.Modules.Contains("ModuleDockingNode") || p.Resources.Contains("MonoPropellant"))
                    {
                        pm.hasOxidizerLine = true;
                    }
                    if ((p.Modules.OfType<ModuleEnginesFX>().Any(x => x.propellants.Any(y => y.name == "LiquidFuel"))) || (p.Modules.OfType<ModuleEngines>().Any(x => x.propellants.Any(y => y.name == "LiquidFuel"))) || p.Modules.Contains("ModuleDockingNode") || p.Resources.Contains("MonoPropellant"))
                    {

                        pm.hasLiquidFuelLine = true;
                    }
                    */
                }
            }
            foreach (Part p in v.parts)
            {
                if (p.Modules.Contains("ModuleWildfire"))
                {
                    var pm = p.Modules.OfType<ModuleWildfire>().Single();
                    pm = p.FindModulesImplementing<ModuleWildfire>().First();
                    if (p.children != null)
                    {
                        for (int i = 0; i < v.parts.Count; i++)
                        {
                            foreach (Part cp in p.children)
                            {
                                var cpm = cp.Modules.OfType<ModuleWildfire>().Single();
                                cpm = cp.FindModulesImplementing<ModuleWildfire>().First();
                                if (cpm.hasMonoPropLine)
                                {
                                    pm.hasMonoPropLine = true;
                                }
                                /*
                                if (cpm.hasOxidizerLine)
                                {
                                    pm.hasOxidizerLine = true;
                                }
                                if (cpm.hasLiquidFuelLine)
                                {
                                    pm.hasLiquidFuelLine = true;
                                }*/
                            }
                        }
                    }
                }
            }
        }
    }
}
