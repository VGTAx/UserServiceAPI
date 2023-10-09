using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserServiceAPI.Data;
using UserServiceAPI.DTOs;
using UserServiceAPI.Models;

namespace UserServiceAPI.Controller
{
  /// <summary>
  /// Controller for managing user-related operations.
  /// </summary>
  [Route("api/[controller]")]
  [ApiController]
  public class UserServiceController : ControllerBase
  {
    private readonly UserServiceContext _context;
    private readonly ILogger<UserServiceController> _logger;
    private readonly IMapper _mapper;
    private readonly List<string> userAccessLevelList = new List<string>
      {
        "SuperAdmin",
        "Admin",
        "Support",
        "User"
      };

    /// <summary>
    /// UserServiceController constructor
    /// </summary>
    /// <param name="context">DB context</param>
    /// <param name="logger">Logger for recording logs.</param>
    /// <param name="mapper">Mapper for data transformation.</param>
    public UserServiceController(
        UserServiceContext context,
        ILogger<UserServiceController> logger,
        IMapper mapper)
    {
      _context = context;
      _logger = logger;
      _mapper = mapper;
    }
    /// <summary>
    /// Method returns users and object to create pagination
    /// </summary>
    /// <param name="userFilterDto">DTO with filters for searching users.</param>
    /// <param name="userRoleName" example="User">Role name to filter users by role.
    /// Roles: User, Support, Admin, SuperAdmin
    /// </param>
    /// <param name="page" example="1">Page in pagination.</param>
    /// <param name="pageSize" example="5">Number of entries per page</param>
    /// <param name="sortOrder" example="NameAsc">
    /// <br> Sort order of results: ASC or DESC (Ignore Case) </br>
    /// <br>Types Sort order: NameAsc/NameDesc, AgeAsc/AgeDesc, EmailAsc/EmailDesc, RoleAsc/RoleDesc</br>
    /// <br>Users are sorted by role based on the access level of the user role. Example: user has roles [User, Admin, SuperAdmin]. When sorting users,
    ///   the SuperAdmin role will be used, because its has the highest access level from the list</br> 
    /// <br><u>Access level list(desc): SuperAdmin - Admin - Support - User</u></br>
    /// </param>
    /// <response code="200">Users were got</response> 
    /// <response code="400">
    /// <ul>
    ///   <li>Page number in pagination must be 1 or greater</li>
    ///   <li>Page size in pagination must be 1 or greater</li>
    /// </ul>
    /// </response>
    /// <response code="404">
    /// <ul>
    ///   <li>No entries found matching the filter</li>
    ///   <li>No users found</li>
    ///   <li>Page was not found in pagination. Exceeding the pagination boundary, moving to a non-existent page.</li>
    /// </ul>
    /// </response> 
    /// <response code="500">Internal server error.</response> 
    /// <returns>Returns an HTTP status code indicating the result of the get users method.</returns>
    [HttpGet("GetUsers", Name = "GetUsers")]
    public async Task<IActionResult> GetUsers([FromQuery] UserFilterDto userFilterDto, string userRoleName = "", int page = 1, int pageSize = 10, string sortOrder = "IdAsc")
    {
      _logger.LogInformation($"Entering {nameof(GetUsers)} method");

      try
      {
        var users = await _context.Users
          .Select(u => new User
          {
            Id = u.Id,
            Age = u.Age,
            Name = u.Name,
            Email = u.Email,
            Roles = u.UserRoles
                      .Select(ur => ur.Role)
                      .ToList()!
          })
          .ToListAsync();

        if (!users.Any())
        {
          _logger.LogWarning("No users found");
          _logger.LogInformation($"Exiting {nameof(GetUsers)} method");
          return NotFound("No users found.");
        }

        if (page < 1)
        {
          _logger.LogWarning("Page number must be 1 or greater ");
          _logger.LogInformation($"Exiting {nameof(GetUsers)} method");
          return BadRequest("Page number must be 1 or greater");
        }

        if (pageSize < 1)
        {
          _logger.LogWarning("Page size must be 1 or greater");
          _logger.LogInformation($"Exiting {nameof(GetUsers)} method");
          return BadRequest("Page size must be 1 or greater");
        }

        var returnUsers = FilterEntries(users, userFilterDto, userRoleName);
        var count = returnUsers.Count;

        if (!returnUsers.Any())
        {
          _logger.LogWarning("No entries found matching the filter");
          _logger.LogInformation($"Exiting {nameof(GetUsers)} method");
          return NotFound("No entries found matching the filter");
        }

        returnUsers = SortEntries(returnUsers, sortOrder);

        returnUsers = returnUsers.Skip((page - 1) * pageSize)
                  .Take(pageSize)
                  .ToList();

        var pages = new PaginationInfo(count, page, pageSize);

        if (pages.TotalPages < page)
        {
          _logger.LogWarning("Page was not found");
          _logger.LogInformation($"Exiting {nameof(GetUsers)} method");
          return NotFound("Page was not found");
        }

        var getUserPages = new UsersPaginationData(returnUsers, pages);

        _logger.LogInformation("Users were got. Users count: {count}", returnUsers.Count);
        _logger.LogInformation($"Exiting {nameof(GetUsers)} method");
        return Ok(getUserPages);
      }
      catch (Exception)
      {
        return StatusCode(500, "Internal server error.");
      }
    }

