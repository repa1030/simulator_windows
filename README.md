# Unity Project of the HAD Simulator

**Go [here](https://github.com/repa1030/simulator_ubuntu) for the ROS environment of the simulator.**  
Current Version: Unity 2019.4.14  
Required Unity Version: 2019.4.x  

_Standalone Version is available in Build/_  

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
* Realistic Tree 9 - Pixel Games:  
<https://assetstore.unity.com/packages/3d/vegetation/trees/realistic-tree-9-rainbow-tree-54622>

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
* Requirements for the traffic simulation (optional):
    * SUMO Traffic Simulator (at least Version 1.5.0, make sure to add SUMO to PATH variable during installation):  
    <https://sumo.dlr.de/docs/Downloads.php>
    * Python on Windows (at least Python 3.6, make sure to add python to PATH variable):  
    <https://www.python.org/downloads/windows/>
    * Python Websockets:  
    `pip install websockets`
* Start for the first time (may take couple of minutes)

---
## Start of the Simulation in Editor

* Open the scene "MainMenu" that is located under Assets/Scenes
* Change the settings if required
* Start the simulation by pushing the start button

---
## Build the Simulator

* Go to "File" -> "Build Settings..."
* Make sure all scenes are checked
* Press "Build"
* Choose the directory where the final build should be located
* The following process might take a while