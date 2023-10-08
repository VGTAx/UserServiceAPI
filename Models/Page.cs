using System.ComponentModel.DataAnnotations.Schema;

namespace UserServiceAPI.Models
{
  [NotMapped]
  public class Page
  {
    /// <summary>
    /// 
    /// </summary>
    public int CurrentPage { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int TotalPages { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int PageSize{ get; set; }

    public int CountOfRecords { get; set; }
    /// <summary>
    /// 
    /// </summary>    
    public bool HasPreviousPage => CurrentPage > 1;
    /// <summary>
    /// 
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;

    /// <summary>
    /// 
    /// </summary>
    public Page() { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    public Page(int count, int pageNumber, int pageSize)
    {
      CurrentPage = pageNumber;
      TotalPages = (int)Math.Ceiling(count / (double)pageSize);
      PageSize = pageSize;
      CountOfRecords = count;
    }
  }
}
