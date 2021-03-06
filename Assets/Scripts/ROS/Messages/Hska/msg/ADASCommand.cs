/* 
 * This message is auto generated by ROS#. Please DO NOT modify.
 * Note:
 * - Comments from the original code will be written in their own line 
 * - Variable sized arrays will be initialized to array of size 0 
 * Please report any issues at 
 * <https://github.com/siemens/ros-sharp> 
 */



using RosSharp.RosBridgeClient.MessageTypes.Std;

namespace RosSharp.RosBridgeClient.MessageTypes.Hska
{
    public class ADASCommand : Message
    {
        public const string RosMessageName = "hska_msgs/ADASCommand";

        //  Desired State of the Advanced Driver Assistance Systems
        public Header header { get; set; }
        public const byte NONE = 0;
        public const byte LEFT = 1;
        public const byte RIGHT = 2;
        public bool lkas { get; set; }
        //  Lane Keeping Assist System, 0 = inactive  or 1 = active
        public byte lane_change_req { get; set; }
        //  Lane change request for left and right
        public bool cc { get; set; }
        //  Cruise Control, 0 = inactive  or 1 = active
        public float cmd_velocity { get; set; }
        //  Command Velocity for Cruise Controller [m/s]

        public ADASCommand()
        {
            this.header = new Header();
            this.lkas = false;
            this.lane_change_req = 0;
            this.cc = false;
            this.cmd_velocity = 0.0f;
        }

        public ADASCommand(Header header, bool lkas, byte lane_change_req, bool cc, float cmd_velocity)
        {
            this.header = header;
            this.lkas = lkas;
            this.lane_change_req = lane_change_req;
            this.cc = cc;
            this.cmd_velocity = cmd_velocity;
        }
    }
}
