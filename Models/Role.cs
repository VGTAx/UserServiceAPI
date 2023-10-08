﻿using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UserServiceAPI.Models
{
  /// <summary>
  /// Represent a role
  /// </summary>
  public class Role
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
  }
}