using System.Text.RegularExpressions;

namespace zad1_apbd
{
    public class EmailAddressFinder
    {
        public static async Task Main(string[] args)
        {
            //Jeśli argument nie został przekazany, powinien zostać zwrócony błąd ArgumentNullException
            if (args.Length == 0)
            {
                throw new ArgumentNullException(nameof(args), "Argument can not be null");
            }
            
            //Pojedynczy argument w postaci adresu url strony, który będzie wykorzytany przez machanizm skanowania
            var urlAddress = args[0];
            
            //Sprawdzenie czy adres url jest poprawny
            if (!Uri.IsWellFormedUriString(urlAddress, UriKind.RelativeOrAbsolute))
            {
                throw new ArgumentException("Url address is not valid");
            }

            var httpClient = new HttpClient();
            var httpResponseMessage = await httpClient.GetAsync(urlAddress);
            
            //Walidacja poprawności zwacanego statusu wiadomości
            if (((int)httpResponseMessage.StatusCode) is > 299 or < 200)
            {
                throw new Exception("Error during downloading from web page. Returned code does not mean success.");
            }

            //wywołanie metody, która drukuje adresy maili lub w przypadku błędu rzuca wyjątkiem
            await PrintOrThrowWhenEmpty(httpResponseMessage);
        }

        private static async Task PrintOrThrowWhenEmpty(HttpResponseMessage httpResponseMessage)
        {
            var content = await httpResponseMessage.Content.ReadAsStringAsync();
            
            //Sprawdzenie zawartość strony podiada jakiś adres email
            if (content is null)
            {
                throw new Exception("No email addresses found");
            }
            
            //Wyrażenie regularne sprawdzające adres email
            Regex regexMail = new Regex(
                @"(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var emails = regexMail
                .Matches(content) //kolekcja dopasowań
                .Select(x => x.Groups[0].Value) //obiekt reprezentujący pojedyńcze dopasowanie z sukcesem
                .Distinct() //filtrowanie unikalnych wartości (bez powtórzeń)
                .ToList(); //zapis do listy stringów

            //Warunek sprawdzający czy w liście maili znajduje się jakikolwiek mail
            if (emails.Count == 0)
            {
                throw new Exception("No email address found");
            }
            
            //Wypisanie na konsolę listy unikalnych adresów email znalezionych na stronie o podanym url
            foreach (var email in emails)
            {
                Console.WriteLine(email);
            }
        }
    }
}