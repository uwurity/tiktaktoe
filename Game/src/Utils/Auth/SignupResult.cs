using CSharpFunctionalExtensions;
using Nakama;

namespace tiktaktoe.Utils.Auth;

public enum SignupState
{
    Success,
    Exists,
    UsernameUsed,
    EmailUsed,
    Undefined
}

public class SignupResult
{    
    public SignupState State { get; }
    public Maybe<ISession> Session { get; }

    public SignupResult(SignupState state, Maybe<ISession>? session = null)
    {
        State = state;
        Session = session ?? Maybe.None;
    }
        
    public SignupResult(Maybe<ISession> session, SignupState? state = null)
    {
        State = state ?? SignupState.Success;
        Session = session;
    }

    public override string ToString() => State.ToString();
}
