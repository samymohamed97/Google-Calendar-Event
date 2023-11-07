using GoogleCalendarEvent.DTO;
using GoogleCalendarEvent.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GoogleCalendarEvent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration config;
        public AccountController(UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            this.userManager = userManager;
            this.config = config;

        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm]RegisterDTO RegDTO)
        {
            if (ModelState.IsValid == true)
            {

                ApplicationUser user = new ApplicationUser();
                user.UserName = RegDTO.UserName;
                user.Email = RegDTO.Email;
                IdentityResult result = await userManager.CreateAsync(user, RegDTO.Password);


                if (result.Succeeded)
                {
                    return Ok("Account Created");
                }
                return BadRequest(result.Errors.FirstOrDefault());


            }
            return BadRequest(ModelState);
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromForm]LoginDTO LoginDTO)
        {
            {
                if (ModelState.IsValid == true)
                {
                    ApplicationUser user = await userManager.FindByEmailAsync(LoginDTO.Email);
                    if (user != null)
                    {
                        bool found = await userManager.CheckPasswordAsync(user, LoginDTO.Password);
                        if (found)
                        {
                            var claims = new List<Claim>();
                            claims.Add(new Claim(ClaimTypes.Email, user.Email));
                            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
                            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

                            var role = await userManager.GetRolesAsync(user);
                            foreach (var itemRole in role)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, itemRole));
                            }
                            SecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Secret"]));
                            SigningCredentials signcred = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                            JwtSecurityToken token = new JwtSecurityToken(
                             issuer: config["JWT:ValidIssuer"],
                             audience: config["JWT:ValidAudience"],
                             claims: claims,
                             signingCredentials: signcred,
                             expires: DateTime.Now.AddHours(1)
                                );
                            return Ok(new
                            {
                                token = new JwtSecurityTokenHandler().WriteToken(token),
                                expiration = token.ValidTo
                            });
                        }
                    }
                    return Unauthorized();
                }
                return Unauthorized();
            }
        }
    }
}