    /// <summary>
    /// Method returns a user by ID
    /// </summary>
    /// <param name="userId" example="1">User ID to get user</param>
    /// <response code="200">User was got.</response>
    /// <response code="404">User was not found.</response>
    /// <response code="500">Internal server error</response>
    /// <returns>Returns an HTTP status code indicating the result of the get user method.</returns>    
    [HttpGet("GetUser/{userId}", Name = "GetUser")]
    public IActionResult GetUser(int userId)
    {
      _logger.LogInformation($"Entering {nameof(GetUser)} method");

      try
      {
        if (!IsUserExist(userId))
        {
          _logger.LogWarning("User with ID: {userId} not found", userId);
          _logger.LogInformation($"Exiting {nameof(GetUser)} method");
          return NotFound($"User with ID: {userId} not found");
        }
        var user = _context.Users
              .Where(u => u.Id == userId)
              .Select(u => new User
              {
                Id = u.Id,
                Name = u.Name,
                Age = u.Age,
                Email = u.Email,
                Roles = u.UserRoles
                        .Where(u => u.UserId == userId)
                        .Select(u => u.Role)
                        .ToList()!
              });

        _logger.LogInformation("Return user with roles. UserID: {userId}", userId);
        _logger.LogInformation($"Exiting {nameof(GetUser)} method");
        return Ok(user);
      }
      catch (Exception)
      {
        return StatusCode(500, "Internal server error.");
      }
    }

    /// <summary>
    /// Method creates new user without roles
    /// </summary>
    /// <param name="userCreateDto" example='{"name":"Jeff Bezos", "age":59, "email":"j.bezos@amazon.com"}'>
    /// DTO with new user data.</param>
    /// <response code="201">User was сreated.</response>
    /// <response code="400">
    /// <uL>
    ///   <li>Email has already taken.</li>
    ///   <li>Age is less or equal 0.</li>
    ///   <li>Email/Age/Name field is required.</li>
    ///   <li>The Email field is not a valid e-mail address.</li>
    /// </uL>    
    /// </response>
    /// <response code="500">Internal server error</response>
    /// <returns>Returns an HTTP status code indicating the result of the create user method.</returns>
    [HttpPost("CreateUser", Name = "CreateUser")]
    public async Task<IActionResult> CreateUser([FromBody] UserCreateDto userCreateDto)
    {
      _logger.LogInformation($"Entering {nameof(CreateUser)} method*/");

      try
      {
        if (IsEmailUnique(userCreateDto.Email!))
        {
          _logger.LogWarning("User was not created. {Email} has already taken.", userCreateDto.Email);
          _logger.LogInformation($"Exiting {nameof(CreateUser)} method");
          return BadRequest($"{userCreateDto.Email} has already taken");
        }

        if (!IsAgePositiveNumber(userCreateDto.Age))
        {
          _logger.LogWarning("User was not created. Age must be than 0.");
          _logger.LogInformation($"Exiting {nameof(CreateUser)} method");
          return BadRequest($"Age must be than 0");
        }

        var user = _mapper.Map<User>(userCreateDto);

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User was created");
        _logger.LogInformation($"Exiting {nameof(CreateUser)} method");
        return CreatedAtRoute($"GetUser", new { userId = user.Id }, userCreateDto);
      }
      catch (Exception)
      {
        return StatusCode(500, "Internal server error.");
      }

    }

