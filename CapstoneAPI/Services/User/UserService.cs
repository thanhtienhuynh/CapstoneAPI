namespace CapstoneAPI.Services.User
{
    using AutoMapper;
    using CapstoneAPI.DataSets.User;
    using CapstoneAPI.Helpers;
    using CapstoneAPI.Repositories;
    using CapstoneAPI.Wrappers;
    using FirebaseAdmin.Auth;
    using Microsoft.IdentityModel.Tokens;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using static CapstoneAPI.Controllers.UsersController;

    public class UserService : IUserService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ILogger _log = Log.ForContext<UserService>();
        public UserService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Response<LoginResponse>> Login(Token firebaseToken)
        {
            Response<LoginResponse> response = new Response<LoginResponse>();

            try
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
                        Username = decodedToken.Claims["email"].ToString(),
                        Fullname = decodedToken.Claims["name"].ToString(),
                        AvatarUrl = decodedToken.Claims["picture"].ToString(),
                        IsActive = true,
                        RoleId = userRoleId
                    };
                    _uow.UserRepository.Insert(user);

                    if (await _uow.CommitAsync() <= 0)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Đăng nhập vào hệ thống thất bại!");
                        return response;
                    }
                }
                bool isAdmin = user.Role.Name.Equals(Consts.ADMIN_ROLE);
                var claims = new[]
                {
                        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                        new Claim(ClaimTypes.Role, isAdmin ? Consts.ADMIN_ROLE : Consts.USER_ROLE),
                        new Claim(ClaimTypes.GivenName, user.Fullname ?? ""),
                        new Claim(ClaimTypes.MobilePhone, user.Phone ?? ""),
                        new Claim(ClaimTypes.Uri, user.AvatarUrl ?? ""),
                        new Claim(ClaimTypes.Email, user.Email ?? ""),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(AppSettings.Settings.JwtSecret));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                token = new JwtSecurityToken(AppSettings.Settings.Issuer,
                                                    AppSettings.Settings.Audience,
                                                    claims,
                                                    expires: DateTime.UtcNow.AddSeconds(Consts.TOKEN_EXPIRED_TIME),
                                                    signingCredentials: creds);
                UserDataSet userResponse = _mapper.Map<UserDataSet>(user);
                userResponse.IsAdmin = isAdmin;
                LoginResponse loginResponse = new LoginResponse()
                {
                    User = userResponse,
                    Token = new JwtSecurityTokenHandler().WriteToken(token)
                };
                response.Succeeded = true;
                response.Data = loginResponse;
            } catch (Exception ex)
            {
                _log.Error(ex.ToString());
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }
            
            return response;
        }
    }
}
