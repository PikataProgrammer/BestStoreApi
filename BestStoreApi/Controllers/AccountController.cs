using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BestStoreApi.Dtos;
using BestStoreApi.Models;
using BestStoreApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BestStoreApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    public AccountController(IConfiguration configuration, ApplicationDbContext context)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("Register")]
    public IActionResult Register(UserDto userDto)
    {
        var emailCount = _context.Users.Count(e => e.Email == userDto.Email);
        if (emailCount > 1)
        {
            ModelState.AddModelError("Email", "Email is already registered");
            return BadRequest(ModelState);
        }
        //encrypt the password
        var passwordHasher = new PasswordHasher<User>();
        var encryptedPassword = passwordHasher.HashPassword(new User(), userDto.Password);

        User user = new User()
        {
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            Email = userDto.Email,
            Phone = userDto.Phone ?? string.Empty,
            Address = userDto.Address,
            Password = encryptedPassword,
            Role = "client",
            CreatedAt = DateTime.Now
        };

        _context.Users.Add(user);
        _context.SaveChanges();
        
        var jwt = CreateJWToken(user);

        UserProfileDto userProfileDto = new UserProfileDto()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            Role = user.Role,
            CreatedAt = DateTime.Now
        };

        var response = new
        {
            Token = jwt,
            User = userProfileDto
        };
        
        return Ok(response);
    }

    [HttpPost("Login")]
    public IActionResult Login(string email, string password)
    {
        var user = _context.Users.FirstOrDefault(e => e.Email == email);

        if (user == null)
        {
            ModelState.AddModelError("Email", "Email or password not valid");
            return BadRequest(ModelState);
        }
        
        var passwordHasher = new PasswordHasher<User>();
        var result = passwordHasher.VerifyHashedPassword(new User(), user.Password, password);

        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("Password", "Wrong password");
            return BadRequest(ModelState);
        }
        
        var jwt = CreateJWToken(user); 
        
        UserProfileDto userProfileDto = new UserProfileDto()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            Role = user.Role,
            CreatedAt = DateTime.Now
        };

        var response = new
        {
            Token = jwt,
            User = userProfileDto
        };
        
        return Ok(response);
    }

    [HttpPost("ForgotPassword")]
    public IActionResult ForgotPassword(string email)
    {
        var user = _context.Users.FirstOrDefault(e => e.Email == email);
        if (user == null)
        {
            return BadRequest(ModelState);
        }
        //delete any  old password reset request
        var pswReset = _context.PasswordResets.FirstOrDefault(e => e.Email == user.Email);

        if (pswReset != null)
        {
            //delete old password reset request
            _context.Remove(pswReset);
        }
        
        //create password Reset Token
        string token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString();
        PasswordReset passwordReset = new PasswordReset()
        {
            Email = user.Email,
            Token = token,
            CreatedAt = DateTime.Now
        };
        
        _context.PasswordResets.Add(passwordReset);
        _context.SaveChanges();
        
        string emailSubject = "Password Reset";
        string username = user.FirstName + " " + user.LastName;
        string emailMessage = "Dear " + username + "\n" + 
                              "We recieved your password reset request.\n" + 
                              "Please copy the following token and paste it in the Password Reset Form:\n" + 
                              token + "\n\n" + 
                              "Best Regards\n";

        // emailSender.SendEmail(emailSubject, email,  username, emailMessage).Wait();
        return Ok();
    }

    [HttpPost("ResetPassword")]
    public IActionResult ResetPassword(string token, string newPassword)
    {
        var pwdReset = _context.PasswordResets.FirstOrDefault(e => e.Token == token);
        if (pwdReset == null)
        {
            ModelState.AddModelError("Token", "Wrong or expired token");
            return BadRequest(ModelState);
        }
        var user = _context.Users.FirstOrDefault(e => e.Email == pwdReset.Email);
        if (user == null)
        {
            ModelState.AddModelError("Token", "Wrong or expired token");
            return BadRequest(ModelState);
        }
        
        //encrypt password
        var passwordHasher = new PasswordHasher<User>();
        string encryptedPassword = passwordHasher.HashPassword(new User(), newPassword);
        //save the new encrypted password
        user.Password = encryptedPassword;
        //delete the token
        _context.PasswordResets.Remove(pwdReset);
        
        _context.SaveChanges();
        return Ok();
    }
    // [Authorize]
    // [HttpGet("GetTokenClaims")]
    // public IActionResult GetTokenClaims()
    // {
    //     var identity = User.Identity as ClaimsIdentity;
    //     if (identity != null)
    //     {
    //         Dictionary<string, string> claims = new Dictionary<string, string>();
    //
    //         foreach (Claim claim in identity.Claims)
    //         {
    //             claims.Add(claim.Type, claim.Value);
    //         }
    //         
    //         return Ok(claims);
    //     }
    //
    //     return Ok();
    // }
    //
    //
    // [Authorize]
    // [HttpGet("AuthorizeAuthenticatedUsers")]
    // public IActionResult AuthorizeAuthenticatedUsers()
    // {
    //     return Ok("You are Authorized");
    // }
    // [Authorize(Roles = "admin")]
    // [HttpGet("AuthorizeAdmin")]
    // public IActionResult AuthorizeAdmin()
    // {
    //     return Ok("You are Authorized");
    // }
    // [Authorize(Roles = "admin, seller")]
    // [HttpGet("AuthorizeAdminAndSeller")]
    // public IActionResult AuthorizeAdminAndSeller()
    // {
    //     return Ok("You are Authorized");
    // }
    // [HttpGet("TestToken")]
    // public IActionResult TestToken()
    // {
    //     User user = new User() { Id = 2, Role = "admin" };
    //     string jwt = CreateJWToken(user);
    //     var response = new {JWToken = jwt};
    //     
    //     return Ok(response);
    // }
    private string CreateJWToken(User user)
    {
        List<Claim> claims = new List<Claim>()
        {
            new Claim("id", "" + user.Id),
            new Claim("role", user.Role),
        };
        
        string strKey = _configuration["JwtSettings:Key"];
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(strKey));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );
        
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return jwt;
    }
}