    /// <summary>
    /// Method edited info user
    /// </summary>
    /// <response code="200">User was edited</response>
    /// <response code="400">
    /// <ul>
    ///   <li>Email has already taken</li>
    ///   <li>Age must be than 0</li>
    ///   <li>The Name/Age/Email field is required.</li>
    ///   <li>The Email field is not a valid e-mail address.</li>
    /// </ul>
    /// </response>
    /// <response code="404">User was not found</response>
    /// <response code="500">Internal server error</response>   
    /// <param name="userEditDto" example='{"id": 10, "name":"Jeff Bezos", "age":59, "email":"j.bezos@amazon.com"}'>
    /// DTO with updated user data.</param>
    /// <returns>Returns an HTTP status code indicating the result of the edit user method.</returns>
    [HttpPost("EditUser", Name = "EditUser")]
    public async Task<IActionResult> EditUser([FromBody] UserEditDto userEditDto)
    {
      _logger.LogInformation($"Entering {nameof(EditUser)} method");

      try
      {
        if (!IsUserExist(userEditDto.Id))
        {
          _logger.LogWarning("User was not edited. User with ID: {userId} not found", userEditDto.Id);
          _logger.LogInformation($"Exiting {nameof(EditUser)} method");
          return NotFound($"User with ID: {userEditDto.Id} not found");
        }

        // get user from DB
        var user = _context.Users
                      .AsNoTracking()
                      .FirstOrDefault(u => u.Id == userEditDto.Id);

        // compare the user's email address from the database and the user's email address from the request.
        // IF it is not equal, then the user's email address from the query is checked for uniqueness.
        if (!string.Equals(user!.Email, userEditDto.Email, StringComparison.OrdinalIgnoreCase)
              && IsEmailUnique(userEditDto.Email!))
        {
          _logger.LogWarning("User was not edited. {Email} has already taken.", userEditDto.Email);
          _logger.LogInformation($"Exiting {nameof(EditUser)} method");
          return BadRequest($"{userEditDto.Email} has already taken");
        }

        if (!IsAgePositiveNumber(userEditDto.Age))
        {
          _logger.LogWarning("User was not updated. Age must be than 0.");
          _logger.LogInformation($"Exiting {nameof(EditUser)} method");
          return BadRequest($"Age must be than 0");
        }

        var editUser = _mapper.Map<User>(userEditDto);
        _context.Update(editUser);
        await _context.SaveChangesAsync();
        return Ok("User was edited");
      }
      catch (Exception)
      {
        return StatusCode(500, "Internal server error");
      }
    }

