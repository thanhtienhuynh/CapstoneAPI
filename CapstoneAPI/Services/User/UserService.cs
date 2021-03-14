﻿namespace CapstoneAPI.Services.User
{
    using AutoMapper;
    using CapstoneAPI.DataSets.User;
    using CapstoneAPI.Helpers;
    using CapstoneAPI.Repositories;
    using FirebaseAdmin.Auth;
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using static CapstoneAPI.Controllers.UsersController;

    public class UserService : IUserService
    {
        private IMapper _mapper;

        private readonly IUnitOfWork _uow;

        public UserService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<LoginResponse> Login(Token firebaseToken)
        {
            Models.User user;
            JwtSecurityToken token = null;
            FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(firebaseToken.uidToken);
            user = await _uow.UserRepository.GetFirst(filter: u => u.Email.Equals(decodedToken.Claims["email"].ToString()), includeProperties: "Role");
            if (user == null)
            {
                int userRoleId = (await _uow.RoleRepository.GetFirst(filter: r => r.Name.Equals(Consts.USER_ROLE))).Id;
                user = new Models.User()
                {
                    Email = decodedToken.Claims["email"].ToString(),
                    Fullname = decodedToken.Claims["name"].ToString(),
                    AvatarUrl = decodedToken.Claims["picture"].ToString(),
                    IsActive = true,
                    RoleId = userRoleId
                };
                _uow.UserRepository.Insert(user);

                if (await _uow.CommitAsync() > 0)
                {
                    
                }
                else
                {
                    return null;
                }
            }
            bool isAdmin = user.Role.Name.Equals(Consts.ADMIN_ROLE);
            var claims = new[]
            {
                        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                        new Claim(ClaimTypes.Role, isAdmin ? Consts.ADMIN_ROLE : Consts.USER_ROLE),
                        new Claim(ClaimTypes.GivenName, user.Fullname == null ? "" : user.Fullname),
                        new Claim(ClaimTypes.MobilePhone, user.Phone == null ? "" : user.Phone),
                        new Claim(ClaimTypes.Uri, user.AvatarUrl == null ? "" : user.AvatarUrl),
                        new Claim(ClaimTypes.Email, user.Email == null ? "" : user.Email),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(AppSettings.Settings.JwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            token = new JwtSecurityToken(AppSettings.Settings.Issuer,
                                                AppSettings.Settings.Audience,
                                                claims,
                                                expires: DateTime.UtcNow.AddSeconds(1 * 10),
                                                signingCredentials: creds);
            UserDataSet userResponse = _mapper.Map<UserDataSet>(user);
            userResponse.IsAdmin = isAdmin;
            LoginResponse loginResponse = new LoginResponse()
            {
                User = userResponse,
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };
            return loginResponse;
        }
    }
}
