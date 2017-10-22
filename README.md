# Gesture Controlled VR

## What is this?
- This project allows for the control of a VR scene through Gesture Control.

## Project Structure
- The `echo-server` directory contains a simple Node.js websocket server which broadcasts data it recieves to all subscribed clients.
- The `Virtual Office` directory contains a Unity scene which subscribes to a websocket server and logs any received messages.

## Building the Project
- Setting up the `echo-server`
```
cd echo-server
npm install
npm run start
```
- Setting Up the Unity Project
    - Open the `Virtual Office.sln` in the Unity editor
    - Click the `Play` button in the top toolbar

## Running an Example
- The `echo-server` project listens for `POST` requests received at `localhost:3000/ping` and repeats the value of the `payload` field to any subscribers of the websocket server found at `localhost:3001`
- With both  projects running, send the following example request:
```
curl -X POST \
  http://localhost:3000/ping \
  -H 'content-type: application/json' \
  -d '{
	"payload": "(1, 2)"
  }'
```
- Check that the Unity console displays the following output: 
```
Open
connected
(1, 2)
```