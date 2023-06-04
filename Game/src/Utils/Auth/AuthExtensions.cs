using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Nakama;

namespace tiktaktoe.Utils.Auth;

public static class AuthExtensions
{
	public static async Task<SignupResult> TrySignup(
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
			return new(SignupState.Exists);
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
						return new(Maybe.From(session));
					}
					catch (ApiResponseException registerException)
					{
						if (registerException.GrpcStatusCode == 6)
							return new(SignupState.UsernameUsed);
					}
					break;

				// Invalid credentials.
				case 16:
					return new(SignupState.EmailUsed);
			}
		}

		return new(SignupState.Undefined);
	}
}
