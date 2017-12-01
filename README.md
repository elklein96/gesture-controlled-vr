# Gesture Controlled VR

## What is this?
- This project allows for the control of a VR scene through Gesture Control.

## Project Structure
- The `echo-server` directory contains a simple Node.js websocket server which broadcasts data it recieves to all subscribed clients.
- The `Virtual Office` directory contains a Unity scene which subscribes to a websocket server and logs any received messages.
- The `Camera Control` directory contains a C++ project which connects to an Intel R200 RealSense Camera and transmits data to a socket server.
- The `mouse-capture` directory contains an application that captures mouse coordinates and transmits them to a socket server. This is used as a way to send a constant stream of data when debugging without the camera.


## Building the Project
- Setting up the `echo-server` app
```
cd echo-server
npm install
npm start
```
- Setting up the `mouse-capture` app
```
cd mouse-capture
npm install
npm start
```
- Setting up the `Camera Control` project
    >> The [Boost C++ Libraries](http://www.boost.org/) and [Boost.Beast](https://github.com/boostorg/beast) are prerequisites for building this project
    - Open Camera Control.xcodeproj in Xcode
    - Plug in the R200 camera
    - Run the project

- Setting up the Unity project
    - Open the `Virtual Office.sln` in the Unity editor
    - Click the `Play` button in the top toolbar

## Running an Example
- With all three projects running, move your mouse around the window
- Check that the Unity console displays coordinates corresponding to your mouse movements
