namespace UserServiceAPI.DTOs
{
  /// <summary>
  /// 
  /// </summary>
  public class UserFilterDto
  {
    /// <summary>
    /// 
    /// </summary>
    public int AgeFrom { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int AgeTo { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? Name { get; set; } = "";
    /// <summary>
    /// 
    /// </summary>
    public string? Email { get; set; } = "";
  }
}
