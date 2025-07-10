using FastTechFoodsOrder.Application.DTOs;
using FluentValidation;

namespace FastTechFoodsOrder.Application.Validators
{
    internal class UpdateOrderSatusValidator : AbstractValidator<UpdateOrderStatusDto>
    {
        public UpdateOrderSatusValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("O status do pedido é obrigatório.");
        }
    }
}
