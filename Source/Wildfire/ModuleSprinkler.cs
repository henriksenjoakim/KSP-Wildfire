using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace wildfire
{
    class ModuleSprinkler :PartModule
    {
        private GameObject sprinklerFx;
        private GameObject sparkFx;

        private AudioSource sprinklerAudio;
        private AudioSource bleepAudio;
        private AudioSource bleeeepAudio;
        private AudioSource sparklerAudio;
        private AudioSource spoolupAudio;

        //[KSPField(guiActive = true, guiActiveEditor = false, isPersistant = false, guiName = "Launch is safe:")]
        public bool isSafe = false;

        public float countDownTimer = 15;
        public bool countDownInitiated = false;
        private float timerCurrent = 0f;
        private float timerTotal = 1f;
        public bool bleeeepHasPlayed = false;

        public void setupFX()
        {
            sprinklerFx = (GameObject)GameObject.Instantiate(UnityEngine.Resources.Load("Effects/fx_exhaustFlame_white_tiny"));
            sprinklerFx.transform.position = this.part.transform.position;
            sprinklerFx.particleEmitter.localVelocity = this.part.transform.up * 10;
            sprinklerFx.particleEmitter.useWorldSpace = true;
            sprinklerFx.particleEmitter.maxEnergy = 1;
            sprinklerFx.particleEmitter.maxEmission = 20;
            sprinklerFx.particleEmitter.minEnergy = 1;
            sprinklerFx.particleEmitter.minEmission = 10;
            sprinklerFx.particleEmitter.maxSize = 6;
            sprinklerFx.particleEmitter.minSize = 3;
            //sprinklerFx.particleEmitter.angularVelocity = 10;
            sprinklerFx.particleEmitter.emit = false;

            sparkFx = (GameObject)GameObject.Instantiate(UnityEngine.Resources.Load("Effects/fx_exhaustSparks_yellow"));
            sparkFx.transform.position = this.part.transform.position;
            sparkFx.particleEmitter.localVelocity = this.part.transform.up * 30;
            sparkFx.particleEmitter.useWorldSpace = true;          
            //sparkFx.particleEmitter.minEnergy = 1f;
            sparkFx.particleEmitter.minEmission = 350;
            //sparkFx.particleEmitter.maxEnergy = 1f;
            sparkFx.particleEmitter.maxEmission = 400;
            //sparkFx.particleEmitter.angularVelocity = 10;
            sparkFx.particleEmitter.rndAngularVelocity = 0;
            sparkFx.particleEmitter.rndRotation = false;
            sparkFx.particleEmitter.emit = false;
        }

        public void setupAudio()
        {
            bleepAudio = gameObject.AddComponent<AudioSource>();
            bleepAudio.volume = GameSettings.SHIP_VOLUME;
            bleepAudio.clip = GameDatabase.Instance.GetAudioClip("NANA/Wildfire/Sounds/BleepSound");
            bleepAudio.loop = false;
            bleepAudio.dopplerLevel = 0;
            bleepAudio.Stop();

            bleeeepAudio = gameObject.AddComponent<AudioSource>();
            bleeeepAudio.volume = GameSettings.SHIP_VOLUME;
            bleeeepAudio.clip = GameDatabase.Instance.GetAudioClip("NANA/Wildfire/Sounds/BleeeepSound");
            bleeeepAudio.loop = false;
            bleeeepAudio.dopplerLevel = 0;
            bleeeepAudio.Stop();

            sprinklerAudio = gameObject.AddComponent<AudioSource>();
            sprinklerAudio.volume = GameSettings.SHIP_VOLUME / 3;
            sprinklerAudio.clip = GameDatabase.Instance.GetAudioClip("NANA/Wildfire/Sounds/WaterSound");
            sprinklerAudio.loop = true;
            sprinklerAudio.Stop();

            sparklerAudio = gameObject.AddComponent<AudioSource>();
            sparklerAudio.volume = GameSettings.SHIP_VOLUME / 3;
            sparklerAudio.clip = GameDatabase.Instance.GetAudioClip("NANA/Wildfire/Sounds/SparklerSound");
            sparklerAudio.loop = true;
            sparklerAudio.Stop();

            spoolupAudio = gameObject.AddComponent<AudioSource>();
            spoolupAudio.volume = GameSettings.SHIP_VOLUME / 3;
            spoolupAudio.clip = GameDatabase.Instance.GetAudioClip("NANA/Wildfire/Sounds/SpoolupSound");
            spoolupAudio.loop = false;
            spoolupAudio.Stop();
        }

        public void countDown()
        {
            if (countDownInitiated == true)
            {
                timerCurrent += Time.deltaTime;
                if (timerCurrent >= timerTotal)
                {
                    timerCurrent -= timerTotal;
                    countDownTimer -= 1;
                    if (countDownTimer != 10)
                    {
                        if (bleepAudio != null)
                        {
                            bleepAudio.Play();
                        }
                    }
                }
            }
        }

        public void launchTime()
        {
            if (countDownTimer <= 12)
            {
                isSafe = true;
                sparkFx.particleEmitter.emit = true;
                if (sparklerAudio != null)
                {                    
                    if (!sparklerAudio.isPlaying)
                    {
                        sparklerAudio.Play();
                    }
                }
            }

            if (countDownTimer == 10)
            {
                if (bleeeepAudio != null && bleeeepHasPlayed == false)
                {
                    if (!bleeeepAudio.isPlaying)
                    {
                        bleeeepAudio.Play();
                        bleeeepHasPlayed = true;
                    }
                    ScreenMessages.PostScreenMessage("Launch!", 5.0f, ScreenMessageStyle.UPPER_CENTER);
                }
            }
        }

        public override void OnFixedUpdate()
        {
            if (countDownTimer > 0)
            {
                countDown();
                launchTime();
                sprinklerFx.transform.position = this.part.transform.position;
                sparkFx.transform.position = this.part.transform.position;
            }
            else
            {
                countDownInitiated = false;
                isSafe = false;
                if (sprinklerAudio != null)
                {
                    if (sprinklerAudio.isPlaying)
                    {
                        sprinklerAudio.Stop();
                    }
                }
                if (sparklerAudio != null)
                {
                    if (sparklerAudio.isPlaying)
                    {
                        sparklerAudio.Stop();
                    }
                }
                bleeeepHasPlayed = false;
                sprinklerFx.particleEmitter.emit = false;
                sparkFx.particleEmitter.emit = false;
                countDownTimer = 15;
            }

        }

        public override void OnActive()
        {
            countDownInitiated = true;
            sprinklerFx.particleEmitter.emit = true;
            if (sprinklerAudio != null)
            {
                if (!sprinklerAudio.isPlaying)
                {
                    sprinklerAudio.Play();
                }
                if (spoolupAudio != null)
                {
                    spoolupAudio.Play();
                }
            }
            base.OnActive();
        }


        public void OnDestroy()
        {
            if (sprinklerAudio != null)
            {
                sprinklerAudio.Stop();
            }
            if (bleepAudio != null)
            {
                bleepAudio.Stop();
            }
            if (bleeeepAudio != null)
            {
                bleeeepAudio.Stop();
            }
            if (spoolupAudio != null)
            {
                spoolupAudio.Stop();
            }
            if (sparklerAudio != null)
            {
                sparklerAudio.Stop();
            }

            GameEvents.onGamePause.Remove(onGamePause);
            GameEvents.onGameUnpause.Remove(onGameUnpause);
        }

        public void onGamePause()
        {
            if (sprinklerAudio != null)
            {
                sprinklerAudio.volume = 0;
            }
            if (sparklerAudio != null)
            {
                sparklerAudio.volume = 0;
            }
        }

        public void onGameUnpause()
        {
            if (sprinklerAudio != null)
            {
                sprinklerAudio.volume = GameSettings.SHIP_VOLUME / 3;
            }
            if (sparklerAudio != null)
            {
                sparklerAudio.volume = GameSettings.SHIP_VOLUME / 3;
            }

        }   

        public override void OnStart(PartModule.StartState state)
        {
            this.part.stackIcon.CreateIcon();
            this.part.stagingIcon = "PARACHUTES";
            this.part.stagingOn = true;
            if (state == StartState.Editor || state == StartState.None) return;
            GameEvents.onGamePause.Add(onGamePause);
            GameEvents.onGameUnpause.Add(onGameUnpause);
            setupFX();
            setupAudio();
            base.OnStart(state);
        }
    }
}
