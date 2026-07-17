using System.ComponentModel.DataAnnotations;

namespace LibrarySeatReservation.Web.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "用户名不能为空")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "用户名长度为 3-50 个字符")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "用户名只能包含字母、数字和下划线")]
    [Display(Name = "用户名")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "姓名不能为空")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "姓名长度为 1-50 个字符")]
    [Display(Name = "姓名")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码不能为空")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密码至少 6 位")]
    [DataType(DataType.Password)]
    [Display(Name = "密码")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "确认密码")]
    [Compare("Password", ErrorMessage = "两次密码输入不一致")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
