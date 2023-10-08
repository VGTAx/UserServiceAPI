using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace UserServiceAPI.Models
{
  /// <summary>
  /// Represents a user with basic information.
  /// </summary>
  public class User
  {   
    /// <summary>
    /// User Id
    /// </summary> 
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
    /// <summary>
    /// User email
    /// </summary> 
    public List<Role> Roles { get; set; } = new List<Role>();
    /// <summary>
    /// Navigation property
    /// </summary>
    [JsonIgnore]
    public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
  }
}
