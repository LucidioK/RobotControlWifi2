# RobotControlWIFI
Robot Control with wifi camera and wifi Arduino (or whatever wifi controller)
# IMPORTANT:
This is *NOT* working yet. If you want a Robot Control that works, take a look at [RobotControl03](https://github.com/LucidioK/RobotControl03).

However, that Robot Control requires a PC physically attached to the robot, this is trying to control the robot fully remotely.

# Current issues

## Reduce 1-2 Second delay from captured image to less than 100ms
Captured video from RTSP streaming is 1-2 seconds delayed.
Even when played with [ffplay](https://ffmpeg.org/ffplay.html), it is still delayed.
<br/>
When played with Amcrest IP Config, there's almost no delay, though.
<br/>
Need to reduce the delay to 100ms or less.

## Rebuild the robot around the new Arduino WIFI
This is the physical rebuild.
I will create a 3D printed platform to place the Arduino with sensors and motor shield on a PCB board.

## Create the logic to send robot commands wirelessly to the Robot on the PC side
Refactor [RobotCommunication](https://github.com/LucidioK/RobotControlWIFI/blob/main/RobotControl.ClassLibrary/RobotCommunication/RobotCommunication.cs) to run against WIFI instead of COM port.

## Create the logic to receive commands wirelessly on the Arduino side
[SmartRobot.ino](https://github.com/LucidioK/RobotControlWIFI/blob/main/SmartRobot/SmartRobot.ino) has a very basic code that creates a WIFI spot but only allows for setting the board LED on or off. Must bring the functionality from [RobotControl03 SmartRobot.ino](https://github.com/LucidioK/RobotControl03/blob/main/SmartRobot03/SmartRobot03.ino), but using WIFI in a way that does not induce network overload.
