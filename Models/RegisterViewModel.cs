using System.ComponentModel.DataAnnotations;

namespace genial_dotnet_crm.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória")]
    [StringLength(100, ErrorMessage = "A senha deve ter no mínimo {2} caracteres.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirmar senha")]
    [Compare("Password", ErrorMessage = "As senhas não coincidem.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}




