using FluentValidation;
using FacturacionVERIFACTU.API.DTOs;

namespace FacturacionVERIFACTU.API.Validators
{
    /// <summary>
    /// Validador para CrearProductoDto
    /// </summary>
    public class CrearProductoValidator : AbstractValidator<CrearProductoDto>
    {
        public CrearProductoValidator()
        {
            RuleFor(x => x.Codigo)
                .NotEmpty().WithMessage("El código es obligatorio")
                .MaximumLength(50).WithMessage("El código no puede superar 50 caracteres");

            RuleFor(x => x.Descripcion)
                .NotEmpty().WithMessage("La descripción es obligatoria")
                .MaximumLength(500).WithMessage("La descripción no puede superar 500 caracteres");

            RuleFor(x => x.PrecioUnitario)
                .GreaterThan(0).WithMessage("El precio debe ser mayor que 0")
                .LessThan(1000000).WithMessage("El precio no puede superar 999,999");

            RuleFor(x => x.Unidad)
                .MaximumLength(10).When(x => !string.IsNullOrEmpty(x.Unidad));
        }
    }

    /// <summary>
    /// Validador para ActualizarProductoDto
    /// </summary>
    public class ActualizarProductoValidator : AbstractValidator<ActualizarProductoDto>
    {
        public ActualizarProductoValidator()
        {
            RuleFor(x => x.Descripcion)
                .NotEmpty().WithMessage("La descripción es obligatoria")
                .MaximumLength(500).WithMessage("La descripción no puede superar 500 caracteres");

            RuleFor(x => x.PrecioUnitario)
                .GreaterThan(0).WithMessage("El precio debe ser mayor que 0")
                .LessThan(1000000).WithMessage("El precio no puede superar 999,999");

            RuleFor(x => x.Unidad)
                .MaximumLength(10).When(x => !string.IsNullOrEmpty(x.Unidad));
        }
    }
}
