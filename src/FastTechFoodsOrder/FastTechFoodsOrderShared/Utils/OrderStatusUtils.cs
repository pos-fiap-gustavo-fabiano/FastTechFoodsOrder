
using FastTechFoodsOrder.Shared.Enums;

namespace FastTechFoodsOrder.Shared.Utils
{
    /// <summary>
    /// Classe utilitária para conversões e operações relacionadas ao OrderStatus
    /// </summary>
    public static class OrderStatusUtils
    {
        /// <summary>
        /// Converte uma string para o enum OrderStatus correspondente
        /// </summary>
        /// <param name="status">String do status a ser convertida</param>
        /// <returns>OrderStatus correspondente ou null se inválido</returns>
        public static OrderStatus? ConvertStringToStatus(string? status)
        {
            return status?.ToLower() switch
            {
                "pending" => OrderStatus.Pending,
                "accepted" => OrderStatus.Accepted,
                "preparing" => OrderStatus.Preparing,
                "ready" => OrderStatus.Ready,
                "cancelled" => OrderStatus.Cancelled,
                "delivered" => OrderStatus.Delivered,
                "received" => OrderStatus.Pending, // Mapeamento para compatibilidade
                _ => null
            };
        }

        /// <summary>
        /// Converte um enum OrderStatus para string em lowercase
        /// </summary>
        /// <param name="status">OrderStatus a ser convertido</param>
        /// <returns>String do status em lowercase</returns>
        public static string ConvertStatusToString(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "pending",
                OrderStatus.Accepted => "accepted",
                OrderStatus.Preparing => "preparing",
                OrderStatus.Ready => "ready",
                OrderStatus.Cancelled => "cancelled",
                OrderStatus.Delivered => "delivered",
                _ => "unknown"
            };
        }

        /// <summary>
        /// Verifica se uma string representa um status válido
        /// </summary>
        /// <param name="status">String do status a ser validada</param>
        /// <returns>True se válido, false caso contrário</returns>
        public static bool IsValidStatus(string? status)
        {
            return ConvertStringToStatus(status) != null;
        }

        /// <summary>
        /// Obtém todos os status válidos como strings
        /// </summary>
        /// <returns>Lista de strings com todos os status válidos</returns>
        public static IEnumerable<string> GetAllValidStatuses()
        {
            return Enum.GetValues<OrderStatus>()
                .Select(ConvertStatusToString)
                .Where(s => s != "unknown");
        }

        /// <summary>
        /// Verifica se um status pode ser atualizado para outro status
        /// </summary>
        /// <param name="currentStatus">Status atual</param>
        /// <param name="newStatus">Novo status desejado</param>
        /// <returns>True se a transição é válida</returns>
        public static bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            // Regras de transição de status
            return currentStatus switch
            {
                OrderStatus.Pending => newStatus is OrderStatus.Accepted or OrderStatus.Cancelled,
                OrderStatus.Accepted => newStatus is OrderStatus.Preparing or OrderStatus.Cancelled,
                OrderStatus.Preparing => newStatus is OrderStatus.Ready or OrderStatus.Cancelled,
                OrderStatus.Ready => newStatus is OrderStatus.Delivered,
                OrderStatus.Cancelled => false, // Status final - não pode ser alterado
                OrderStatus.Delivered => false, // Status final - não pode ser alterado
                _ => false
            };
        }

        /// <summary>
        /// Obtém a descrição amigável do status
        /// </summary>
        /// <param name="status">Status a ser descrito</param>
        /// <returns>Descrição amigável do status</returns>
        public static string GetStatusDescription(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Pendente",
                OrderStatus.Accepted => "Aceito",
                OrderStatus.Preparing => "Em Preparação",
                OrderStatus.Ready => "Pronto",
                OrderStatus.Cancelled => "Cancelado",
                OrderStatus.Delivered => "Entregue",
                _ => "Status Desconhecido"
            };
        }
    }
}
