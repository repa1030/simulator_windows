# =================================================================
# Copyright (C) 2020 Hochschule Karlsruhe - Technik und Wirtschaft
# This program and the accompanying materials
# are made available under the terms of the MIT license.
# =================================================================
# Authors: Anna-Lena Marmein, Magdalena Kugler, Yannick Rauch,
#          Patrick Rebling
# =================================================================

import traci
import time
import os
import sys
import SumoVehicle

class TrafficSimulator(object):
    def __init__(self, path):

        self.path = path
        self.StartSumo()

    def StartSumo(self):
        if 'SUMO_HOME' in os.environ:
            tools = os.path.join(os.environ['SUMO_HOME'], 'tools')
            sys.path.append(tools)
            sumoBinary = os.path.join(os.environ['SUMO_HOME'], 'bin/sumo-gui.exe')
        else:
            sys.exit("please declare environment variable 'SUMO_HOME'")
        sumoCmd = [sumoBinary, "-c",
                   self.path, "--start", "--collision.check-junctions", "true"]
        traci.start(sumoCmd)

        print("Sumo is running")

    def RestartSumo(self):

        #Restart the program
        try:
            traci.close()
            self.StartSumo()
        except:
            print("Restart was not possible. Stop.")
            sys.exit()
        return False, False, []

    def StepSumo(self, SumoObjects, egoVehicle):

        try:
            traci.simulationStep()  # step simulatior
        except:
            print("Restarting SUMO")
            egoVehicle["isSet"], egoVehicle["isReceived"], SumoObjects = self.RestartSumo()
            return SumoObjects, egoVehicle

        SumoObjectsRaw0 = traci.vehicle.getIDList()  # get every vehicle ID
        SumoObjectNames = list(set(SumoObjectsRaw0))  # Make it unique

        # Remove SUMO objects from the list if they left the network
        for Obj in SumoObjects:
            if(not(any(ObjName == Obj.ID for ObjName in SumoObjectNames))):
                SumoObjects.remove(Obj)

        # Append new objects and update existing ones.
        for VehID in SumoObjectNames:
            if(not(any(Obj.ID == VehID for Obj in SumoObjects)) and VehID != "ego_vehicle"):
                NewlyArrived = SumoVehicle.SumoObject(VehID)
                SumoObjects.append(NewlyArrived)

        # Update Sumo vehicle objects
        for Obj in SumoObjects:
            Obj.UpdateVehicle()

        # Update Ego Vehicle
        if not egoVehicle["isSet"] and egoVehicle["isReceived"]:
            traci.vehicle.add(egoVehicle["id"], egoVehicle["route"], egoVehicle["vehicleType"])
            traci.vehicle.setLaneChangeMode(egoVehicle["id"], 512) # bit mask for no lane changing
            egoVehicle["isSet"] = True
        elif egoVehicle["isSet"]:
            traci.vehicle.moveToXY(egoVehicle["id"], egoVehicle["edge"], egoVehicle["laneIndex"], egoVehicle["posX"], egoVehicle["posY"], egoVehicle["heading"], egoVehicle["keepRouteMode"])
            traci.vehicle.setSpeed(egoVehicle["id"], egoVehicle["velocity"])
            # Format: Bitmask  Bit 0: SignalRight; Bit 1: SignalLeft; Bit 2: SignalEmergency; Bit 3: BrakeLights; Bit 4: FrontLights; ... Bit 13: SignalsEmergencyYellow
            # see https://sumo.dlr.de/docs/TraCI/Vehicle_Signalling.html
            # set brake light bit
            if egoVehicle["brakeState"] == 1:
                traci.vehicle.setSignals(egoVehicle["id"], traci.vehicle.getSignals(egoVehicle["id"]) | 0b00000000001000)
            else:
                traci.vehicle.setSignals(egoVehicle["id"], traci.vehicle.getSignals(egoVehicle["id"]) & 0b11111111110111)
            # set backwards driving light bit
            if egoVehicle["drivingBackwards"] == 1:
                traci.vehicle.setSignals(egoVehicle["id"], traci.vehicle.getSignals(egoVehicle["id"]) | 0b00000010000000)
            else:
                traci.vehicle.setSignals(egoVehicle["id"], traci.vehicle.getSignals(egoVehicle["id"]) & 0b11111101111111)
            # set signal lights (currently not working in visualization)
            if egoVehicle["signalLights"] == "Off":
                traci.vehicle.setSignals(egoVehicle["id"], traci.vehicle.getSignals(egoVehicle["id"]) & 0b00011111111111)
            elif egoVehicle["signalLights"] == "Right":
                traci.vehicle.setSignals(egoVehicle["id"], traci.vehicle.getSignals(egoVehicle["id"]) & 0b00011111111111)
                traci.vehicle.setSignals(egoVehicle["id"], traci.vehicle.getSignals(egoVehicle["id"]) | 0b10000000000000)
            elif egoVehicle["signalLights"] == "Left":
                traci.vehicle.setSignals(egoVehicle["id"], traci.vehicle.getSignals(egoVehicle["id"]) & 0b00011111111111)
                traci.vehicle.setSignals(egoVehicle["id"], traci.vehicle.getSignals(egoVehicle["id"]) | 0b01000000000000)
            elif egoVehicle["signalLights"] == "All":
                traci.vehicle.setSignals(egoVehicle["id"], traci.vehicle.getSignals(egoVehicle["id"]) & 0b00011111111111)
                traci.vehicle.setSignals(egoVehicle["id"], traci.vehicle.getSignals(egoVehicle["id"]) | 0b00100000000000)
        return SumoObjects, egoVehicle