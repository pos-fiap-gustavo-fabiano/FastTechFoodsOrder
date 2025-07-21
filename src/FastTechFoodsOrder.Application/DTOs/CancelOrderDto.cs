
namespace FastTechFoodsOrder.Application.DTOs
{
    public class CancelOrderDto
    {
        public string Status { get; set; }
        public string UpdatedBy { get; set; }
        public string CancelReason
        {
            get; set;
        }
    }
}
