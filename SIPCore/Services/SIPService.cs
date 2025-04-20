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

    public void Initialize(string username, string password, string domain, int port)
    {
        try
        {
            _softPhone = SoftPhoneFactory.CreateSoftPhone(IPAddress.Loopback, 10000, 20000);
            
            var account = new SIPAccount(
                registrationRequired: true,
                displayName: "SIPTranslator",
                userName: username,
                registerPassword: password,
                domainHost: domain,
                domainPort: port
            );
            
            _phoneLine = _softPhone.CreatePhoneLine(account);
            _phoneLine.RegistrationStateChanged += (sender, e) => 
            {
                RegistrationStatusChanged?.Invoke($"Estado registro: {e.State}");
            };
            
            _softPhone.RegisterPhoneLine(_phoneLine);
            
            _connector = new MediaConnector();
            _audioSender = new PhoneCallAudioSender();
            _audioReceiver = new PhoneCallAudioReceiver();
        }
        catch (Exception ex)
        {
            CallStatusChanged?.Invoke($"Error: {ex.Message}");
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