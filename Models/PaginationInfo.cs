using System.ComponentModel.DataAnnotations.Schema;

namespace UserServiceAPI.Models
{
  /// <summary>
  /// Class represents pagination information
  /// </summary>
  [NotMapped]  
  public class PaginationInfo
  {
    /// <summary>
    /// Current page in pagination
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Total pages in pagination
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Count entries on a page
    /// </summary>
    public int PageSize{ get; set; }

    /// <summary>
    /// Total number of entries
    /// </summary>
    public int CountOfRecords { get; set; }

    /// <summary>
    /// Presence of previous page in pagination
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;

    /// <summary>
    /// 
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;

    /// <summary>
    /// Presence of next page in pagination
    /// </summary>
    public PaginationInfo() { }

    /// <summary>
    /// PaginationInfo constructor
    /// </summary>
    /// <param name="count">Count entries</param>
    /// <param name="pageNumber">Current page in pagination</param>
    /// <param name="pageSize">Count entries on a page</param>
    public PaginationInfo(int count, int pageNumber, int pageSize)
    {
      CurrentPage = pageNumber;
      TotalPages = (int)Math.Ceiling(count / (double)pageSize);
      PageSize = pageSize;
      CountOfRecords = count;
    }
  }
}
