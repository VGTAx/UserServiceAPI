using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UserServiceAPI.DTOs
{
  /// <summary>
  /// DTO for filtering users. 
  /// </summary>
  public class UserFilterDto
  {
    /// <summary>
    /// Age filter initial value. If the value is zero, it is not used in the filter
    /// </summary>
    /// <example>25</example>
    [DefaultValue(0)]
    public int AgeFrom { get; set; }
    /// <summary>
    /// Age filter final value. If the value is zero, it is not used in the filter
    /// </summary>
    /// <example>52</example>
    [DefaultValue(0)]
    public int AgeTo { get; set; }
    /// <summary>
    /// Usermame for filter. Searches by partial match
    /// </summary>
    /// <example>Pavel Durov</example> 
    /// <value></value>
    [DefaultValue("")]
    public string? Name { get; set; } = String.Empty;
    /// <summary>
    /// Email for filter. Searches by partial match 
    /// </summary>
    /// <example>p.durov@telegram.com</example>
    [DefaultValue("")]
    public string? Email { get; set; } = String.Empty;
  }
}
