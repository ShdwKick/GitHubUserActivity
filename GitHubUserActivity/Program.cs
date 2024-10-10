using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GitHubUserActivity.DataModels;

namespace GitHubUserActivity
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            await GitHubUserActivityParser.GetInstance().ParseData();
        }
    }

    public class GitHubUserActivityParser
    {
        private readonly HttpClient _httpClient;
        private static GitHubUserActivityParser _instance;
        private string _userName;
        private string _responseString;
        private List<Event> _userEvents;

        private GitHubUserActivityParser()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("GitHubUserActivityApp/1.0");
        }

        public static GitHubUserActivityParser GetInstance()
        {
            if(_instance == null)
                _instance = new GitHubUserActivityParser();

            return _instance;
        }

        public void GetUserName()
        {
            Console.Write("Enter GitHub user name:");
            _userName = Console.ReadLine();
        }

        public async Task ParseData()
        {
            GetUserName();
            Console.Clear();
            _responseString = await SendRequest();
            await ComputeResponse();
            await PrintData();
        }

        private async Task<string> SendRequest()
        {
            string url = $"https://api.github.com/users/{_userName}/events";
            Console.WriteLine("Sending request...");
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                Console.WriteLine("Response received...");

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();
                else
                    return $"Error: {response.StatusCode}";
            }
            catch (HttpRequestException ex)
            {
                return $"Request failed: {ex.Message}";
            }
        }


        private async Task ComputeResponse()
        {
            Console.Clear();
            if (_responseString.Contains("Error") || _responseString.Contains("Request failed"))
            {
                if (_responseString.Contains("Not Found") || _responseString.Contains("NotFound"))
                {
                    Console.WriteLine("User not found");
                    await OfferAnotherUser();
                    return;
                }

                Console.WriteLine(_responseString);
                await OfferAnotherUser();
            }
            if (_responseString == "[]")
            {
                Console.WriteLine("No activity in last 3 month");
                await OfferAnotherUser();
            }

            _userEvents = JsonSerializer.Deserialize<List<Event>>(_responseString);
            if (_userEvents == null || !_userEvents.Any())
            {
                Console.WriteLine("No activity in the last 3 months or invalid data.");
                await OfferAnotherUser();
            }
        }

        private async Task PrintData()
        {
            if (_userEvents == null || !_userEvents.Any())
            {
                Console.WriteLine("User events list empty");
                return;
            }

            Console.Clear();
            var groupedEvents = _userEvents.GroupBy(q => q.repo.name);
            var output = new StringBuilder();

            foreach (var gitHubEvent in groupedEvents)
            {
                output.AppendLine($"╭GitHub repository - {gitHubEvent.Key}");
                output.AppendLine("│");

                foreach (var evnt in gitHubEvent)
                {
                    output.AppendLine("│ ╭──────────────────────────────────────────────────────")
                        .AppendLine($"│ │ Event ID: {evnt.id}")
                        .AppendLine($"│ │ Event Type: {evnt.type}")
                        .AppendLine($"│ │ Actor:")
                        .AppendLine($"│ │   ├─ Id: {evnt.actor.id}")
                        .AppendLine($"│ │   ├─ Login: {evnt.actor.login}")
                        .AppendLine($"│ │   └─ URL: {evnt.actor.url}")
                        .AppendLine($"│ │ Repository:")
                        .AppendLine($"│ │   ├─ Name: {evnt.repo.name}")
                        .AppendLine($"│ │   └─ URL: {evnt.repo.url}")
                        .AppendLine($"│ │ Public: {(evnt.isPublic ? "Yes" : "No")}")
                        .AppendLine($"│ │ Created At: {evnt.created_at}");
                }
                output.AppendLine("╰─╰─────────────────────────────────────────────────────");
            }

            PrintToConsole(output.ToString());
            await OfferAnotherUser();
        }

        private async Task OfferAnotherUser()
        {
            Console.WriteLine("Another user?(y/n)");
            var key = Console.ReadKey();
            if (key.Key == ConsoleKey.Y)
            {
                Console.Clear();
                await ParseData();
                return;
            }
            else
            {
                Environment.Exit(0);
            }
        }

        private static void PrintToConsole(string text, int printSpeed = 5)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Enter)
                {
                    Console.Write(text.Substring(i));
                    break;
                }
                Console.Write(text[i]);
                Thread.Sleep(printSpeed);
            }
        }
    }
}
