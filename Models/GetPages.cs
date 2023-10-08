namespace UserServiceAPI.Models
{
  /// <summary>
  /// 
  /// </summary>
  public class GetPages
  {
    public IEnumerable<User> Users { get; set; }
    public Page Pages { get; set; }

    public GetPages () { }

    public GetPages(IEnumerable<User> users, Page pages)
    {
      Users = users;
      Pages = pages;
    }
  }
}
