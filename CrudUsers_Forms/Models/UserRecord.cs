using System;

namespace CrudUsers_Forms.Models;

[Serializable]
public class UserRecord
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public UserRole Role { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }

    public string RoleDescription => Role switch
    {
        UserRole.SuperAdmin => "Super Administrador",
        UserRole.Admin => "Administrador",
        _ => "Usuario"
    };
}
