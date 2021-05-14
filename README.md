# Virtual_Pet_App_Development_Project

This project was developed for Android in Unity 2019.4.14f.

Yellow cuboids are placeholders for food bowls and are only able to be used once.
Red cuboids are beds, and the pigs are representative for all the toy Items.

- Use the blue speed buttons to control game time speed.
- Camera cannot be manipulated in the editor with the mouse as it's set up for touchscreen input.

- Select dog object (child of "GameManager/Dogs" in hierarchy) to view the nodes in its current A* path.
- Tap dog to view its current values via the UI. (Left mouse button click in the editor and a single finger tap on an Android device).

Current implemented states are:
- Idle (Wander around randomly while no afflictions are present or no Items for ailments are available).
- Pause (Do nothing).
- Hungry (Goes to the closest free food bowl to recover Hunger value).
- Tired (Goes to the closest free bed to sleep in and recover Rest value).
- Playful (Goes to the closest free toy and run with it to recover Happiness and Attention values).
