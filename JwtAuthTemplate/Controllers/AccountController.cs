#nullable disable

using JwtAuthTemplate.Constants;
using JwtAuthTemplate.Data.Models;
using JwtAuthTemplate.Dtos;
using JwtAuthTemplate.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JwtAuthTemplate.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _config;
    private readonly ITokenService _tokenService;

    public AccountController(UserManager<ApplicationUser> userManager,
                             SignInManager<ApplicationUser> signInManager,
                             RoleManager<IdentityRole> roleManager,
                             IConfiguration config,
                             ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _config = config;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserLoginResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
    {
        if (userLoginDto is null || !ModelState.IsValid) return BadRequest();

        var user = await _userManager.Users.AsNoTracking()
                                           .Where(u => u.Email == userLoginDto.Email)
                                           .FirstOrDefaultAsync();

        if (user is null) return Unauthorized();

        var result = await _signInManager.CheckPasswordSignInAsync(user, userLoginDto.Password, lockoutOnFailure: false);

        if (!result.Succeeded) return Unauthorized();

        var token = await _tokenService.CreateTokenAsync(user);

        Response.Cookies.Append(_config["JwtSettings:CookieName"],
                                token,
                                new CookieOptions()
                                {
                                    HttpOnly = true,
                                    Secure = true,
                                    SameSite = SameSiteMode.Strict,
                                    Expires = DateTimeOffset.Now.AddMinutes(_config.GetSection("JwtSettings:ExpiresInMinutes").Get<int>())
                                });

        var userRoles = await _userManager.GetRolesAsync(user);

        var userResponseDto = UserLoginResponseDto.CreateFromUser(user, userRoles);

        return Ok(userResponseDto);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Logout()
    {
        if (string.IsNullOrEmpty(Request.Cookies[_config["JwtSettings:CookieName"]])) return BadRequest();

        Response.Cookies.Delete(_config["JwtSettings:CookieName"]);

        return NoContent();
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserProfileResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterUser([FromBody] UserRegistrationDto userRegistrationDto)
    {
        if (userRegistrationDto is null || !ModelState.IsValid) return BadRequest();

        var userToCreate = new ApplicationUser() 
        { 
            Email = userRegistrationDto.Email,
            UserName = userRegistrationDto.Email[..userRegistrationDto.Email.IndexOf("@")],
        };

        var result = await _userManager.CreateAsync(userToCreate, userRegistrationDto.Password!);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(errors);
        }

        if (await _roleManager.RoleExistsAsync(RoleTypes.USER) == false)
        {
            var userRole = new IdentityRole { Name = RoleTypes.USER, NormalizedName = RoleTypes.USER.ToUpper() };
            await _roleManager.CreateAsync(userRole);
        }

        await _userManager.AddToRoleAsync(userToCreate, RoleTypes.USER);
        await _userManager.AddClaimAsync(userToCreate, new Claim(ClaimTypes.NameIdentifier, userToCreate.Id));

        var actionName = nameof(GetUserProfile);
        var routeValues = new { userId = userToCreate.Id };
        var createdUser = UserProfileResponseDto.CreateFromUser(userToCreate);

        return CreatedAtAction(actionName, routeValues, createdUser);
    }

    [HttpGet("users")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserProfileResponseDto>))]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userManager.Users.AsNoTracking().ToListAsync();

        var userProfileDtos = UserProfileResponseDto.CreateFromUserList(users);

        return Ok(userProfileDtos);
    }

    [HttpGet("users/{userId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserProfileResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserProfile(string userId)
    {
        if (userId is null) return BadRequest();

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null) return NotFound();

        var userProfileDto = UserProfileResponseDto.CreateFromUser(user);

        return Ok(userProfileDto);
    }

    [HttpPut("users/{userId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserProfile([FromBody] UserProfileDto userProfileDto)
    {
        if (userProfileDto is null) return BadRequest();

        var loggedInUserEmail = User.FindFirstValue(ClaimTypes.Email);

        if (loggedInUserEmail is null || loggedInUserEmail != userProfileDto.Email) return Unauthorized();

        var userToUpdate = await _userManager.FindByIdAsync(userProfileDto.Id);

        if (userToUpdate is null) return NotFound();

        userToUpdate.FirstName = userProfileDto.FirstName;
        userToUpdate.LastName = userProfileDto.LastName;

        await _userManager.UpdateAsync(userToUpdate);

        return Ok(UserProfileResponseDto.CreateFromUser(userToUpdate));
    }

    // TODO: Add route to update user email

    // TODO: Add route to update user password
}
