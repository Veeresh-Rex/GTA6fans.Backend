using System.ComponentModel.DataAnnotations;
using GTA6fans.Domain.Enums;

namespace GTA6fans.Application.DTOs;

public class UpdateUserRequest
{
    [StringLength(50, MinimumLength = 3)]
    public string? DisplayName { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public UserStatus? Status { get; set; }
}
