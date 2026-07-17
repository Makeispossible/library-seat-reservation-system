using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySeatReservation.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "用户名不能为空")]
    [Display(Name = "用户名")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码不能为空")]
    [DataType(DataType.Password)]
    [Display(Name = "密码")]
    public string Password { get; set; } = string.Empty;

    [HiddenInput]
    public string? ReturnUrl { get; set; }
}
