namespace SIPCore.Services;

public interface ISIPService
{
    void Initialize(string username, string password, string domain, int port);
    void MakeCall(string number);
    void HangUp();
    void EnableTranslation(bool enable);
    
    event Action<string> CallStatusChanged;
    event Action<string> RegistrationStatusChanged;
    event Action<string> TranslationStatusChanged;
}