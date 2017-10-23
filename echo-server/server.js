const express = require('express');
const bodyParser  = require('body-parser');
const http = require('http');
const WebSocket = require('ws');

const wss = new WebSocket.Server({ port: process.env.PORT || 3001 });
const app = express();

app.set('port', process.env.PORT || 3000);
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({
  extended: true
}));

const connections = [];

wss.on('connection', (ws) => {
  connections.push(ws);

  ws.on('message', notifySubscribers);
  ws.onclose = removeSubscriber;

  ws.send('connected');
});

app.post('/ping', (req, res, next) => {
  notifySubscribers(req.body.payload);

  res.send({ data: payload });
  return next();
});

http.createServer(app).listen(app.get('port'), () => {
  console.log(`Express server listening on port ${app.get('port')}`);
});

function notifySubscribers(data) {
  connections.forEach((subscriber) => {
    subscriber.send(data);
  });
}

function removeSubscriber(socket) {
  connections.splice(socket);
}
