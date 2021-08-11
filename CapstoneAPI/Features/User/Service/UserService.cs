namespace CapstoneAPI.Features.User.Service
{
    using AutoMapper;
    using CapstoneAPI.Features.User.DataSet;
    using CapstoneAPI.Filters;
    using CapstoneAPI.Helpers;
    using CapstoneAPI.Repositories;
    using CapstoneAPI.Wrappers;
    using FirebaseAdmin.Auth;
    using FirebaseAdmin.Messaging;
    using Microsoft.IdentityModel.Tokens;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;

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

        public async Task<Response<UserDataSet>> ValidateJwtToken(string token)
        {
            Response<UserDataSet> response = new Response<UserDataSet>();

            try
            {
                Models.User user = await _uow.UserRepository.GetUserByToken(token);
                
                if (user == null || !user.IsActive)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                } else
                {
                    Models.Province province = await _uow.ProvinceRepository.GetById(user.ProvinceId);
                    response.Succeeded = true;
                    response.Data = _mapper.Map<UserDataSet>(user);
                    if (province != null)
                    {
                        response.Data.ProvinceName = province.Name;
                    }
                }
            }
            catch (Exception ex)
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
                    user = new Models.User()
                    {
                        Email = decodedToken.Claims["email"].ToString(),
                        Username = decodedToken.Claims["email"].ToString(),
                        Fullname = decodedToken.Claims["name"].ToString(),
                        AvatarUrl = decodedToken.Claims["picture"].ToString(),
                        IsActive = true,
                        RoleId = int.Parse(Roles.Student)
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
                var claims = new[]
                {
                        new Claim(ClaimTypes.Role, user.RoleId.ToString() ?? ""),
                        new Claim("userId", user.Id.ToString() ?? ""),
                        new Claim("userName", user.Username ?? ""),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(AppSettings.Settings.JwtSecret));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                token = new JwtSecurityToken(AppSettings.Settings.Issuer,
                                                    AppSettings.Settings.Audience,
                                                    claims,
                                                    expires: JWTUtils.GetCurrentTimeInVN().AddSeconds(Consts.TOKEN_EXPIRED_TIME),
                                                    signingCredentials: creds);
                Models.Province province = await _uow.ProvinceRepository.GetById(user.ProvinceId);
                UserDataSet userResponse = _mapper.Map<UserDataSet>(user);
                if (province != null)
                {
                    userResponse.ProvinceName = province.Name;
                }
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

        public async Task<PagedResponse<List<UserDataSet>>> GetListUsers(PaginationFilter paging, AdminUserFilter query)
        {
            PagedResponse<List<UserDataSet>> response = new PagedResponse<List<UserDataSet>>();

            try
            {
                Expression<Func<Models.User, bool>> filter = null;

                filter = u => (string.IsNullOrEmpty(query.Fullname) || u.Fullname.Contains(query.Fullname))
                && (string.IsNullOrEmpty(query.Email) || u.Fullname.Contains(query.Email))
                && (!query.IsActive.HasValue || u.IsActive == query.IsActive)
                && (!query.Role.HasValue || u.RoleId == query.Role);

                List<UserDataSet> users = (await _uow.UserRepository
                    .Get(filter: filter, orderBy: o => o.OrderByDescending(u => u.Fullname),
                    first: paging.PageSize, offset: (paging.PageNumber - 1) * paging.PageSize))
                    .Select(u => _mapper.Map<UserDataSet>(u)).ToList();

                var totalRecords = await _uow.UserRepository.Count(filter);
                response = PaginationHelper.CreatePagedReponse(users, paging, totalRecords);
            }
            catch (Exception ex)
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

        public async Task<Response<bool>> UpdateUser(UpdateUserParam param)
        {
            Response<bool> response = new Response<bool>();
            if (param.Role != null &&  param.Role != int.Parse(Roles.Admin)
                && param.Role != int.Parse(Roles.Staff) && param.Role != int.Parse(Roles.Student))
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Cập nhật vai trò không hợp lệ!");
                return response;
            }
            try
            {
                Models.User user = await _uow.UserRepository.GetById(param.Id);
                if (user == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tài khoản này không tồn tại");
                    return response;
                }

                if (param.IsActive != null)
                {
                    user.IsActive = (bool) param.IsActive;
                }

                if (param.Role != null)
                {
                    user.RoleId = (int) param.Role;
                }

                if (await _uow.CommitAsync() <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Lỗi hệ thống!");
                    return response;
                }
                response.Data = true;
                response.Succeeded = true;
            }
            catch (Exception ex)
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

        public async Task<Response<bool>> UnsubscribeTopic(RegisterToken registerToken, string token)
        {
            Response<bool> response = new Response<bool>();
            Models.User user = await _uow.UserRepository.GetUserByToken(token);

            if (user == null)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Bạn chưa đăng nhập!");
                return response;
            }
            if (registerToken == null || registerToken.token == null)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Token unsubscribe không hợp lệ!");
                return response;
            }
            var res = await FirebaseMessaging.DefaultInstance
               .UnsubscribeFromTopicAsync(new List<string>{ registerToken.token}, user.Id.ToString());

            if (res.SuccessCount > 0)
            {
                response.Succeeded = true;
            } else
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Lỗi firebase!");
            }
            return response;
        }

        public async Task<Response<bool>> SubscribeTopic(RegisterToken registerToken, string token)
        {
            Response<bool> response = new Response<bool>();
            Models.User user = await _uow.UserRepository.GetUserByToken(token);

            if (user == null)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Bạn chưa đăng nhập!");
                return response;
            }
            if (registerToken == null || registerToken.token == null)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Token subscribe không hợp lệ!");
                return response;
            }
            var res = await FirebaseMessaging.DefaultInstance
               .SubscribeToTopicAsync(new List<string> { registerToken.token }, user.Id.ToString());

            if (res.SuccessCount > 0)
            {
                response.Succeeded = true;
            }
            else
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Lỗi firebase!");
            }
            return response;
        }
    }
}
