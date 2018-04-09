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

  console.log('Created a new connection');

  ws.on('message', data => {
    if (process.env.AUDIT_FLAG) {
      console.log(data, new Date().getTime());
    }
    connections.forEach(subscriber => {
      if (subscriber !== ws && subscriber.readyState === WebSocket.OPEN) {
        subscriber.send(data);
      }
    });
  });

  ws.on('error', (err) => {});
  ws.onclose = removeSubscriber;

  ws.send('connected');
});

app.post('/ping', (req, res, next) => {
  notifySubscribers(req.body.payload);
  res.send({ data: payload });
  return next();
});

app.get('/connections', (req, res, next) => {
  res.send({ data: `There are currently ${connections.length} subscribers` });
  return next();
});

app.delete('/connections', (req, res, next) => {
  connections.forEach(subscriber => {
    subscriber.close();
  });

  res.send({ data: 'Ok' });
  return next();
});

http.createServer(app).listen(app.get('port'), () => {
  console.log(`Express server listening on port ${app.get('port')}`);
});

function notifySubscribers(data) {
  connections.forEach(subscriber => {
    subscriber.send(data);
  });
}

function removeSubscriber(socket) {
  connections.splice(socket, 1);
}
