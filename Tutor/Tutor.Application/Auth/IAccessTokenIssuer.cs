using Tutor.Domain.Users;

namespace Tutor.Application.Auth;

public interface IAccessTokenIssuer
{
    string Issue(User user);
}
