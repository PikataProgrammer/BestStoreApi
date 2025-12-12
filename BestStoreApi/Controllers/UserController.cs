using BestStoreApi.Dtos;
using BestStoreApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreApi.Controllers;

[Authorize(Roles = "admin")]
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetUsers(int? page)
    {
        if (page == null || page < 1)
        {
            page = 1;
        }

        int pageSize = 5;
        int totalPages = 0;
        
        decimal count = _context.Users.Count();
        
        totalPages = (int)Math.Ceiling(count / pageSize);
        
        
        var users = _context.Users.OrderByDescending(x => x.Id)
            .Skip((int)(page.Value - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        List<UserProfileDto> userProfiles = new List<UserProfileDto>();

        foreach (var user in users)
        {
            var userProfileDto = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone =  user.Phone,
                Address = user.Address,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
            userProfiles.Add(userProfileDto);
        }

        var response = new
        {
            Users = userProfiles,
            TotalPages = totalPages,
            PageSize = pageSize,
            Page = page,
        };
        
        return Ok(response);
    }

    [HttpGet("{id}")]
    public IActionResult GetUserId(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null)
        {
            return NotFound();
        }
        var userProfileDto = new UserProfileDto()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone =  user.Phone,
            Address = user.Address,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };

        return Ok(userProfileDto);        
    }
}