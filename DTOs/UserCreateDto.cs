using System.ComponentModel.DataAnnotations;

namespace UserServiceAPI.DTOs
{
  /// <summary>
  /// DTO to create a new user.
  /// </summary>
  public class UserCreateDto
  {
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
    /// <summary>
    /// User email
    /// </summary> 
  }
}
