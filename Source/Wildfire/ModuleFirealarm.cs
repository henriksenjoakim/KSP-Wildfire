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
        public AudioSource generalAlarm;
        public AudioSource panicAlarm;
        public AudioSource clearAudio;
        GameObject soundObject = new GameObject();

        public int activeFires = 0;
        public int extinguisherCount = 0;
        public int commandModules = 0;
        public bool delay = false;
        public bool onFire = false;
        public bool commandModulePresent = false;

        public void checkForFires()
        {
            Vessel v = FlightGlobals.ActiveVessel;
            if (v != null && v.isEVA != true)
            {
                foreach (Part p in v.parts)
                {
                    if (p.Modules.Contains("ModuleCommand"))
                    {
                        commandModules += 1;
                    }
                    if (p.Modules.Contains("ModuleFireExtinguisher"))
                    {
                        extinguisherCount += 1;
                    }
                    var pp = p.Modules.OfType<ModuleWildfire>().Single();
                    pp = p.FindModulesImplementing<ModuleWildfire>().First();
                    if (pp.isOnFire == true)
                    {
                        activeFires += 1;
                    }
                }
            }
            else
            {
                commandModules = 0;
                extinguisherCount = 0;
                activeFires = 0;
            }
            if (activeFires > 0)
            {
                onFire = true;
            }
            else
            {
                onFire = false;
            }
            if (commandModules > 0)
            {
                commandModulePresent = true;
            }
            else
            {
                commandModulePresent = false;
            }
            activeFires = 0;
            commandModules = 0;
            extinguisherCount = 0;
        }

        public void clearAudioHandler()
        {
            if (!delay) return;
            else
            {
                if (!onFire)
                {
                    if (clearAudio != null)
                    {
                        if (!clearAudio.isPlaying && FlightGlobals.ActiveVessel.isEVA != true && commandModulePresent)
                        {                            
                            clearAudio.Play();
                        }                        
                    }
                    delay = false;
                }
            }
        }

        public void alarmHandler()
        {
            if (onFire)
            {
                delay = true;
                if (FlightGlobals.ActiveVessel.isEVA == false && commandModulePresent)
                {
                    onScreenMessages();
                    if (generalAlarm != null)
                    {
                        if (!generalAlarm.isPlaying)
                        {
                            generalAlarm.Play();
                        }
                    }
                    if (extinguisherCount == 0)
                    {
                        if (panicAlarm != null)
                        {
                            if (!panicAlarm.isPlaying)
                            {
                                panicAlarm.Play();
                            }
                        }
                    }
                }
                else
                {
                    if (generalAlarm != null)
                    {
                        if (generalAlarm.isPlaying)
                        {
                            generalAlarm.Stop();
                        }
                    }
                    if ( panicAlarm != null)
                    {
                        if (panicAlarm.isPlaying)
                        {
                            panicAlarm.Stop();
                        }
                    }
                }
            } 
            else
            {
                if (generalAlarm != null)
                {
                    if (generalAlarm.isPlaying)
                    {
                        generalAlarm.Stop();
                    }
                }
                if (generalAlarm != null)
                {
                    if (panicAlarm.isPlaying)
                    {
                        panicAlarm.Stop();
                    }
                }
            }
        }

        float timerCurrent = 0f;
        float timerTotal = 2f;

        private void tickHandler()
        {
            timerCurrent += Time.deltaTime;
            if (timerCurrent >= timerTotal)
            {
                timerCurrent -= timerTotal;
                checkForFires();
                alarmHandler();
                clearAudioHandler();
            }
        }

        public void Update()
        {
            tickHandler();
            if (generalAlarm.isPlaying || panicAlarm.isPlaying || clearAudio.isPlaying)
            {
                soundObject.transform.position = FlightGlobals.ActiveVessel.transform.position;
            }
            else
            {
                return;
            }
        }

        public void onScreenMessages()
        {
            if (onFire)
            {
                ScreenMessages.PostScreenMessage("Warning, ship is on fire!", 5.0f, ScreenMessageStyle.UPPER_CENTER); 
            }
        }

        public void onGamePause()
        {
            if (generalAlarm != null)
            {
                generalAlarm.volume = 0;
            }
            if (panicAlarm != null)
            {
                panicAlarm.volume = 0;
            }
            if (clearAudio != null)
            {
                clearAudio.volume = 0;
            }
        }

        public void onGameUnpause()
        {
            if (generalAlarm != null)
            {
                generalAlarm.volume = GameSettings.SHIP_VOLUME / 3;
            }
            if (panicAlarm != null)
            {
                panicAlarm.volume = GameSettings.SHIP_VOLUME / 3;
            }
            if (clearAudio != null)
            {
                clearAudio.volume = GameSettings.SHIP_VOLUME / 3;
            }
        }

        private void OnDestroy()
        {
            if (generalAlarm != null)
            {
                generalAlarm.Stop();
            }
            if (panicAlarm != null)
            {
                panicAlarm.Stop();
            }
            if (clearAudio != null)
            {
                clearAudio.Stop();
            }
            GameEvents.onGamePause.Remove(onGamePause);
            GameEvents.onGameUnpause.Remove(onGameUnpause);
        }

        public void Start()
        {      
            soundObject.transform.position = FlightGlobals.ActiveVessel.transform.position;

            generalAlarm = soundObject.AddComponent<AudioSource>();
            generalAlarm.volume = GameSettings.SHIP_VOLUME /3;
            generalAlarm.clip = GameDatabase.Instance.GetAudioClip("NANA/Wildfire/Sounds/WarningSound");
            generalAlarm.loop = true;
            generalAlarm.dopplerLevel = 0;
            generalAlarm.Stop();

            panicAlarm = soundObject.AddComponent<AudioSource>();
            panicAlarm.volume = GameSettings.SHIP_VOLUME /3;
            panicAlarm.clip = GameDatabase.Instance.GetAudioClip("NANA/Wildfire/Sounds/PanicSound");
            panicAlarm.loop = true;
            panicAlarm.dopplerLevel = 0;
            panicAlarm.Stop();

            clearAudio = soundObject.AddComponent<AudioSource>();
            clearAudio.volume = GameSettings.SHIP_VOLUME /3;
            clearAudio.clip = GameDatabase.Instance.GetAudioClip("NANA/Wildfire/Sounds/ClearSound");
            clearAudio.loop = false;
            clearAudio.dopplerLevel = 0;
            clearAudio.Stop();

            GameEvents.onGamePause.Add(onGamePause);
            GameEvents.onGameUnpause.Add(onGameUnpause);
        }
    }
}
