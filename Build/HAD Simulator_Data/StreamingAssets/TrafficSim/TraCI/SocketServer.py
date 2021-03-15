# =================================================================
# Copyright (C) 2020 Hochschule Karlsruhe - Technik und Wirtschaft
# This program and the accompanying materials
# are made available under the terms of the MIT license.
# =================================================================
# Authors: Anna-Lena Marmein, Magdalena Kugler, Yannick Rauch,
#          Patrick Rebling
# =================================================================

import websockets
import asyncio
import time

class SocketServer():

    def __init__(self, ip, port, queue_in, queue_out):
        self.ip = ip
        self.port = port
        self.queue_in = queue_in
        self.queue_out = queue_out
        #Create socket object
        print("Starting WebSocket server at ws://" + str(self.ip) + ":" + str(self.port))

    def serverStart(self):
        socket_server = websockets.serve(self.serverMain, self.ip, self.port)
        asyncio.get_event_loop().run_until_complete(socket_server)
        asyncio.get_event_loop().run_forever()
        print("Disconnected")

    # Server main task
    async def serverMain(self, websocket, path):
        recv_task = asyncio.ensure_future(
            self.recv_ws(websocket, path))
        send_task = asyncio.ensure_future(
            self.send_ws(websocket, path))
        done, pending = await asyncio.wait(
            [recv_task, send_task],
            return_when=asyncio.FIRST_COMPLETED,
        )
        for task in pending:
            task.cancel()

    # Asynchronous receiving task
    async def recv_ws(self, websocket, path):
        async for message in websocket:
            msg_in = message.split("@")
            self.queue_in.queue.clear()
            self.queue_in.put(msg_in[1])

    # Asynchronous sending task
    async def send_ws(self, websocket, path):
        while True:
            if not self.queue_out.empty():
                msg_out = self.queue_out.get()
                await websocket.send(msg_out)
            await asyncio.sleep(0.005)


def prepareUnityData(vehicles, unity_queue, t_stamp, step_current, step_target):

    if (step_current < step_target):
        step_current = step_target
    data = "O1G" + "{0:.3f}".format(t_stamp) + "#" + "{0:.3f}".format(step_target) + "#" + "{0:.3f}".format(step_current) + "#"

    # vehicles in the simulation
    for veh in vehicles:
        data += veh.ID + ";" + "{0:.3f}".format(veh.PosX_FrontBumper) + ";" + "{0:.3f}".format(veh.PosY_FrontBumper) + ";" + "{0:.3f}".format(veh.PosZ_FrontBumper) + ";" + "{0:.2f}".format(veh.Velocity) + ";"  + "{0:.2f}".format(veh.Heading) + ";" + str(veh.Length) +  ";" + str(int(veh.StBrakePedal)) + ";" + str(veh.StBlinker) + "@"

    # ddd line break
    data += "&\n"

    unity_queue.queue.clear()
    unity_queue.put(data)  # Enqueue the updated data.