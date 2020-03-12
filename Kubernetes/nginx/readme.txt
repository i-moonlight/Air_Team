kubectl create configmap nginxconfigmap --from-file=default.conf


#create cert and key file with this commands
openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout /d/tmp/nginx.key -out /d/tmp/nginx.crt -subj "/CN=my-nginx/O=my-nginx"

# Convert the keys to base64 encoding
cat /d/tmp/nginx.crt | base64
cat /d/tmp/nginx.key | base64

add those to nginxsecrets.yaml, The base64 encoded value should all be on a single line.

kubectl apply -f nginxsecrets.yaml

# pull image used in next deployment
docker pull bprashanth/nginxhttps:1.1

# apply nginx service and deployment in single file
# if service created after deploy kill pods to create them again
# for correct configured EV
kubectl apply -f nginx-secure-app.yaml
