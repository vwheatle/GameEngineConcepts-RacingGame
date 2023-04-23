# [Racing Game](https://github.com/vwheatle/GameEngineConcepts-RacingGame)

for my Game Engine Concepts class..

Reuses much of the code from my previous project, the [Third-Person Platformer](https://github.com/vwheatle/GameEngineConcepts-ThirdPersonPlatformer)

## Resources Used

- https://youtu.be/PEHtceu7FBw - CharacterController stick-to-surface script to prevent losing contact with the slope when moving down it. Adapted for... to do more things.
- I think I was mentally referencing the Yo! Noid 2 capsule Noid character when I was implementing the procedural animations? Definitely had it in mind.
- I'm stealing a good amount of code from my last game, [Speedrun FPS](https://github.com/vwheatle/IntroToGameDev-SpeedrunFPS/).

- Some sounds generated with https://raylibtech.itch.io/rfxgen
- Coin Finalize sound from https://freesound.org/people/PhilSavlem/sounds/338260/
- Dying sound from https://freesound.org/people/craigsmith/sounds/675810/

## Current Issues

### holdovers from the 3rd-person platformer

- jumping up the REALLY BIG slope below the example level exhausts all your jumps while you're still grounded on the slope
- your soul may still be attached to the oscillating platform even after you are thrown off of it
	- i really just need to trust the sphere cast i do every frame with more of the "grounded" variable.
- you don't rotate to face the direction you're moving in ("if i want to look less like a capsule, this is a good starting point" - player prefab)
	- this necessitates another nested empty pivot object!!
