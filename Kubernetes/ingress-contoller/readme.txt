helm upgrade --install ingress-nginx ingress-nginx   --repo https://kubernetes.github.io/ingress-nginx   --namespace ingress-nginx --create-namespace  --set controller.image.digest=null --set controller.admissionWebhooks.enabled=false

https://github.com/kubernetes/ingress-nginx/tree/main/charts/ingress-nginx
https://kubernetes.github.io/ingress-nginx/deploy/#quick-start


