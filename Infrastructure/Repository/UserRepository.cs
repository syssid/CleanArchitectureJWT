using Application.Contracts;
using Application.DTOs.Request;
using Application.DTOs.Response;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    internal class UserRepository : IUser
	{
		private readonly AppDbContext _context;

		public IConfiguration Configuration { get; }

		public UserRepository(AppDbContext context, IConfiguration configuration)
        {
			_context = context;
			Configuration = configuration;
		}
        public async Task<LoginResponse> LoginUserAsync(LoginDTO loginDTO)
		{
			var getUser = await _context.Users.FirstOrDefaultAsync(user => user.Email == loginDTO.Email);
			if (getUser == null)
				return new LoginResponse(false, "User not found");

			bool checkPassword = BCrypt.Net.BCrypt.Verify(loginDTO.Password, getUser.Password);
			if (checkPassword)
				return new LoginResponse(true, "Login Successful", GenerateJWTToken(getUser));
			else
				return new LoginResponse(false, "Invalid Credentials");
			
		}

		private async Task<ApplicationUser> findUserByEmail(string Email) =>
			await _context.Users.FirstOrDefaultAsync(user => user.Email == Email);

		public async Task<RegistrationResponse> RegisterUserAsync(RegisterUserDTO registerUserDTO)
		{
			var getUser = await findUserByEmail(registerUserDTO.Email!);
			if (getUser != null)
				return new RegistrationResponse(false, "User Already Exists");

			_context.Users.Add(new ApplicationUser()
			{
				Name = registerUserDTO.Name,
				Email = registerUserDTO.Email,
				Password = BCrypt.Net.BCrypt.HashPassword(registerUserDTO.Password)
			});

			await _context.SaveChangesAsync();
			return new RegistrationResponse(true, "Registration Successfully");
		}

		private string GenerateJWTToken(ApplicationUser user)
		{
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]!));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			var userClaims = new[]
			{
				new Claim(ClaimTypes.NameIdentifier , user.ID.ToString()),
				new Claim(ClaimTypes.Name, user.Name!),
				new Claim(ClaimTypes.Email, user.Email!)
			};

			var token = new JwtSecurityToken(
				issuer : Configuration["Jwt:Issuer"],
				audience : Configuration["Jwt:Audience"],
				claims : userClaims,
				expires: DateTime.Now.AddDays(5),
				signingCredentials: credentials
				);
			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
