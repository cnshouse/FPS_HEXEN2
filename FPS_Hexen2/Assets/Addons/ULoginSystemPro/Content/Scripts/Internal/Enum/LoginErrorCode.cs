namespace MFPS.ULogin
{
    public enum LoginErrorCode
    {
        UserNotExist = 001,
        PasswordIncorrect = 002,
        UserAlreadyRegister = 003,
        InvalidEmail = 004,
        EmailExist = 005,
        EmailNotSend = 006,
        NotActive = 007,
        UserNotFound = 008,
        UserAndEmailNotFound = 009,
        NoUserInRanking = 010,
    }

    public enum RememberMeBehave
    {
        RememberUserName = 0,
        RememberSession = 1,
        NoRemember = 2,
    }
}