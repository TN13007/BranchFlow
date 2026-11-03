using System.ComponentModel.DataAnnotations;

namespace BranchFlow.Web.Models.Auth;

public sealed class LoginFormModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
    [StringLength(100, ErrorMessage = "Tên đăng nhập không quá 100 ký tự.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    public string Password { get; set; } = string.Empty;
}
