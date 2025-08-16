using Story_spoiler.Models;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;


namespace Story_spoiler
{
    [TestFixture]
    public class StorySpoilersTests
    {
        private RestClient client;
        private static string createdStoryId;
        private const string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net/api";


        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("va123", "123456");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };
            client = new RestClient(options);
        }
        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString();
        }

        [Test, Order(1)]
        public void CreateNewStoryWithRequiredFields_ReturnOk()
        {
            var story = new
            {
                Title = "new story",
                Description = "New amazing story",
                Url = ""
            };
            var request = new RestRequest("/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            createdStoryId = json.GetProperty("storyId").GetString() ?? string.Empty;

            Assert.That(createdStoryId, Is.Not.Null.And.Not.Empty);
            Assert.That(response.Content, Does.Contain("Successfully created!"));
        }

        [Test, Order(2)]
        public void EditCreatedStory_ShouldReturnOk()
        {
            var editRequest = new StoryDTO
            {
                Title = "Edited Story",
                Description = "This is an updated test story description",
                Url = ""
            };
            var request = new RestRequest($"/Story/Edit/{createdStoryId}", Method.Put);
            request.AddQueryParameter("storyId", createdStoryId);
            request.AddJsonBody(editRequest);
            var response = this.client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(editResponse.Msg, Is.EqualTo("Successfully edited"));
        }

        [Test, Order(3)]
        public void GetAllStories_ShouldReturnList()
        {
            var request = new RestRequest("/Story/All", Method.Get);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var stories = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);
            Assert.That(stories, Is.Not.Empty);

        }

        [Test, Order(4)]
        public void DeleteStory_ShouldReturnOk() 
        {
            var request = new RestRequest($"/Story/Delete/{createdStoryId}", Method.Delete);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var story = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(story.Msg, Is.EqualTo("Deleted successfully!"));
        }

        [Test, Order(5)]
        public void CreateStory_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var story = new
            {
                Title = "new title",
                Description = ""
            };
            var request = new RestRequest("/Story/Create", Method.Post);
            request.AddJsonBody(story);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditNonExistingStory_ShouldReturnNotFound()
        {
            string nonExistingStoryId = "123";
            var editRequest = new StoryDTO
            {
                Title = "Edited Non-Existing Idea",
                Description = "This is an updated test idea description for a non-existing idea.",
                Url = ""
            };
            var request = new RestRequest($"/Story/Edit/{nonExistingStoryId}", Method.Put);
            request.AddQueryParameter("storyId", nonExistingStoryId);
            request.AddJsonBody(editRequest);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No spoilers..."));
        }

        [Test, Order(7)]
        public void DeleteNotExistingStory_ShouldReturnBadRequest()
        {
            string nonExistingStoryId = "123";
            var request = new RestRequest($"/Story/Delete/{nonExistingStoryId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler!"));
        }


        [OneTimeTearDown]
        public void Teardown()
        {
            client?.Dispose();
        }
    }
}