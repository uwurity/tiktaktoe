using System.Threading.Tasks;
using LanguageExt;
using static LanguageExt.Prelude;
using Nakama;

namespace tiktaktoe.Shared.NakamaHelpers;

public static class AuthExtensions
{
    public enum SignupState
    {
        Exists,
        UsernameUsed,
        EmailUsed,
        Undefined
    }

    public static async Task<Either<SignupState, ISession>> TrySignup(
        this IClient client,
        string email,
        string password,
        string? username = null
    )
    {
        try
        {
            var session = await client.AuthenticateEmailAsync(email, password, username, create: false);
            await client.SessionLogoutAsync(session);
            return Left(SignupState.Exists);
        }
        catch (ApiResponseException e)
        {
            switch (e.GrpcStatusCode)
            {
                // not exists
                case 5:
                    try
                    {
                        var session = await client.AuthenticateEmailAsync(email, password, username, create: true);
                        return Right(session);
                    }
                    catch (ApiResponseException registerException)
                    {
                        if (registerException.GrpcStatusCode == 6)
                            return Left(SignupState.UsernameUsed);
                    }
                    break;

                // Invalid credentials.
                case 16:
                    return Left(SignupState.EmailUsed);
            }
        }

        return Left(SignupState.Undefined);
    }
}
