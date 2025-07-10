using FastTechFoodsOrder.Application.DTOs;
using FluentValidation;

namespace FastTechFoodsOrder.Application.Validators
{
    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderDto>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.CustomerId).NotEmpty().WithMessage("ID do cliente é obrigatório");
            RuleFor(x => x.DeliveryMethod).NotEmpty().WithMessage("O método de entrega é obrigatório.");
            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Pelo menos um item do pedido é obrigatório.")
                .Must(items => items.All(item => item.Quantity > 0))
                .WithMessage("Todos os itens do pedido devem ter uma quantidade maior que zero.");
        }
    }
}
