using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;
using StorySpoilerAPI.Models;

namespace StorySpoilerAPI
{
    [TestFixture]
    public class StorySpoilerTests
    {
        private RestClient client;
        private static string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";
        private static string lastCreatedStoryId;
        public const string userName = "emi123";
        public const string userPassword = "123456";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken(userName, userPassword);
            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);
            var jsonItems = JsonSerializer.Deserialize<JsonElement>(response.Content);

            var accessToken = jsonItems.GetProperty("accessToken").GetString();

            return accessToken;
        }

        [Test, Order(1)]
        public void Test_CreateNewStory_WithRequiedFields_ShouldReturnCreated()
        {
            var requestBody = new StoryDTO
            {
                Title = "New Story Title",
                Description = "Some Description",
                Url = ""
            };
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(requestBody);

            var response = client.Execute(request);

            Assert.That(response.IsSuccessStatusCode, Is.True);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.Content, Does.Contain("Successfully created!"));

            var responseItems = JsonSerializer.Deserialize<JsonElement>(response.Content);
            Assert.That(responseItems.TryGetProperty("storyId", out _), Is.True);

            lastCreatedStoryId = responseItems.GetProperty("storyId").GetString();
        }

        [Test, Order(2)]
        public void Test_EditLastCreatedStory_ShouldReturnSuccess()
        {
            var editedBody = new StoryDTO
            {
                Title = "Edited Story Title",
                Description = "Some Edited Description",
                Url = ""
            };
            var request = new RestRequest($"/api/Story/Edit/{lastCreatedStoryId}", Method.Put);
            request.AddJsonBody(editedBody);

            var response = client.Execute(request);

            Assert.That(response.IsSuccessStatusCode, Is.True);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Successfully edited"));
        }

        [Test, Order(3)]
        public void Test_GetAllStories_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);

            var response = client.Execute(request);

            Assert.That(response.IsSuccessStatusCode, Is.True);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseItems = JsonSerializer.Deserialize<StoryDTO[]>(response.Content);
            Assert.That(responseItems, Is.Not.Empty);
            Assert.That(responseItems.Any(), Is.True);
        }

        [Test, Order(4)]
        public void Test_DeleteLastCreatedStory_ShouldReturnSuccess()
        {
            var request = new RestRequest($"/api/Story/Delete/{lastCreatedStoryId}", Method.Delete);

            var response = client.Execute(request);

            Assert.That(response.IsSuccessStatusCode, Is.True);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));
        }

        [Test, Order(5)]
        public void Test_CreateNewStory_WithoutRequiedFields_ShouldReturnBadRequest()
        {
            var requestBody = new StoryDTO
            {
                Url = ""
            };
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(requestBody);

            var response = client.Execute(request);

            Assert.That(response.IsSuccessStatusCode, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void Test_EditNonExistingStory_ShouldReturnNotFound()
        {
            var falseStoryid = "123";

            var editedBody = new StoryDTO
            {
                Title = "Edited Story Title",
                Description = "Some Edited Description",
                Url = ""
            };
            var request = new RestRequest($"/api/Story/Edit/{falseStoryid}", Method.Put);
            request.AddJsonBody(editedBody);

            var response = client.Execute(request);

            Assert.That(response.IsSuccessStatusCode, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No spoilers..."));
        }

        [Test, Order(7)]
        public void Test_DeleteNonExistingStory_ShouldReturnBadRequest()
        {
            var falseStoryid = "123";

            var request = new RestRequest($"/api/Story/Delete/{falseStoryid}", Method.Delete);

            var response = client.Execute(request);

            Assert.That(response.IsSuccessStatusCode, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler!"));
        }

        [OneTimeTearDown]
        public void Clear()
        {
            client?.Dispose();
        }
    }
}
