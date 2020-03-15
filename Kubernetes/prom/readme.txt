https://kubernetes.github.io/ingress-nginx/user-guide/monitoring/

#deployed with these images 
prom/prometheus:v2.3.2
grafana/grafana:6.1.6

#dashboards link
https://github.com/kubernetes/ingress-nginx/tree/master/deploy/grafana/dashboards


kubectl apply -k prometheus/


#prometheus.yaml contains jobs for pulling metrics from
# 1-ingress-nginx-endpoints
# 2- kube-state-metrics
# 3- metrics-server-exporter 

kubectl apply -k grafana/

#login to grafana by node port 
#add doshboard by adding json in dashboard folder 