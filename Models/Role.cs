using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UserServiceAPI.Models
{
  /// <summary>
  /// Represent a role
  /// </summary>
  public class Role : IComparable<Role>, IEquatable<Role?>
  { 
    /// <summary>
    /// Role Id
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Role name
    /// </summary>
    [Required]
    public string? RoleName { get; set; }

    /// <summary>
    /// Navigation property
    /// </summary>
    [JsonIgnore]
    public List<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public override bool Equals(object? obj)
    {
      return Equals(obj as Role);
    }

    public bool Equals(Role? other)
    {
      return other is not null &&
             RoleName == other.RoleName;
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(RoleName);
    }

    int IComparable<Role>.CompareTo(Role? other)
    {
      return Id.CompareTo(other?.Id);
    }    
  }
}
