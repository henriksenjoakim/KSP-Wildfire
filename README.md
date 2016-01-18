# KSP-Wildfire

Warning: Work in progress!

This adds a pseudo fire simulation add-on to the videogame Kerbal Space Program.

Overheating, bumping into things, crashing, pulling too many Gees(WIP) and other reckless activities may cause fire to break out in the subjected part. A fire will cause the part to gradually overheat, giving only a few seconds until it disintegrates. There is also a risk that the fire may spread to adjacent parts, leading to an (unfortunate) chain reaction.

No more "Oh, I didn't really need that part anyway..." You really need the Primary Buffer panel to stay attached for the whole re-entry sequence! It may cause a leak, or a short-circuit, or general spontaneous combustion.

This add-on attempts to introduce less randomness in how failure (fires) occurs, thus a fire will never occur at total random, but can only be caused by something. This encourages taking proactive steps in order to reduce the chance of critical failure, as well as encouraging safe flying (who am I kidding?). 

As commander of your sheep, you may want to herd your crew to the pre-installed escape pods when you find the ship on fire and burning from enemy attacks. A LES is also recommended for launch. If you play with a life-support add-on, you may also want to pack potatoes and your stillsuit, in case your craft goes down on Duna...

Indecently, the add-on also comes included with a fire extinguisher system with built in extinguisher fluid (currently liquid CO2, until I can find something better), but it cost both extra weight and power (look for an RCS tank clone under Utilities). Once installed and activated, it will automatically try to extinguish fires that may occur on the craft. You can have more than one installed.


Package contents:
1x Automatic fire extinguisher part (Monopropellant tank clone under Utilities). When activated it will automatically try to douse fires at the cost of liquid CO2 and electric charge.
1x Automatic fire alarm (Preinstalled in every cockpit).
1x Right-click risk info menu.
1x Various fire hazards.


More detail:
-When temperature of a part reaches 70%+ a fire may break out.
-When a part is on fire, a fire may break out in its connected parts.
-When a part collides with another object with a velocity of 90% of its rated crash tolerance a fire may break out.
-When a part explodes, connected parts may catch fire.

Whether or not a part will catch fire at either event mentioned above is dependent on type of part and its contents. At its current state it takes into consider:
- Vacuum and atmosphere (on Kerbin for now).
- Resources (fuel etc.). Liquid fuel, monopropellant, electric charge and solid fuel.
- Part type: Engines and crew compartment parts.
- Connected fuel containers.
- Explosions, crash velocity etc.
The mod also tries to calculate where fuel lines between parts are connected (Currently it only works for RCS-thrusters and parts containing monopropellant).

Consequently, the mod also considers situations when a fire is doused:
- Submerging a part in water may douse a fire.
- In vacuum there is a high chance a fire may douse as long as the part does not contain any oxidizer/crew compartment).
- Not carrying flammable materials or oxidizers will increase the chance of fire going out.
- The automatic fire extinguisher is active.
- There is a tiny chance a fire may douse itself.

- Parts with decouplers are immune to connected explosions.
- Parts with heatshield does not catch fire.
- Fuel lines and struts are excluded.


Required add-ons:
- ModuleManager (http://forum.kerbalspaceprogram.com/index.php?/topic/50533-105-module-manager-2617-january-10th-with-even-more-sha-and-less-bug/) (Adds mod functionality to all parts in game)
- Community Resource Pack (https://kerbalstuff.com/mod/974/Community%20Resource%20Pack) (Uses Liquid CO2.)


Considered features (may or may not come):
- Gameplay balancing (More realism?).
- Integration with other hazardous mods (need to ask for permissions first).
- Pseudo weather hazards (when I can figure out how to make a toolbar interface).
- Atmospheric and deep space hazards.
- Crew skill based fire risk reduction (need to find a gameplay-friendly way to calculate the reduction).
- Ground Control skill based fire risk reduction (encourage hiring random staff).
- Fuel sparklers and water-based sound suppression (timing based mini-game) for reducing fire risk on launch (for encouraging a proper     countdowns).
- Weight increase for every part when fire extinguisher system is installed.
- More fuel line integration (if I can figure out why it doesn't work).
- More fuel type hazards.
- Douse fires by opening airlocks (For use with Life Support Mods).
- Bubbles.
- Decupler risk, fuel cell risk, RTG risk.
- Dynamic extinguishing.

This work is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.
For more details, check out http://creativecommons.org/licenses/by-nc-sa/4.0/
