using SIPCore.Services;
using System;
using System.Threading;

namespace SIPClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var sipService = new SIPService();
            sipService.RegistrationStatusChanged += Console.WriteLine;
            sipService.CallStatusChanged += Console.WriteLine;

            // Inicializar con tus credenciales
            sipService.Initialize();
            
            // Esperar registro
            Console.WriteLine("Esperando registro SIP...");
            Thread.Sleep(3000); // Espera 3 segundos para registro

            while (true)
            {
                Console.WriteLine("\n1. Llamar\n2. Colgar\n3. Activar traducción\n4. Salir");
                var opt = Console.ReadKey().KeyChar;
                
                switch (opt)
                {
                    case '1':
                        Console.Write("\nNúmero: ");
                        var number = Console.ReadLine();
                        if (!string.IsNullOrEmpty(number))
                        {
                            sipService.MakeCall(number);
                        }
                        else
                        {
                            Console.WriteLine("Error: Debes ingresar un número");
                        }
                        break;
                        
                    case '2':
                        sipService.HangUp();
                        break;
                        
                    case '3':
                        Console.WriteLine("\nTraducción activada (implementación pendiente)");
                        break;
                        
                    case '4':
                        return;
                        
                    default:
                        Console.WriteLine("\nOpción no válida");
                        break;
                }
            }
        }
    }
}