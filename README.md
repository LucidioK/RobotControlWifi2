# RobotControlWIFI
Robot Control with wifi camera and wifi Arduino® (or whatever wifi controller)

This robot currently does:
1. Connect via wifi.
1. Move ahead, back.
1. Turn right, left.
1. Stop.
1. Capture image from an HTTP source.
1. Detect objects in the image.

## How to get the Code

Open a command prompt then run this:
```
https://github.com/LucidioK/RobotControlWifi2.git
cd RobotControlWifi2
initialize.cmd
```

Then edit the file `SmartRobot/secrets.h`, informing your WiFi SSID name and the password.

## What you will need

### Software

* For the PC part, you will need [Visual Studio](https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=Community&channel=Release) or [Visual Studio Code](https://code.visualstudio.com/download).
* If you opt for Visual Studio Code, install the [C# Devkit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) extension.
* For the Arduino part, you will need the [Arduino IDE](https://www.arduino.cc/en/software), or you can use the [Arduino Extension](https://marketplace.visualstudio.com/items?itemName=vsciot-vscode.vscode-arduino) in Visual Studio Code.
* You will need to install the following libraries on your Arduino (open the IDE, then Tools/Manage Libraries):
  * VL53L0X by Pololu (https://github.com/pololu/vl53l0x-arduino)
  * WiFiNINA by Arduino®
  * Arduino_LSM6DS3 by Arduino®
  * QMC5883LCompass by MPrograms
  * L298N by Andrea Lombardo

### Hardware

* You PC must have an [NVidia display adapter capable of running CUDA](https://developer.nvidia.com/cuda-gpus).
* Currently this only runs on Windows.
* Tank-style chassis with two motors, such as [this](https://www.amazon.com/dp/B0BDYHVS2P) or [this](https://www.amazon.com/dp/B09VZVFL9D)
* Arduino® Uno Wifi R2 or [Arduino® UNO R4 WiFi](https://store.arduino.cc/products/uno-r4-wifi). Other Arduino with WiFi did not work for me.
* Distance detector: [VL53L0XV2 Laser Ranger Module](https://www.amazon.com/dp/B0B6ZT7NRW)
* Compass+Accelerometer: [Adafruit LSM303DLHC Triple-axis Accelerometer+Magnetometer (Compass)](https://www.amazon.com/dp/B07X3GFKYD)
* Motor Driver: [L298N DC motor driver](https://www.amazon.com/dp/B0C5JCF5RS)

## How to use it

1. Make sure your PC is in the same WiFi network as configured in `SmartRobot/secrets.h`
1. Open the Arduino IDE then open `SmartRobot.ino`, which will be under directory `RobotControlWifi2\SmartRobot`
1. Connect the Arduino device with your PC via USB cable.
1. If the Arduino IDE does not recognize your device immediately, open Tools/Board/Boards Manager and install `Arduino AVR Boards`.
1. After the IDE and the device are in good terms, select the port where the device is.
1. Upload the code: Either click on the right-arrow icon, or select Sketch/Upload.
1. Open the Serial Monitor (menu Tools/Serial Monitor).
1. Select 74880 bauds.
1. There will be many messages in the Serial Monitor, wait until it says "Ready!".
1. Open Visual Studio, then open `RobotControlWifi2\RobotControl.sln`
1. Press `F5` to start the program.
1. Click on the `Connect` button, to connect to the robot. This will try to find the robot in the WiFi network. It takes about 1-2 minutes.
1. Either detach the robot from the USB cable and put it on the floor, or suspend it so the tracks do not touch anything.
1. Move the `L` and `R` slider to the `150` position.
1. Now click Ahead, Left, Right, or Back. Click Stop to stop the robot. Have fun.

## Current issues

### Implement scan & run 
Now that we can move the robot and detect object, we need to move the robot in the direction of the object.



