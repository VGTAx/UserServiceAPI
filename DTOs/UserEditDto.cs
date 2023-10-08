using Swashbuckle.AspNetCore.Annotations;
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
    public int Id { get; set; }
    /// <summary>
    /// User name
    /// </summary>
    [Required]
    public string? Name { get; set; }
    /// <summary>
    /// User age
    /// </summary>
    [Required]
    public int Age { get; set; }
    /// <summary>
    /// User email
    /// </summary>
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    
  }
}
