using Microsoft.Extensions.DependencyInjection;
using SIPCore.Services;

var services = new ServiceCollection()
    .AddSingleton<ISIPService, SIPService>()
    .AddSingleton<TranslationService>()
    .BuildServiceProvider();

var sipService = services.GetRequiredService<ISIPService>();
var translationService = services.GetRequiredService<TranslationService>();

// Configurar eventos
sipService.CallStatusChanged += Console.WriteLine;
sipService.RegistrationStatusChanged += Console.WriteLine;
sipService.TranslationStatusChanged += Console.WriteLine;

translationService.TranslationPerformed += translatedText => 
{
    Console.WriteLine($"Texto traducido: {translatedText}");
};

// Inicializar con tus credenciales
sipService.Initialize(
    username: "frankyan",
    password: "hb8zRUcD8CTGS6x",
    domain: "sip.antisip.com",
    port: 5060
);

// Menú simple
while (true)
{
    Console.WriteLine("\n1. Llamar\n2. Colgar\n3. Activar traducción\n4. Salir");
    var opt = Console.ReadKey().KeyChar;
    
    switch (opt)
    {
        case '1':
            Console.Write("\nNúmero: ");
            var number = Console.ReadLine();
            sipService.MakeCall(number);
            break;
        case '2':
            sipService.HangUp();
            break;
        case '3':
            sipService.EnableTranslation(true);
            break;
        case '4':
            return;
    }
}