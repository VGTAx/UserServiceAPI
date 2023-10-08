using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Text.RegularExpressions;
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
    /// 
    /// </summary>
    /// <param name="userFilterDto"></param>
    /// <param name="userRoleName"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <param name="sortOrder"></param>
    /// <returns></returns>
    [HttpGet("GetUsers",Name = "GetUsers")]
    public async Task<IActionResult> GetUsers([FromQuery]UserFilterDto userFilterDto, string userRoleName = "", int page = 1, int pageSize = 10, string sortOrder = "IdAsc")
    {
      _logger.LogInformation("Call GetUsers Method {one}, {second}", 1, 3);
      var users = _context.Users
          .Select(u => new User
          {
            Id = u.Id,
            Age = u.Age,
            Name = u.Name,
            Email = u.Email,
            Roles = u.UserRoles
                      .Select(ur => ur.Role)
                      .ToList()!
          });

      if(page < 1) {
        page = 1;
      }
      

      var items = FilterEntries(users, userFilterDto, userRoleName);      
      var count = items.Count();

      items = SortEntries(users, sortOrder);

      if (items.Count() == 0)
      {
        return NotFound("no entries found");
      }

      items = items.Skip((page - 1) * pageSize).Take(pageSize);

      if (items.Count() == 0)
      {
        return NotFound("no entries found");
      }

      var pages = new Page(count, page, pageSize);
      
      if (pages.TotalPages < page)
      {
        return BadRequest("Page was not found");
      }

      var getUserPages = new GetPages(items, pages);

      return Ok(getUserPages);
    }

    /// <summary>
    /// Method returns a user by ID
    /// </summary>
    /// <param name="userId" example="1">User ID to get it</param>
    /// <response code="200">User was got.</response>
    /// <response code="404">User was not found.</response>
    /// <response code="500">Internal server error</response>
    /// <returns>User with roles</returns>    
    [HttpGet("GetUser/{userId}", Name = "GetUser")]
    public IActionResult GetUser(int userId)
    {
      _logger.LogInformation($"Entering {nameof(GetUser)} method");
      try
      {
        if (!IsUserExist(userId))
        {
          _logger.LogInformation("User with ID: {userId} not found", userId);
          _logger.LogInformation($"Exiting {nameof(GetUser)} method\n");
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
        _logger.LogInformation($"Exiting {nameof(GetUser)} method\n");
        return Ok(user);
      }
      catch (Exception)
      {
        return StatusCode(500, "Internal server error.");
      }
    }

    /// <summary>
    /// Method creates new user
    /// </summary>
    /// <param name="userCreateDto" example='{"name":"Jeff Bezos", "age":59, "email":"j.bezos@amazon.com"}'>
    /// DTO with new user data.</param>
    /// <response code="201">User was сreated.</response>
    /// <response code="400">User was not created:
    /// <uL>
    ///   <li>Email has already taken.</li>
    ///   <li>Age is less or equal 0.</li>
    ///   <li>Email/Age/Name field is required.</li>
    ///   <li>The Email field is not a valid e-mail address.</li>
    /// </uL>    
    /// </response>
    /// <response code="500">Internal server error</response>
    /// <returns>Returns an HTTP status code indicating the result of the create user operation.</returns>
    [HttpPost("CreateUser", Name = "CreateUser")]
    public async Task<IActionResult> CreateUser([FromBody] UserCreateDto userCreateDto)
    {
      _logger.LogInformation($"Entering {nameof(CreateUser)} method");
      try
      {
        if (IsEmailUnique(userCreateDto.Email!))
        {
          _logger.LogInformation("User was not created. {Email} has already taken.", userCreateDto.Email);
          _logger.LogInformation($"Exiting {nameof(CreateUser)} method\n");
          return BadRequest($"{userCreateDto.Email} has already taken");
        }

        if (!IsAgePositiveNumber(userCreateDto.Age))
        {
          _logger.LogInformation("User was not created. Age must be than 0.");
          _logger.LogInformation($"Exiting {nameof(CreateUser)} method\n");
          return BadRequest($"Age must be than 0");
        }

        var user = _mapper.Map<User>(userCreateDto);

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User has created");
        _logger.LogInformation($"Exiting {nameof(CreateUser)} method\n");
        return CreatedAtRoute($"GetUser", new { userId = user.Id }, userCreateDto);
      }
      catch (DbException)
      {
        return StatusCode(500, "Internal server error.");
      }

    }

    /// <summary>
    /// Method edited info user
    /// </summary>
    /// <responce code="200">User was edited</responce>
    /// <response code="400">User was not edited:
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
    /// <returns>Returns an HTTP status code indicating the result of the user edit user operation.</returns>
    [HttpPost("EditUser", Name = "EditUser")]
    public async Task<IActionResult> EditUser([FromBody] UserEditDto userEditDto)
    {
      try
      {
        if (!IsUserExist(userEditDto.Id))
        {
          _logger.LogInformation("User was not edited. User with ID: {userId} not found", userEditDto.Id);
          _logger.LogInformation($"Exiting {nameof(EditUser)} method\n");
          return NotFound($"User with ID: {userEditDto.Id} not found");
        }

        // get user from DB
        var user = _context.Users
                      .AsNoTracking()
                      .FirstOrDefault(u=> u.Id == userEditDto.Id);

        // compare the user's email address from the database and the user's email address from the request.
        // IF it is not equal, then the user's email address from the query is checked for uniqueness.
        if (!string.Equals(user!.Email, userEditDto.Email, StringComparison.OrdinalIgnoreCase) 
              && IsEmailUnique(userEditDto.Email!))
        {
          _logger.LogInformation("User was not edited. {Email} has already taken.", userEditDto.Email);
          _logger.LogInformation($"Exiting {nameof(EditUser)} method\n");
          return BadRequest($"{userEditDto.Email} has already taken");
        }

        if (!IsAgePositiveNumber(userEditDto.Age))
        {
          _logger.LogInformation("User was not updated. Age must be than 0.");
          _logger.LogInformation($"Exiting {nameof(EditUser)} method\n");
          return BadRequest($"Age must be than 0");
        }

        var editUser = _mapper.Map<User>(userEditDto);
        _context.Update(editUser);
        await _context.SaveChangesAsync();
        return Ok("User was edited");
      }
      catch (DbException)
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
    /// <returns>Returns an HTTP status code indicating the result of the delete user operation.</returns>
    [HttpDelete("DeleteUser/{userId}", Name = "DeleteUser")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
      _logger.LogInformation($"Entering {nameof(DeleteUser)} method");
      try
      {
        if (!IsUserExist(userId))
        {
          _logger.LogInformation("User with ID: {userId} not found", userId);
          _logger.LogInformation($"Exiting {nameof(DeleteUser)} method\n");
          return NotFound($"User with ID: {userId} not found");
        }

        var user =
          _context.Users
            .Where(u => u.Id == userId)
            .FirstOrDefault()!;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User with ID: {userId} was deleted", userId);
        _logger.LogInformation($"Exiting {nameof(DeleteUser)} method\n");
        return NoContent();
      }
      catch (DbException)
      {
        return StatusCode(500, "Internal server error");
      }
    }

    /// <summary>
    /// Method changes user roles 
    /// </summary>
    /// <param name="userRoleNames" example='["User", "Admin", "Editor"]'>List of role names which user must have</param>
    /// <param name="userId" example="1">User ID</param>
    /// <response code="200">Role(s) was(were) added.</response>
    /// <response code="400">Role(s) was(were) not added.
    /// <response code="500">Internal server error</response>
    /// <uL>
    ///   <li>No roles were provided for addition.</li>      
    ///   <li>Role was not found</li>   
    /// </uL>    
    /// </response>  
    /// <response code="404">Role(s) was(were) not added.
    /// <ul>
    ///   <li>User was not found</li>      
    /// </ul>    
    /// </response>
    /// <returns>Returns an HTTP status code indicating the result of the user change role(s) operation.</returns>
    [HttpPost("ChangedUserRoles", Name = "ChangedUserRoles")]
    public async Task<IActionResult> ChangedUserRoles([FromBody] List<string> userRoleNames, int userId)
    {
      _logger.LogInformation($"Entering {nameof(ChangedUserRoles)} method");
      try
      {
        if (!IsUserExist(userId))
        {
          _logger.LogInformation("User with ID: {userId} not found", userId);
          _logger.LogInformation($"Exiting {nameof(ChangedUserRoles)} method\n");
          return NotFound($"User with ID: {userId} not found");
        }

        if (userRoleNames.Count == 0)
        {
          _logger.LogInformation("No roles were provided for addition");
          _logger.LogInformation($"Exiting {nameof(ChangedUserRoles)} method\n");
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
            _logger.LogInformation("Role with Name: {roleName} has not existed", roleName);
            _logger.LogInformation($"Exiting {nameof(ChangedUserRoles)} method\n");
            return BadRequest($"Role with Name: {roleName} has not existed");
          }
          // add role to User
          await _context.UserRoles
            .AddAsync(new UserRole
            {
              RoleId = _context.Roles
                          .Where(r => r.RoleName == roleName)
                          .Select(r=>r.Id)
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
        _logger.LogInformation($"Exiting {nameof(ChangedUserRoles)} method\n");
        return Ok($"Role(s) was(were) changed");
      }
      catch (DbException)
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
      return useRoles;
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

    private IEnumerable<User> FilterEntries(IEnumerable<User> items, UserFilterDto userFilterDto, string userRoleName)
    {
      if (!String.IsNullOrEmpty(userRoleName))
      {
        items = items.Where(u => 
              u.Roles.Exists(r => r.RoleName == userRoleName));
      }

      if (!String.IsNullOrEmpty(userFilterDto.Name))
      {
        items = items.Where(u =>
                  u.Name!.Contains(userFilterDto.Name));
      }

      if (!String.IsNullOrEmpty(userFilterDto.Email))
      {
        items = items.Where(u =>
                  u.Email!.Contains(userFilterDto.Email));
      }

      if (userFilterDto.AgeFrom != 0 && userFilterDto.AgeTo != 0)
      {
        items = items.Where(u => u.Age >= userFilterDto.AgeFrom && u.Age <= userFilterDto.AgeTo);
      }
      else if (userFilterDto.AgeFrom != 0)
      {
        items = items.Where(u => u.Age >= userFilterDto.AgeFrom);
      }
      else if (userFilterDto.AgeTo != 0)
      {
        items = items.Where(u => u.Age <= userFilterDto.AgeTo);
      }

      return items;
    }

    private IEnumerable<User> SortEntries(IEnumerable<User> items, string sortOrder = "IdAsc")
    {
      var rolePriority = new List<string> { "SuperAdmin", "Admin", "User" };

      switch (sortOrder)
      {
        case "NameAsc":
          return items.OrderBy(u => u.Name);
        case "NameDesc":
          return items.OrderByDescending(u => u.Name);
        case "AgeAsc":
          return items.OrderBy(u => u.Age);
        case "AgeDesc":
          return items.OrderByDescending(u => u.Age);
        case "EmailAsc":
          return items.OrderBy(u => u.Email);
        case "EmailDesc":
          return items.OrderByDescending(u => u.Email);
        case "RoleAsc":
          return items.OrderBy(u => u.Roles.OrderBy(r => r.RoleName)).ThenBy(u => u.Name);
        case "RoleDesc":
          return items.OrderByDescending(u => u.Roles.OrderByDescending(r => r.RoleName).Select(r => r.RoleName).FirstOrDefault());
        default:
          return items.OrderBy(u => u.Id);
      }
    }

    
  }
}
