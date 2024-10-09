using System.Collections.ObjectModel;
using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json.Serialization;
using System.Transactions;

namespace GitHubUserActivity.DataModels
{
    public class Repo
    {
        public int id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class Actor
    {
        public int id { get; set; }
        public string login { get; set; }
        public string display_login { get; set; }
        public string gravatar_id { get; set; }
        public string url { get; set; }
        public string avatar_url { get; set; }
    }

    public class Payload
    {
        public int repository_id { get; set; }
        public int push_id { get; set; }
        public int size { get; set; }
        public int distinct_size { get; set; }

        [JsonPropertyName("ref")] public string reference { get; set; }
        public string head { get; set; }
        public string before { get; set; }
        public ObservableCollection<Commit> commits { get; set; }
        [JsonPropertyName("public")] public bool isPublic { get; set; }
        public DateTime created_at { get; set; }
    }

    public class Commit
    {
        public string sha { get; set; }
        public Author author { get; set; }
        public string message { get; set; }
        public bool distinct { get; set; }
        public string url { get; set; }
    }

    public class Author
    {
        public string email { get; set; }
        public string name { get; set; }
    }

    public class Event
    {
        public string id { get; set; }
        public string type { get; set; }
        public Actor actor { get; set; }
        public Repo repo { get; set; }
        //public Payload payload { get; set; }
        [JsonPropertyName("public")] public bool isPublic { get; set; }
        public DateTime created_at { get; set; }
    }
}