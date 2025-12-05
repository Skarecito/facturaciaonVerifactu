using FluentValidation;
using FacturacionVERIFACTU.API.DTOs;

namespace FacturacionVERIFACTU.API.Validators
{
    /// <summary>
    /// Validator para CrearClientDto
    /// </summary>
    public class CrearClienteValidator : AbstractValidator<CrearClienteDto>
    {
        public CrearClienteValidator()
        {
            RuleFor(x=>x.NIF)
                .NotEmpty().WithMessage("El NIF es obligatorio")
                .MaximumLength(20).WithMessage("El NIF no puede superar 20 caracteres")
                .Matches(@"^[A-Z0-9]+$").WithMessage("El NIF solo puede contener letras mayúsculas y números");

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es obligatorio")
                .MaximumLength(200).WithMessage("El nombre no puede superar los 200 caracteres");

            RuleFor(x => x.Direccion)
                .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Nombre));

            RuleFor(x=>x.CodigoPostal)
                .MaximumLength(10).When(x=> !string.IsNullOrEmpty(x.CodigoPostal))
                .Matches(@"^\d{5}$").When(x => !string.IsNullOrEmpty(x.CodigoPostal))
                .WithMessage("El código postal debe tener 5 dígitos");

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("El email no es valid");

            RuleFor(x=> x.Telefono)
                .MaximumLength(20).When(x=> !string.IsNullOrEmpty (x.Telefono));

        }

        /// <summary>
        /// Validador para ActualizarClienteDto
        /// </summary>
        public class ActualizarClienteValidator : AbstractValidator<ActualizarClienteDto>
        {
            public ActualizarClienteValidator()
            {
                RuleFor(x => x.Nombre)
                    .NotEmpty().WithMessage("El nombre es obligatorio")
                    .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres");

                RuleFor(x => x.Direccion)
                    .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Direccion));

                RuleFor(x => x.CodigoPostal)
                    .MaximumLength(10).When(x => !string.IsNullOrEmpty(x.CodigoPostal))
                    .Matches(@"^\d{5}$").When(x => !string.IsNullOrEmpty(x.CodigoPostal))
                    .WithMessage("El código postal debe tener 5 dígitos");

                RuleFor(x => x.Email)
                    .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                    .WithMessage("El email no es válido");

                RuleFor(x => x.Telefono)
                    .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Telefono));
            }
        }
    }
}
