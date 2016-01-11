using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace wildfire
{
    public class ModuleFireExtinguisher : PartModule
    {
        [KSPField(isPersistant = true)]
        public bool autoActivated = false;

        public bool sprinklerActivated = false;

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = true, externalToEVAOnly = false, guiActiveUnfocused = false, guiName = "AutoExtinguish On")]
        public void autoActivate()
        {
            if (!autoActivated)
            {
                autoActivated = true;
                ToggleEvent("autoActivate", false);
                ToggleEvent("autoDeactivate", true);
            }
        }

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = true, externalToEVAOnly = false, guiActiveUnfocused = false, guiName = "AutoExtinguish Off")]
        public void autoDeactivate()
        {
            if (autoActivated)
            {
                autoActivated = false;
                ToggleEvent("autoDeactivate", false);
                ToggleEvent("autoActivate", true);
            }
        }

        private void ToggleEvent(string eventName, bool state)
        {
            Events[eventName].active = state;
            Events[eventName].externalToEVAOnly = state;
            Events[eventName].guiActive = state;
        }

        public override void OnAwake()
        {

            if (!autoActivated)
            {
                ToggleEvent("autoDeactivate", false);
                ToggleEvent("autoActivate", true);
            }
            else
            {
                ToggleEvent("autoActivate", false);
                ToggleEvent("autoDeactivate", true);
            }
            base.OnAwake();
        }

    }
}
