#Formula-Student-Simulator#
---
Make sure to maximise window when opening this for better formatting.
Note: Before working on this project, make sure that you are familar with atleast the basics of Unity, C#, Object Oriented Programming and GitHub.
It will also be helpful to read the project report as it explains how a lot of the things were implemented.
Once you are fimilar with the bsaics, use this tutorial series to understant how a raycast car works: https://www.youtube.com/watch?v=x0LUiE0dxP0&list=PLcbsEpz1iFyjjddSqLxnnGSJthfCcmsav 

There are similar tutrials online which can be useful too. 

Note: 
Some of the files in this project are too large for GitHub to track so they are ignored.
Make sure to read the "gitignore" files to find out more.
Also make sure that all team members start with the same version of the project when you enable github version control.

There is already a car in the project setup, you may want to add a new desgin in the future.
*How to import a new car:*
Export your CAD file into an FBX format.
Your hierarchy should have 5 items, 1 body and 4 wheels.
Don't export the entire assembly with pieces separated.
When you import the car into Unity, the axis should be aligned as follows:
X -> Right.
Y -> Up 
Z -> Forwards
All the scales should be at 1 and rotations at 0.
The script that controls the car needs all the scales to be at 1 and rotations to start at 0 or it will not work properly.
Once the car is imported and aligned poperly, you need to have the following structure:

* Car
    * body
    * wheels
        * FL Wheel
        * RL Wheel
        * FR Wheel
        * RR Wheel

Click on each wheel and create an empty game object as a child, set its co-ordinates and roataions to 0 and scales to 1.
Name these objects FL Spring, FR Spring etc.
Drag them upwards (no left or right movement, their centres need to be vertically aligned with the wheels.)
Create an empty game object called Springs.
Drag and drop FL Spring, FR Spriing etc. into the Springs gameobject.

* Do the following steps for FL wWheel and repeat for other wheels:
    * Right click on FL Wheel > Copy.
    * Right click on FL Wheel > Paste as child.
    * Rename the pasted object to FL Mesh. 
    * Remove the mesh filter and mesh renderer compoent of FL Wheel, found on the inespector.

Add an empty game object as a child of the car and call it COM Finder, move it to the centre of mass location of the car (by eye should be ok as a starting point).
Within Unity go to the "Assets" folder > "CarCamera" > Drag and drop the "CamParent" prefab into the car gameobject. 
Move the camera behind the car unitl you have clear view.
In the existing car, you may see a "Free Look Camera" and "CM Freelook 1".
These are a part of the Cinemachine package and allow for mouse movement to control the camera.
They are completely optional but if you enable "Free Look Camera", you must also enable "CM Freelook 1" or vice versa.
You may also see "First Person Camera", this is also optional.
Note that there should only be one camera active in the car and the rest should be disabled. 
Either:
* "CamParent"
* "Free Look Camera" and "CM Freelook 1"
* "First Person Camera"

Your hierarchy for the car should look like this:

* Car
    * Body
    * Wheels
        * FL Wheel
            * FL Mesh
        * RL Wheel
            * FL Mesh
        * FR Wheel
            * FR Mesh
        * RR Wheel
            * RR Mesh
    * Springs
        * FL Spring
        * FR Spring
        * RL Spring
        * RR Spring
    * CamParent

Go to the inspector and add a rigid body component and a box collider to "Car".
Set the rigidbody mass, and adjust to box collider so that it covers the car, and partially the wheels. 
Add the same scripts and components as the car that is alreay included in the project.
Adjust all the values in the "Raycast Controller" section of the inspector. 
Drag and drop FL Wheel, FR Wheel etc into the appropriate fields in the "Raycast Controller" component. 
Drag and drop FL Spring, FR Spring etc into the appropriate fields in the "Raycast Controller" component.
Drag and drop FL Mesh, FR Mesh etc into the appropriate fields in the "Raycast Controller" component. 
Drive the car.

You can go on the "Scenes" folder to change between North Weald and Silverston tracks and edit them separately.
Note that each scene has its own copy of the car, each car will have its own values like mass, gear ratios etc. However, changing the code in the script will apply to all cars. This is because they use the same script but count as different game objects.
The same applies to any other game objects that use a script.
To create a new track environment in a different scene, just creat a new scene and copy-paste everything from the current scene.
Then you can just remove the existing track and create a new one, whithout having to setup things like the Telemetry, Main menu etc again.
To setup the checkpoints again, delete everything that is a child of the "Checkpoints" object, then add the "StartLine" prefab as a child. 
Then add a "Checkpoint" prefab as a child of the "Checkpoints" empty game object, and move it to the first checkpoint's position.
Copy-paste the newly added prefab and move it to the second checkpoint's position.
Repeat for the remaining checkpoints. 
Make sure each gameobject/prefab has its relevant scripts attcehd, and that the scripts's variable have their items or values assigned.
E.g, The UI needs a "LaptimeUIManager" script with laptime text and lap number text dragged and dropepd into the inspector. 
If you see any errors relating to an item not being found, double check the values in the inespector.
The error message should tell you some information about what is missing. 






