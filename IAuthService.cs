using System.Threading.Tasks;
using SafeVault.Data;

namespace SafeVault.Services
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(UserRegisterDto dto);
        Task<string?> AuthenticateAsync(UserLoginDto dto);
    }
}
