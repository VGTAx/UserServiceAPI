using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UserServiceAPI.DTOs
{
  /// <summary>
  /// DTO to edit a user
  /// </summary>
  public class UserEditDto
  {
    /// <summary>
    /// User ID
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }
    /// <summary>
    /// Username
    /// </summary>
    /// <example>User</example>
    [Required]
    public string? Name { get; set; }
    /// <summary>
    /// User age
    /// </summary>
    /// <example>25</example>
    [Required]
    public int Age { get; set; }
    /// <summary>
    /// User email
    /// </summary>
    /// <example>email@example.com</example>
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    
  }
}
