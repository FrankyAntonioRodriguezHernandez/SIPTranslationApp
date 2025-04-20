using Ozeki.Media;
using Ozeki.VoIP;
using System.Net;

namespace SIPCore.Services;

public class SIPService : ISIPService
{
    private ISoftPhone _softPhone;
    private IPhoneLine _phoneLine;
    private IPhoneCall _call;
    private MediaConnector _connector;
    private PhoneCallAudioSender _audioSender;
    private PhoneCallAudioReceiver _audioReceiver;
    private bool _translationEnabled;

    public event Action<string> CallStatusChanged;
    public event Action<string> RegistrationStatusChanged;
    public event Action<string> TranslationStatusChanged;

    public void Initialize(string username = "frankyan", 
                string password = "hb8zRUcD8CTGS6x", 
                string domain = "sip.antisip.com", 
                int port = 5060)
{
    try
    {
        _softPhone = SoftPhoneFactory.CreateSoftPhone(IPAddress.Parse("192.168.29.25"), 10000, 20000);
        
        var account = new SIPAccount(
            registrationRequired: true,
            displayName: "SIPTranslator",
            userName: username,
            registerName: username,
            registerPassword: password,
            domainHost: domain,
            domainPort: port
        );
        
        _phoneLine = _softPhone.CreatePhoneLine(account);
        _phoneLine.RegistrationStateChanged += (sender, e) => 
        {
            RegistrationStatusChanged?.Invoke($"Estado registro: {e.State}");
            if (e.State == RegState.Error)
            {
                RegistrationStatusChanged?.Invoke($"Error de registro: Revisa credenciales y conexión");
            }
        };
        
        _softPhone.RegisterPhoneLine(_phoneLine);
        
        // Espera activa para registro (máximo 10 segundos)
        for (int i = 0; i < 10; i++)
        {
            if (_phoneLine.RegState == RegState.RegistrationSucceeded) 
            {
                RegistrationStatusChanged?.Invoke("¡Registrado correctamente!");
                break;
            }
            Thread.Sleep(1000);
        }
        
        if (_phoneLine.RegState != RegState.RegistrationSucceeded)
        {
            RegistrationStatusChanged?.Invoke("Error: Tiempo de espera agotado para registro");
        }
    }
    catch (Exception ex)
    {
        CallStatusChanged?.Invoke($"Error inicialización: {ex.Message}");
    }
}

    public void MakeCall(string number)
    {
        try
        {
            _call = _softPhone.CreateCallObject(_phoneLine, number);
            _call.CallStateChanged += (sender, e) => 
            {
                CallStatusChanged?.Invoke($"Estado llamada: {e.State}");
                
                if (e.State == CallState.Answered)
                {
                    SetupAudioStreams();
                }
            };
            
            _call.Start();
        }
        catch (Exception ex)
        {
            CallStatusChanged?.Invoke($"Error llamada: {ex.Message}");
        }
    }

    private void SetupAudioStreams()
    {
        try
        {
            dynamic audioSource = Activator.CreateInstance(Type.GetTypeFromProgID("Ozeki.Audio.Source"));
            _connector.Connect(audioSource, _audioSender);
            _audioSender.AttachToCall(_call);
            audioSource.Start();

            dynamic audioSink = Activator.CreateInstance(Type.GetTypeFromProgID("Ozeki.Audio.Sink"));
            _audioReceiver.AttachToCall(_call);
            _connector.Connect(_audioReceiver, audioSink);
            audioSink.Start();
        }
        catch (Exception ex)
        {
            CallStatusChanged?.Invoke($"Error audio: {ex.Message}");
        }
    }

    public void HangUp()
    {
        _call?.HangUp();
    }

    public void EnableTranslation(bool enable)
    {
        _translationEnabled = enable;
        TranslationStatusChanged?.Invoke(enable ? "Traducción activada" : "Traducción desactivada");
    }
}