using System.ComponentModel.DataAnnotations;

namespace AICBank.Core.DTOs
{
    public class AccountUserDTO
    {
        public int Id { get; set; }
        public string Email { get; set; }
        [Display(Name = "Senha")]
        public string Password { get; set; }
        [Display(Name = "Confirme a Senha")]
        public string ConfirmPassword { get; set; }
        [Display(Name = "Telefone")]
        public string Phone { get; set; }
    }
}