    /// <summary>
    /// Method changes user roles 
    /// </summary>
    /// <param name="userRoleNames" example='["User", "Admin", "SuperAdmin"]'>List of role names.
    /// <br>Roles: User, Support, Admin, SuperAdmin</br>
    /// </param>
    /// <param name="userId" example="1">User ID</param>
    /// <response code="200">Role(s) was(were) added.</response>
    /// <response code="400"> 
    /// <uL>
    ///   <li>No roles were provided for addition.</li>      
    ///   <li>Role has not existed</li>   
    /// </uL>    
    /// </response>  
    /// <response code="404">
    /// <ul>
    ///   <li>User was not found</li>      
    /// </ul>    
    /// </response>
    /// <response code="500">Internal server error</response>
    /// <returns>Returns an HTTP status code indicating the result of the user change role(s) method.</returns>
    [HttpPost("ChangedUserRoles", Name = "ChangedUserRoles")]
    public async Task<IActionResult> ChangeUserRoles([FromBody] List<string> userRoleNames, int userId)
    {
      _logger.LogInformation($"Entering {nameof(ChangeUserRoles)} method");

      try
      {
        if (!IsUserExist(userId))
        {
          _logger.LogInformation("User with ID: {userId} not found", userId);
          _logger.LogInformation($"Exiting {nameof(ChangeUserRoles)} method");
          return NotFound($"User with ID: {userId} not found");
        }

        if (userRoleNames.Count == 0)
        {
          _logger.LogWarning("No roles were provided for addition");
          _logger.LogInformation($"Exiting {nameof(ChangeUserRoles)} method");
          return BadRequest($"No roles were provided for addition.");
        }

        // searches for role IDs to add
        var rolesNamesToAdd =
          userRoleNames
          .Except(GetCurrentUserRoles(userId))
          .ToList();

        // add roles to User
        foreach (var roleName in rolesNamesToAdd)
        {
          // check role for existence in DataBase
          if (!IsRoleExist(roleName))
          {
            _logger.LogWarning("Role with Name: {roleName} has not existed", roleName);
            _logger.LogInformation($"Exiting {nameof(ChangeUserRoles)} method");
            return BadRequest($"Role with Name: {roleName} has not existed");
          }
          // add role to User
          await _context.UserRoles
            .AddAsync(new UserRole
            {
              RoleId = _context.Roles
                          .Where(r => r.RoleName == roleName)
                          .Select(r => r.Id)
                          .FirstOrDefault(),
              UserId = userId
            });
        }

        // searches for role IDs to remove. 
        var roleNamesToRemove =
          GetCurrentUserRoles(userId)
          .Except(userRoleNames)
          .ToList();

        // searches for all UserRoles using "roleIDsToRemove"
        var userRolesToRemove =
          _context.UserRoles
          .Where(ur => roleNamesToRemove.Contains(ur.Role.RoleName) && ur.UserId == userId)
          .ToList();

        // Remove UserRoles
        _context.UserRoles.RemoveRange(userRolesToRemove);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Role(s) was(were) changed");
        _logger.LogInformation($"Exiting {nameof(ChangeUserRoles)} method");
        return Ok($"Role(s) was(were) changed");
      }
      catch (Exception)
      {
        return StatusCode(500, "Internal server error");
      }
    }

    /// <summary>
    /// Method deletes a user by ID.
    /// </summary>
    /// <param name="userId" example="1">The ID of the user to be deleted.</param>
    /// <response code="204">User was deleted</response>
    /// <response code="404">User was not found.</response>
    /// <response code="500">Internal server error</response>
    /// <returns>Returns an HTTP status code indicating the result of the delete user method.</returns>
    [HttpDelete("DeleteUser/{userId}", Name = "DeleteUser")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
      _logger.LogInformation($"Entering {nameof(DeleteUser)} method");

      try
      {
        if (!IsUserExist(userId))
        {
          _logger.LogWarning("User with ID: {userId} not found", userId);
          _logger.LogInformation($"Exiting {nameof(DeleteUser)} method");
          return NotFound($"User with ID: {userId} not found");
        }

        var user =
          _context.Users
            .Where(u => u.Id == userId)
            .FirstOrDefault()!;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User with ID: {userId} was deleted", userId);
        _logger.LogInformation($"Exiting {nameof(DeleteUser)} method");
        return NoContent();
      }
      catch (Exception)
      {
        return StatusCode(500, "Internal server error");
      }
    }

    /// <summary>
    /// Method checks the uniqueness of the Email
    /// </summary>
    /// <param name="email" example="user@example.com">Uniqueness check email</param>
    /// <returns>Result checking the uniqueness Email. True if email is unique, else false</returns>
    private bool IsEmailUnique(string email)
    {
      return _context.Users.Any(u => u.Email == email);
    }

    /// <summary>
    /// Method checks the existence of a role.
    /// </summary>
    /// <param name="roleName" example="2">Role ID</param>
    /// <returns>Result checking the existence of a role. True if role exists, else false</returns>
    private bool IsRoleExist(string roleName)
    {
      return _context.Roles.Any(u => u.RoleName == roleName);
    }

