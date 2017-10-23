# Gesture Controlled VR

## What is this?
- This project allows for the control of a VR scene through Gesture Control.

## Project Structure
- The `echo-server` directory contains a simple Node.js websocket server which broadcasts data it recieves to all subscribed clients.
- The `Virtual Office` directory contains a Unity scene which subscribes to a websocket server and logs any received messages.

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
- Setting Up the Unity Project
    - Open the `Virtual Office.sln` in the Unity editor
    - Click the `Play` button in the top toolbar

## Running an Example
- With all three projects running, move your mouse around the window
- Check that the Unity console displays coordinates corresponding to your mouse movements
