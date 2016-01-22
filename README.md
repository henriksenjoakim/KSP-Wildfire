# KSP-Wildfire

Warning: Work in progress!


 This mod adds pseudo fire hazards to the game. Originally created for another mod, but turned standalone.

Overheating, bumping into things, crashing and other reckless activities may cause fire to break out in the subjected part. A fire will cause the part to gradually overheat, giving only a few seconds until it disintegrates. There is also a risk that a fire may spread to adjacent parts, leading to an (unfortunate) chain reaction.

No more "Oh, I didn't really need that part anyway..." You really need the Primary Buffer panel to stay attached for the whole re-entry sequence! It may cause a leak, or a short-circuit, or general spontaneous combustion.

This add-on attempts to introduce less randomness in how failure (fires) occurs, thus a fire will never occur at total random, but can only be caused by something. This will hopefully encourage players to take proactive steps (WIP) in order to reduce the chance of critical failure, as well as encouraging safe flying (who am I kidding?).

As commander of your sheep, you may want to herd your crew to the pre-installed escape pods when you find the ship on fire and burning from enemy attacks. A LES is also recommended for launch. If you play with a life-support add-on, you may also want to pack potatoes and your stillsuit, in case your craft goes down on Duna...

Incidentally, the add-on also comes included with a fire extinguisher system. 

Package contents:
- 1x Automatic fire extinguisher part (Monopropellant tank clone under Utilities). When activated it will automatically try to douse fires at the cost of liquid CO2 and electric charge.
- 1x Automatic fire alarm (Pre-installed in every cockpit).
- 1x Right-click risk info menu. Indicates the highest possible fire risk on the selected part.
- Various fire hazards.
- 1x Water sprinkler with built in sparkler (2 in 1! For performance!). It's a stage-able part in the Utility section (linear RCS clone) when activated, it will start up an give you 5 beeps, and a long beep at the end indicating it's ready. When ready, it will give you a few seconds of halved risk factor for every part! Note: It will only work when landed, and you have to launch before it stops to reap the benefits (NEW in 0.1.3).




More details:

When a part is on fire it will gradually overheat based on its contents. Connected fuel and fuel in the part will drain to feed the fire. Electric charge is also drain by a small amount because of short-circuiting. All parts may catch fire, as well as parts that doesn't "realistically" catch fire are also subjected. However the chance is significantly reduced compared to other parts, and are considered "overheating" as all parts can carry Electric Charge (sparks and smoke only).

[Igniting events] A part may catch fire when:

- ...the temperature of a part reaches 70+% it may catch fire.
- ...a part is on fire connected parts may catch fire.
- ...a part collides with another object with a velocity of 90+% of its rated crash tolerance it may catch fire.
- ...a part explodes, connected parts may catch fire.
- ...a part takes exhaust from an engine it may catch fire.
- ...a part flexes too much from it's connected part, it may catch fire. I.e. Wobbliness, bending (NEW in 0.1.3).

Whether or not a part will catch fire at either event mentioned above is dependent on type of part and its contents. At its current state it takes into consider [Risk factors]:

- Vacuum and atmosphere (on Kerbin for now).
- Resources (fuel etc.). Liquid fuel, monopropellant, electric charge and solid fuel.
- Part type: Engines and crew compartment parts.
- Connected fuel containers.
- Explosions, crash velocity etc.
- The mod also tries to calculate where fuel lines between parts are connected. Fuel lines have an increased risk factor when fuel is on board.
- G-forces.
- Dynamic pressure.

Whenever an igniting events occur on a part, a random number is generated between 0 and 100. If the number is bellow the risk factor of the subjected part a fire will occur.

Consequently, the mod also considers situations when a fire is doused:

- When submerging a part in water may douse a fire.
- When in vacuum there is a high chance a fire may douse as long as the part does not contain any oxidising materials.
- When not carrying flammable materials or oxidisers will increase the chance of fire going out.
- When the automatic fire extinguisher is active.
- There is a tiny chance a fire may douse itself.

Parts with decouplers are immune to connected explosions. Parts with heat-shield does not catch fire. Fuel lines and struts are excluded.

Whenever a part is subjected to excess bending from it's parent part you may hear a "creaking"-sound. If the part bends above the set bending limit (WIP) a blue highlight will indicate that the part is under stress, and you will hear a louder creaking-sound. At this point the part may catch fire according to the risk factor for the specific part.


Required add-ons:
- ModuleManager (http://forum.kerbalspaceprogram.com/index.php?/topic/50533-105-module-manager-2617-january-10th-with-even-more-sha-and-less-bug/) (Adds mod functionality to all parts in game, can be edited in the config-file if you want to exclude parts.)
- Community Resource Pack (https://kerbalstuff.com/mod/974/Community%20Resource%20Pack) (Uses Liquid CO2.)


Considered features (may or may not come):
- Gameplay balancing (More realism?).
- Integration with other hazardous mods (need to ask for permissions first).
- Pseudo weather hazards (when I can figure out how to make a toolbar interface).
- Atmospheric and deep space hazards.
- Crew skill based fire risk reduction (need to find a gameplay-friendly way to calculate the reduction).
- Ground Control skill based fire risk reduction (encourage hiring random staff).
- Douse fires by opening airlocks (For use with Life Support Mods).
- Bubbles.
- Dynamic extinguishing.
- Dynamic variables


This work is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.
For more details, check out http://creativecommons.org/licenses/by-nc-sa/4.0/
