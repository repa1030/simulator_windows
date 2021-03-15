# =================================================================
# Copyright (C) 2020 Hochschule Karlsruhe - Technik und Wirtschaft
# This program and the accompanying materials
# are made available under the terms of the MIT license.
# =================================================================
# Authors: Anna-Lena Marmein, Magdalena Kugler, Yannick Rauch,
#          Patrick Rebling
# =================================================================

import math
import traci

class SumoObject(object):

    def __init__(self, SumoID):
        self.ID = str(SumoID)  # VehicleID
        try:

            self.ObjType = traci.vehicle.getTypeID(self.ID)
            self.Route = traci.vehicle.getRouteID(self.ID)
            self.Edge = traci.vehicle.getRoadID(self.ID)
            self.Length = traci.vehicle.getLength(self.ID)
            self.Width = traci.vehicle.getWidth(self.ID)
            if traci.vehicle.getSignals(self.ID) & 8 == 8: #Bitmask - 8 for brake light
                self.StBrakePedal = True
            else:
                self.StBrakePedal = False
            # Blinker State 0->none, 1->right, 2->left
            if traci.vehicle.getSignals(self.ID) & 1 == 1:
                self.StBlinker = 1
            elif traci.vehicle.getSignals(self.ID) & 2 == 2:
                self.StBlinker = 2
            else:
                self.StBlinker = 0
            tmp_pos = traci.vehicle.getPosition3D(self.ID)  # position: x,y
            self.PosX_FrontBumper = tmp_pos[0]  # X position (front bumper, meters)
            self.PosY_FrontBumper = tmp_pos[1]  # Y position (front bumper, meters)
            self.PosZ_FrontBumper = tmp_pos[2]  # Z position (front bumper, meters)
            self.Velocity = traci.vehicle.getSpeed(self.ID)
            self.Heading = traci.vehicle.getAngle(self.ID)

        except:
            print("Error creating container for SUMO vehicle: ", self.ID)

    def UpdateVehicle(self):

        try:
            if traci.vehicle.getSignals(self.ID) & 8 == 8: #Bitmask - 8 for brake light
                self.StBrakePedal = True
            else:
                self.StBrakePedal = False
            # Blinker State 0->none, 1->right, 2->left
            if traci.vehicle.getSignals(self.ID) & 1 == 1:
                self.StBlinker = 1
            elif traci.vehicle.getSignals(self.ID) & 2 == 2:
                self.StBlinker = 2
            else:
                self.StBlinker = 0

            tmp_pos = traci.vehicle.getPosition3D(self.ID)  # position: x,y
            self.PosX_FrontBumper = tmp_pos[0]  # X position (front bumper, meters)
            self.PosY_FrontBumper = tmp_pos[1]  # Y position (front bumper, meters)
            self.PosZ_FrontBumper = tmp_pos[2]  # Z position (front bumper, meters)
            self.Velocity = traci.vehicle.getSpeed(self.ID)
            self.Heading = traci.vehicle.getAngle(self.ID)
            self.Edge = traci.vehicle.getRoadID(self.ID)
        except:
            print("Error updating SUMO vehicle: ", self.ID)
            # Try to put it back
            self.ReinsertVehicle()

    def ReinsertVehicle(self):

        LaneIndex = -1  # dummy
        KeepRouteMode = 1  # KeepRoute: 2 = Free move.
        try:
            traci.vehicle.add(self.ID, self.Route)  # Try to put it back
        except:
            pass # If already there, do nothing
        try:
            traci.vehicle.moveToXY(self.ID, self.Edge, LaneIndex, self.PosX_FrontBumper, self.PosY_FrontBumper, self.Heading, KeepRouteMode)
            traci.vehicle.setSpeed(self.ID, self.Velocity)
        except:
            print("Error reinserting SUMO vehicle: ", self.ID)