    /// <summary>
    /// Method return current user role IDs.
    /// </summary>    
    /// <param name="userId" exmaple="1">User ID</param>
    /// <returns>List of user role IDs.</returns>
    private List<string> GetCurrentUserRoles(int userId)
    {
      var useRoles = _context.UserRoles
                        .AsNoTracking()
                        .Where(ur => ur.UserId == userId)
                        .Select(ur => ur.Role!.RoleName)
                        .ToList();
      return useRoles!;
    }

    /// <summary>
    /// Method checks the existence of a user by his ID.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Result checking the existence of a user. True if user exists, else false</returns>
    private bool IsUserExist(int userId)
    {
      return _context.Users.Any(u => u.Id == userId);
    }

    /// <summary>
    /// Method сhecks if age is a positive number.
    /// </summary>
    /// <param name="userAge">User age</param>
    /// <returns>Result checking if age is a positive number. True if age is a positive number, else false</returns>
    private bool IsAgePositiveNumber(int userAge)
    {
      return userAge > 0;
    }

    /// <summary>
    /// Filters the list of users
    /// </summary>
    /// <param name="items"></param>
    /// <param name="userFilterDto"></param>
    /// <param name="userRoleName"></param>
    /// <returns></returns>
    private List<User> FilterEntries(List<User> items, UserFilterDto userFilterDto, string userRoleName)
    {
      if (!String.IsNullOrEmpty(userRoleName))
      {
        items = items.Where(u =>
                u.Roles.Exists(r => r.RoleName == userRoleName))
                .ToList();
      }

      if (!String.IsNullOrEmpty(userFilterDto.Name))
      {
        items = items.Where(u =>
                  u.Name!.Contains(userFilterDto.Name))
                  .ToList();
      }

      if (!String.IsNullOrEmpty(userFilterDto.Email))
      {
        items = items.Where(u =>
                  u.Email!.Contains(userFilterDto.Email))
                  .ToList();
      }

      if (userFilterDto.AgeFrom != 0 && userFilterDto.AgeTo != 0)
      {
        items = items
                .Where(u => u.Age >= userFilterDto.AgeFrom && u.Age <= userFilterDto.AgeTo)
                .ToList();
      }
      else if (userFilterDto.AgeFrom != 0)
      {
        items = items
                 .Where(u => u.Age >= userFilterDto.AgeFrom)
                 .ToList();
      }
      else if (userFilterDto.AgeTo != 0)
      {
        items = items.Where(u => u.Age <= userFilterDto.AgeTo)
                .ToList();
      }

      return items;
    }

    /// <summary>
    /// Sorts the list of users
    /// </summary>
    /// <param name="items">List users</param>
    /// <param name="sortOrder">Sort order</param>
    /// <returns>Sorted list of users</returns>
    private List<User> SortEntries(List<User> items, string sortOrder = "IdAsc")
    {
      // sorts roles by access level in list user
      foreach (var item in items)
      {
        item.Roles = item.Roles.OrderBy(i =>
        {
          var roleIndex = userAccessLevelList.IndexOf(i.RoleName!);
          return roleIndex != -1 ? roleIndex : int.MaxValue;
        }).ToList();
      }
      // Users are sorted by role based on the access level of the user role.
      // Example: user has roles [User, Admin, SuperAdmin]. When sorting a user,
      // the SuperAdmin role will be used, because its has the highest access level from the list
      return sortOrder.ToLower() switch
      {
        "nameasc" => items.OrderBy(u => u.Name)
                    .ToList(),
        "namedesc" => items.OrderByDescending(u => u.Name)
                    .ToList(),
        "ageasc" => items.OrderBy(u => u.Age)
                    .ToList(),
        "agedesc" => items.OrderByDescending(u => u.Age)
                    .ToList(),
        "emailasc" => items.OrderBy(u => u.Email)
                    .ToList(),
        "emaildesc" => items.OrderByDescending(u => u.Email)
                    .ToList(),
        "roleasc" => items.OrderBy(u => u.Roles.FirstOrDefault())
                    .ToList(),
        "roledesc" => items.OrderByDescending(u => u.Roles.FirstOrDefault())
                    .ToList(),
        _ => items.OrderBy(u => u.Id)
                    .ToList(),
      };
    }
  }
}