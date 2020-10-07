# Unity Project of the HAD Simulator

Current Version: Unity 2019.3.7
Required Unity Version: 2019.3 or 2019.4

---
## Required Assets

* Wheel Controller 3D - NWH Coding (for the wheel colliders, see Assets/WheelController):  
<https://assetstore.unity.com/packages/tools/physics/wheel-controller-3d-74512>
* Highway Construction Set from  (for the environment, see Assets/HighwayContructionSet):  
<https://assetstore.unity.com/packages/3d/environments/urban/highway-construction-set-4246>

---
## Included Assets

* ROS# - Siemens:  
<https://github.com/siemens/ros-sharp/tree/master/Unity3D/Assets>
* Low-poly Civilian vehicle #5 - Pro 3D models:  
<https://assetstore.unity.com/packages/3d/vehicles/land/low-poly-civilian-vehicle-5-124987>
* ProgressBar Pack - upln:  
<https://assetstore.unity.com/packages/tools/gui/progressbar-pack-120981>
* Wispy Skybox - Mundus:  
<https://assetstore.unity.com/packages/2d/textures-materials/sky/wispy-skybox-21737>

---
## Setting up the Project

* Make sure you have installed Git on your PC:  
<https://gitforwindows.org/>
* Make sure you have Git-LFS installed:  
<https://git-lfs.github.com/>
* Open a command window and type  
`git lfs install`
* Clone the repository into your workspace  
`git clone https://github.com/repa1030/simulator_windows.git`
* Install Unity Hub:  
<https://unity3d.com/de/get-unity/download/>
* Install the correct Unity version:  
<https://unity3d.com/de/get-unity/download/archive/>
* Inside the Unity Hub press "Add" and choose the "simulator_windows" directory
* Start for the first time (may take couple of minutes)

---
## Start of the Simulation in Editor

* Open the scene "MainMenu" that is located under Assets/Scenes
* Start the simulation by pushing the play button
* The following two points have to be done only the first time:  
    * Push the options button and check the parameters
    * Push the save button
* Choose the desired scene

---
## Build the Simulator

* Go to "File" -> "Build Settings..."
* Make sure all scenes are checked
* Press "Build"
* Choose the directory where the final build should be located
* The following process may take a while