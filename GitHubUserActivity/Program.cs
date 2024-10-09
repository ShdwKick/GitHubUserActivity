using System.Collections.ObjectModel;
using System.Net.Http.Json;
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
        private ObservableCollection<Event> _userEvents;

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
            //GetUserName();
            Console.Clear();
            _responseString = await SendRequest();
            ComputeResponse();
            PrintData();
        }

        private async Task<string> SendRequest()
        {
            //string url = $"https://api.github.com/users/{_userName}/events";

            string url = $"https://api.github.com/users/ShdwKick/events";
            Console.WriteLine("Sending request...");
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            Console.WriteLine("Response received...");
            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                return result;
            }
            else
            {
                var str = await response.Content.ReadAsStringAsync();
                Console.WriteLine(str);
                return $"Error: {response.StatusCode}";
            }
        }

        private void ComputeResponse()
        {
            if (_responseString.Contains("Error:"))
            {
                Console.WriteLine(_responseString);
                Console.WriteLine("Press any key");
                Console.ReadKey();
                return;
            }

            _userEvents = JsonSerializer.Deserialize<ObservableCollection<Event>>(_responseString);
        }

        private void PrintData()
        {
            if (_userEvents == null)
            {
                Console.WriteLine("user events list empty");
                return;
            }
            Console.Clear();
            var groupedEvents = _userEvents.GroupBy(q => q.repo.name);
            
            foreach (var gitHubEvent in groupedEvents)
            {
                Console.WriteLine($"╭GitHub repository - {gitHubEvent.Key}");
                Console.WriteLine($"│");

                foreach (var evnt in gitHubEvent)
                {
                    if(evnt != gitHubEvent.First())
                        Console.WriteLine("│ ╭─────────────────────────────────────────────────────╯");
                    else
                        Console.WriteLine("│ ╭──────────────────────────────────────────────────────");

                    Console.WriteLine("│ │ Event ID: " + evnt.id);
                    Console.WriteLine("│ │ Event Type: " + evnt.type);
                    Console.WriteLine("│ │ Actor:");
                    Console.WriteLine("│ │   ├─ Id: " + evnt.actor.id);
                    Console.WriteLine("│ │   ├─ Login: " + evnt.actor.login);
                    Console.WriteLine("│ │   └─ URL: " + evnt.actor.url);
                    Console.WriteLine("│ │ Repository:");
                    Console.WriteLine("│ │   ├─ Name: " + evnt.repo.name);
                    Console.WriteLine("│ │   └─ URL: " + evnt.repo.url);
                    Console.WriteLine("│ │ Public: " + (evnt.isPublic ? "Yes" : "No"));
                    Console.WriteLine("│ │ Created At: " + evnt.created_at);

                    if(evnt != gitHubEvent.Last())
                        Console.WriteLine("│ ╰─────────────────────────────────────────────────────╮");
                    else
                        Console.WriteLine("╰─╰─────────────────────────────────────────────────────\n");
                }

            }
        }
    }
}
