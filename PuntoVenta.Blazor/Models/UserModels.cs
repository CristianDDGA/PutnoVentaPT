using System.ComponentModel.DataAnnotations;

namespace PuntoVenta.Blazor.Models;

public class UserModel
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public bool IsLocked { get; set; }
    public int FailedLoginAttempts { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;

    public string? LastModifiedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}

public class CreateUserModel
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(10, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 10 caracteres.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,10}$", ErrorMessage = "La contraseña debe tener al menos una letra mayúscula, una minúscula, un número y un carácter especial.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmar la contraseña es obligatorio.")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio.")]
    public int RoleId { get; set; }
}

public class UpdateUserModel
{
    public int UserId { get; set; }

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio.")]
    public int RoleId { get; set; }
}

public class RoleModel
{
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
