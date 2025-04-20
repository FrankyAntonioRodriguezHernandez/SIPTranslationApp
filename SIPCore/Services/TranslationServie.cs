namespace SIPCore.Services;

public class TranslationService
{
    public event Action<string> TranslationPerformed;
    
    public async Task<string> TranslateAsync(string text, string fromLang, string toLang)
    {
        // Implementación simulada
        await Task.Delay(500);
        var translated = $"Translated({fromLang}-{toLang}): {text}";
        TranslationPerformed?.Invoke(translated);
        return translated;
    }
}