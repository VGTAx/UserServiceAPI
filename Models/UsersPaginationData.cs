namespace UserServiceAPI.Models
{
  /// <summary>
  /// Class for passing user and pagination data.
  /// </summary>
  public class UsersPaginationData
  {
    /// <summary>
    /// Users data
    /// </summary>
    public List<User> Users { get; set; }

    /// <summary>
    /// Contains pagination information
    /// </summary>
    public PaginationInfo Pages { get; set; }

    public UsersPaginationData () { }

    /// <summary>
    /// GetUserAndPaginationData constructor
    /// </summary>
    /// <param name="users">Users list with data</param>
    /// <param name="pages">Object with pagination data</param>
    public UsersPaginationData(List<User> users, PaginationInfo pages)
    {
      Users = users;
      Pages = pages;
    }
  }
}
