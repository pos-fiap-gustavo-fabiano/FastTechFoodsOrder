# FastTechFoodsOrder - MassTransit Integration

## Configuração do RabbitMQ

### Variáveis de Ambiente

Para que o sistema funcione corretamente, você precisa configurar a seguinte variável de ambiente:

```bash
RABBITMQ_URI=rabbitmq://guest:guest@localhost:5672
```

### Executando o RabbitMQ com Docker

Se você não tem o RabbitMQ instalado, pode executá-lo usando Docker:

```bash
docker run -d --hostname my-rabbit --name some-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

Isso irá:
- Expor a porta 5672 para conexões AMQP
- Expor a porta 15672 para o painel de gerenciamento web
- Usuário padrão: `guest`
- Senha padrão: `guest`

### Acesso ao Painel de Gerenciamento

Após executar o RabbitMQ, você pode acessar o painel de gerenciamento em:
- URL: http://localhost:15672
- Usuário: guest
- Senha: guest

## Como Funciona

### Publicação de Mensagens

Quando um pedido tem seu status atualizado através do método `UpdateOrderStatusAsync`, o sistema:

1. Busca o pedido atual para obter o status anterior
2. Atualiza o status no banco de dados
3. Publica uma mensagem específica para o novo status na fila correspondente

### Filas Criadas

O sistema cria as seguintes filas automaticamente:

- `order-pending-queue` - Para pedidos com status "pending"
- `order-accepted-queue` - Para pedidos com status "accepted"
- `order-preparing-queue` - Para pedidos com status "preparing"
- `order-ready-queue` - Para pedidos com status "ready"
- `order-completed-queue` - Para pedidos com status "completed"
- `order-cancelled-queue` - Para pedidos com status "cancelled"

### Mensagens Publicadas

Cada atualização de status gera uma mensagem específica contendo:

```json
{
  "orderId": "string",
  "customerId": "string",
  "previousStatus": "string",
  "newStatus": "string",
  "updatedBy": "string",
  "updatedAt": "datetime",
  "cancelReason": "string (opcional)",
  "orderDetails": {
    // Objeto completo do pedido
  }
}
```

### Consumers

O sistema inclui consumers de exemplo para cada tipo de mensagem que você pode customizar:

- `OrderPendingConsumer` - Processa pedidos pendentes
- `OrderAcceptedConsumer` - Processa pedidos aceitos
- `OrderPreparingConsumer` - Processa pedidos em preparação
- `OrderReadyConsumer` - Processa pedidos prontos
- `OrderCompletedConsumer` - Processa pedidos completados
- `OrderCancelledConsumer` - Processa pedidos cancelados

## Exemplo de Uso

### Atualizar Status do Pedido

```csharp
PUT /api/orders/{id}/status
{
  "status": "preparing",
  "updatedBy": "system",
  "cancelReason": null
}
```

Isso irá:
1. Atualizar o pedido no banco
2. Publicar uma `OrderPreparingMessage` na fila `order-preparing-queue`
3. O `OrderPreparingConsumer` processará a mensagem

### Configuração Personalizada

Para usar uma URI diferente do RabbitMQ, configure a variável de ambiente:

```bash
# Para desenvolvimento local
RABBITMQ_URI=rabbitmq://localhost

# Para produção com credenciais
RABBITMQ_URI=rabbitmq://usuario:senha@servidor:5672/vhost
```

## Monitoramento

Você pode monitorar as filas e mensagens através do painel de gerenciamento do RabbitMQ em http://localhost:15672, onde poderá ver:

- Número de mensagens em cada fila
- Taxa de publicação e consumo
- Conexões ativas
- Exchanges e bindings criados automaticamente pelo MassTransit
