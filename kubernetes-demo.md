apiVersion: apps/v1
kind: Deployment
metadata:  
  name: aspnet-deployment  
  labels:
    app: apiapp
spec:
  replicas: 1
  selector:
    matchLabels:
      app: apiapp
  template:
    metadata:
      labels:
        app: apiapp
    spec:
      containers:
      - name: apiapp
        image: madhub2018/netcoreapi
        imagePullPolicy: IfNotPresent
        ports:
        - containerPort: 80  




apiVersion: v1
kind: Service
metadata:
  name: aspdemo
spec:
  selector:
    app: apiapp
  ports:
  - protocol: TCP
    port: 80
    targetPort: 80
    
    
    
const http = require('http');
const os = require('os');

console.log("Kubia server starting...");

var handler = function(request, response) {
  console.log("Received request from " + request.connection.remoteAddress);
  response.writeHead(200);
  response.end("You've hit " + os.hostname() + "\n");
};


apiVersion: apps/v1
kind: Deployment                 
metadata:
  name: kubia 
  labels:
    app: kubia
spec:
  replicas: 2
  selector:
    matchLabels:
      app: kubia
  template:
    metadata:
      labels:
        app: kubia
    spec:
      containers:
      - name: kubia
        image: madhub2018/kubia:latest
        imagePullPolicy: IfNotPresent
        ports:
        - containerPort: 8080  


apiVersion: v1
kind: Service
metadata:
  name: kubia
spec:
  ports:
  - port: 80                
    targetPort: 8080        
  selector:                 
    app: kubia              
var www = http.createServer(handler);
www.listen(8080);
