# =================================================================
# Copyright (C) 2020 Hochschule Karlsruhe - Technik und Wirtschaft
# This program and the accompanying materials
# are made available under the terms of the MIT license.
# =================================================================
# Authors: Anna-Lena Marmein, Magdalena Kugler, Yannick Rauch,
#          Patrick Rebling
# =================================================================

import os
import sys
import time
import xml.etree.ElementTree as ET
from queue import Queue
from threading import Thread
import TrafficSimulator
import SocketServer

class SumoUnity():

    def __init__(self, uri, port, path, sumo_step_size):

        self.uri = uri
        self.port = port

        # Define queues for communication
        self.unity_queue = Queue(maxsize=1)
        self.sumo_queue = Queue(maxsize=1)
        
        # Launch SUMO
        self.sumo_step_size = sumo_step_size
        self.traffic_sim = TrafficSimulator.TrafficSimulator(path)
        self.sumo_objects = []

        # Initiate ego vehicle
        self.ego_vehicle = {
            "isSet": False,
            "isReceived": False,
            "id": "ego_vehicle",
            "vehicleType": "EgoVehicle",
            "route": "route_outside",
            "edge": "O_E1",
            "laneIndex": -1,
            "posX": 0.0,
            "posY": 0.0,
            "heading": 0.0,
            "keepRouteMode": 2,
            "velocity": 0.0,
            "brakeState": 0,
            "drivingBackwards": 0,
            "signalLights": "Off"
        }

    def sumoThread(self):

        sumo_time_stamp = 0.0

        while True:

            # Get timestamp
            t_1 = time.time()

            # Receive Ego Vehicle Data
            if (not self.sumo_queue.empty()):
                data = self.sumo_queue.get()
                self.sumo_queue.queue.clear()
                data = data.replace(",", ".")
                data = data.split(";")
                self.ego_vehicle["isReceived"] = True
                self.ego_vehicle["posX"] = float(data[0])
                self.ego_vehicle["posY"] = float(data[1])
                self.ego_vehicle["velocity"] = float(data[2])
                self.ego_vehicle["heading"] = float(data[3])
                self.ego_vehicle["brakeState"] = int(data[4])
                self.ego_vehicle["drivingBackwards"] = int(data[5])
                self.ego_vehicle["signalLights"] = str(data[6])

            # Update SUMO
            self.sumo_objects, self.ego_vehicle = self.traffic_sim.StepSumo(self.sumo_objects, self.ego_vehicle)
            self.ego_vehicle["isReceived"] = False

            # Current Sumo Time
            sumo_time_stamp += self.sumo_step_size

            # Update Unity
            SocketServer.prepareUnityData(self.sumo_objects, self.unity_queue, sumo_time_stamp, time.time() - t_1, self.sumo_step_size)

            # Synchronize time
            t_2 = time.time() - t_1
            if t_2 > self.sumo_step_size:
                pass
            else:
                time.sleep(self.sumo_step_size - t_2)

    def main(self):

        # Start Sumo simulation thread
        sumo_thread = Thread(target=self.sumoThread)
        sumo_thread.start()
        # Start the websocket server
        self.server = SocketServer.SocketServer(self.uri, self.port, self.sumo_queue, self.unity_queue)
        self.server.serverStart()


if __name__ == "__main__":

    # Get arguments from command line
    uri, port, network, sumo_step_size = sys.argv[1], int(sys.argv[2]), sys.argv[3], float(sys.argv[4])
    network_cfg = network + ".sumocfg"
    # Get absolute path to config file of desired network
    sumo_network = os.path.join(network, network_cfg)
    folder_path = os.path.join(os.path.dirname(__file__),"..","SumoNetworks","")
    abs_path_to_cfg = os.path.join(folder_path, sumo_network)
    # Parse xml config file and modify the step-length
    tree = ET.parse(abs_path_to_cfg)
    root = tree.getroot()
    try:
        cfg_step_size = float(root.find('step-length').get('value'))
        if (cfg_step_size != sumo_step_size):
            root.find('step-length').set('value', str(sumo_step_size))
    except:
        time_info = ET.SubElement(root, 'step-length')
        time_info.set('value', str(sumo_step_size))
    tree.write(abs_path_to_cfg)
    # Run
    simulation = SumoUnity(uri, port, abs_path_to_cfg, sumo_step_size)
    simulation.main()