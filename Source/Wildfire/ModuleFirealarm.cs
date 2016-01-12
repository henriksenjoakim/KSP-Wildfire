using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace wildfire
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ModuleFirealarm : MonoBehaviour
    {
        AudioSource audioSource;
        GameObject soundObject;
        float activeFires = 0;

        public void Update()
        {
            Vessel v = FlightGlobals.ActiveVessel;
            float totalParts = v.parts.Count;
            float partCounter = 0;

            if (totalParts > 0 && totalParts != null)
            {
                foreach (Part p in v.parts)
                {
                    partCounter += 1;
                    var pp = p.Modules.OfType<ModuleWildfire>().Single();
                    pp = p.FindModulesImplementing<ModuleWildfire>().First();
                    if (pp.isOnFire == true)
                    {
                        activeFires += 1;
                    }
                    if (partCounter == totalParts)
                    {
                        soundHandler();
                        activeFires = 0;
                    }
                }
            }
            else
            {
                activeFires = 0;
                soundHandler();
            }
        }

        public void soundHandler()
        {
            if (activeFires > 0 && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
            else if (activeFires > 0 && audioSource.isPlaying)
            {
                //donothing
            }
            else
            {
                audioSource.Stop();
            }
        }

        public void Start()
        {
            soundObject = new GameObject();
            soundObject.transform.position = FlightGlobals.ActiveVessel.transform.position;
            audioSource = soundObject.AddComponent<AudioSource>();
            audioSource.volume = GameSettings.SHIP_VOLUME;
            audioSource.clip = GameDatabase.Instance.GetAudioClip("NANA/Wildfire/Sounds/WarningSound");
            audioSource.loop = true;
            audioSource.dopplerLevel = 0;
            audioSource.Stop();
        }
    }
}
