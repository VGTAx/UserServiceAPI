namespace UserServiceAPI.Models
{
  /// <summary>
  /// Model of communication between <see cref="Models.User"></see> and <see cref="Models.Role"></see> (Many-to-Many)
  /// </summary>
  public class UserRole
  {
    /// <summary>
    /// <see cref="UserId">User ID</see>
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Role ID
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// Navigation property
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Navigation property
    /// </summary>
    public Role? Role { get; set; }
  }
}
