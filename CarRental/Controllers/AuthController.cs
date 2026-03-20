using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CarRental.Models;
using CarRental.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CarRental.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = new ApplicationUser { UserName = model.Username, Email = model.Email, FullName = model.FullName };
            var res = await _userManager.CreateAsync(user, model.Password);
            if (!res.Succeeded) return BadRequest(res.Errors);
            await _userManager.AddToRoleAsync(user, "User");
            return Ok(new { message = "User created" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null) return Unauthorized(new { error = "Invalid credentials" });
            var check = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!check.Succeeded) return Unauthorized(new { error = "Invalid credentials" });

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }.Union(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var key = _config["Jwt:Key"];
            var issuer = _config["Jwt:Issuer"] ?? "CarRental";
            var keyBytes = Encoding.UTF8.GetBytes(key ?? string.Empty);
            var token = new JwtSecurityToken(
                issuer: issuer,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256)
            );
            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new { token = tokenStr });
        }
    }
}
