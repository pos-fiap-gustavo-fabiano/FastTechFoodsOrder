# FastTechFoodsOrder - Deploy com Azure Key Vault

## Pré-requisitos
echo "Verificando pré-requisitos..."

# 1. Verificar se o Secret Store CSI Driver está instalado no AKS
kubectl get pods -n kube-system | grep secrets-store

# Se não estiver instalado, instalar:
# helm repo add secrets-store-csi-driver https://kubernetes-sigs.github.io/secrets-store-csi-driver/charts
# helm install csi-secrets-store secrets-store-csi-driver/secrets-store-csi-driver --namespace kube-system

# 2. Verificar se o Azure Key Vault Provider está instalado
kubectl get pods -n kube-system | grep azure-csi

# Se não estiver instalado, instalar:
# kubectl apply -f https://raw.githubusercontent.com/Azure/secrets-store-csi-driver-provider-azure/master/deployment/provider-azure-installer.yaml

## Deploy dos recursos
echo "Aplicando configurações do Kubernetes..."

# 1. Aplicar namespace (se necessário)
kubectl create namespace fasttechfoods --dry-run=client -o yaml | kubectl apply -f -

# 2. Aplicar Service Account com Workload Identity
kubectl apply -f 03-service-account.yaml

# 3. Aplicar Secret Provider Class
kubectl apply -f 01-secret-provider-class.yaml

# 4. Aplicar Deployment
kubectl apply -f 02-order-api-deployment.yaml

echo "Deploy concluído!"
echo ""
echo "Para verificar o status:"
echo "kubectl get pods -n fasttechfoods"
echo "kubectl get secrets -n fasttechfoods"
echo "kubectl describe secretproviderclass fasttechfoods-order-secrets -n fasttechfoods"
