https://blog.codewithdan.com/enabling-metrics-server-for-kubernetes-on-docker-desktop/

#add insecure tls arg to deployment args

#pull this image
docker pull k8s.gcr.io/metrics-server-amd64:v0.3.6

#deploy metrics-server
kubectl apply -f metrics-server/deploy/


#then we need metrics-server-exporter
https://github.com/grupozap/metrics-server-exporter

#pull this image
docker pull vivareal/metrics-server-exporter:v0.0.6

#deploy metrics-server-exporter
kubectl apply -f metrics-server-exporter/deploy/