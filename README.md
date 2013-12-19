TACTICAL TIME-BOUNDED PATHFINDING

INDEX:

1.	Introduction
2.	CreateNavMesh.cs
3.	AStarOnly.cs
4.	SoldierStates.cs
5.	EnemyManager.cs
6.	Statistics.cs
7.	HumanFieldOfView
8.	FireAI
9.	MyBulletScript
10.

1.	INTRODUCTION
This folder contains the source code for my Master's thesis. The general idea is that two AI teams will battle one another using different pathfinding criteria:
one team focusses entirely on path length, while the other tries to balance this with self-preservation. Below I summarise the classes used.

2.	CREATENAVMESH.CS
This is the script I used to create the navigation mesh. It creates a grid in the Unity Editor, with nodes placed at user-defined intervals 
(by default, this is every metre).

3.	ASTARONLY.CS
This is a test class that implements regular A* pathfinding for a unit. This is kept separate from the CreateNavMesh script so that the pathfinding
is independent of the navmesh generation.

4.	SOLDIERSTATES.CS
This is the Finite-State Machine that my units use, so-called because of the character models I use in the testbed. It is exactly the same for each team.

5.	ENEMYMANAGER.CS
This acts as a base for each team, coordinating their paths and respawning them when they die.

6.	STATISTICS.CS
This class gathers statistics for each test. In the test-bed, I attach it to an orthographic camera that watches the scene.

7.	HUMANFIELDOFVIEW.CS
This class is attached to an invisible cone that is attached to a unit's head. If an object enters the cone, a ray is cast towards the object to determine line-of-sight.
If the unit has a line of sight towards the object, and the object is on the other team, a message is sent to the unit to start firing.

8. FIREAI.CS
This class controls an NPC's weapon. It includes methods for reloading and firing.

9. MYBULLETSCRIPT.CS
This class needs a slightly more professional name. It handles the bullet that is fired by a gun.
