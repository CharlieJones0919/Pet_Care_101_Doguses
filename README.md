# Virtual_Pet_App_Development_Project

This project was developed for Android in Unity 2019.4.14f.

The example dog starts in the pause state.
Dog object are currently being represented by the red cubes with a smaller blue cubes indicating their fronts.
Black cuboids are placeholders for food bowls and are at this stage only able to be used once as refilling and food types haven't been implemented yet.
Small flat yellow cuboid is a placeholder for a dog bed.
Various other yellow primatives are only present to demontrate how the A* pathfinding makes paths around objects in the obstacle layer.

- Game time is a relative 72 seconds in-game to every real second. (1 day passes in-game every 20 minutes of real time).
- Camera cannot be manipulated in the editor with the mouse as it's set up for touchscreen input.

- Select dog object (child of "DogManager") to view the nodes in its current A* path.
- Dog's current FSM state can be seen in console output. 
- Tap dog to view its current values via the UI. (Left mouse button click in the editor and a single finger tap on an Android device).

Current implemented states are:
- Idle (Wander around randomly while no afflictions present).
- Pause (Do nothing for 5 seconds).
- Hungry (Goes to the closest free food bowl to recover Hunger value).
- Tired (Goes to the closest free free bed to sleep in and recover Rest value).