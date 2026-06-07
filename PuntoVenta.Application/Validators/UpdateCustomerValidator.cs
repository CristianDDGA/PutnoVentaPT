using FluentValidation;
using PuntoVenta.Application.DTOs.Customer;

namespace PuntoVenta.Application.Validators;

public class UpdateCustomerValidator : AbstractValidator<UpdateCustomerDto>
{
    public UpdateCustomerValidator()
    {
        RuleFor(customer => customer.DocumentNumber)
            .NotEmpty().WithMessage("El número de documento (RUC/Cédula) es obligatorio.")
            .Must(doc => !string.IsNullOrWhiteSpace(doc)).WithMessage("El número de documento no puede contener solo espacios en blanco.")
            .MaximumLength(20).WithMessage("El número de documento no puede superar 20 caracteres.");
            //.Must(IsValidEcuadorianDocument).WithMessage("La cédula ingresada no es válida (Módulo 10).");

        RuleFor(customer => customer.FirstName)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("El nombre no puede contener solo espacios en blanco.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(customer => customer.LastName)
            .NotEmpty().WithMessage("El apellido es obligatorio.")
            .Must(last => !string.IsNullOrWhiteSpace(last)).WithMessage("El apellido no puede contener solo espacios en blanco.")
            .MaximumLength(100).WithMessage("El apellido no puede superar 100 caracteres.");

        RuleFor(customer => customer.Email)
            .EmailAddress().WithMessage("El correo no tiene un formato válido.")
            .MaximumLength(150).WithMessage("El correo no puede superar 150 caracteres.")
            .When(customer => !string.IsNullOrWhiteSpace(customer.Email));

        RuleFor(customer => customer.Phone)
            .MaximumLength(10).WithMessage("El teléfono no puede superar 10 caracteres.")
            .Matches(@"^\d*$").WithMessage("El teléfono solo puede contener dígitos.")
            .When(customer => !string.IsNullOrWhiteSpace(customer.Phone));

        RuleFor(customer => customer.Address)
            .MaximumLength(200).WithMessage("La dirección no puede superar 200 caracteres.")
            .When(customer => !string.IsNullOrWhiteSpace(customer.Address));

        RuleFor(customer => customer.City)
            .MaximumLength(100).WithMessage("La ciudad no puede superar 100 caracteres.")
            .When(customer => !string.IsNullOrWhiteSpace(customer.City));
    }

    private bool IsValidEcuadorianDocument(string document)
    {
        if (string.IsNullOrWhiteSpace(document)) return false;

        // Limpiar espacios y validar longitud
        var doc = document.Trim();
        if (doc.Length != 10 && doc.Length != 13) return false;
        if (!doc.All(char.IsDigit)) return false;

        // Si es RUC de persona natural, debe terminar en 001
        if (doc.Length == 13 && !doc.EndsWith("001")) return false;

        // Validar como cédula (los primeros 10 dígitos)
        var cedula = doc.Substring(0, 10);
        
        // Código de provincia (01 a 24, 30 en el exterior)
        var provinceCode = int.Parse(cedula.Substring(0, 2));
        if ((provinceCode < 1 || provinceCode > 24) && provinceCode != 30) return false;

        // El tercer dígito es menor a 6 para personas naturales
        var thirdDigit = int.Parse(cedula.Substring(2, 1));
        if (thirdDigit >= 6) return true; 

        // Algoritmo Módulo 10
        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            int digit = cedula[i] - '0';
            if (i % 2 == 0) 
            {
                digit *= 2;
                if (digit > 9) digit -= 9;
            }
            sum += digit;
        }

        int checkDigit = cedula[9] - '0';
        int calculatedCheckDigit = sum % 10 == 0 ? 0 : 10 - (sum % 10);

        return checkDigit == calculatedCheckDigit;
    }
}
