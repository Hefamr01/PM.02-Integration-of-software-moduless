using NUnit.Framework;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;

namespace ApiIntegrationTests
{
    public class ApiTests
    {
        private HttpClient _client;
        private string _authToken;

        private string username = "a";
        private string password = "a";

        [OneTimeSetUp]
        public void Setup()
        {
            _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:53383/api/") };
        }

        [Test]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            var loginData = new { username = username, password = password };
            var content = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("auth/login", content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var json = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(json);
            bool a = result.success;
            Assert.That(a, Is.True);
            Assert.That(result.token, Is.Not.Null);
            _authToken = result.token;
        }

        [Test]
        public async Task CreateProduct_ThenGet_ReturnsCreatedProduct()
        {
            var login = await _client.PostAsync("auth/login",
                new StringContent(JsonConvert.SerializeObject(new { username = username, password = password }), Encoding.UTF8, "application/json"));
            var loginJson = await login.Content.ReadAsStringAsync();
            dynamic loginResult = JsonConvert.DeserializeObject(loginJson);
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", (string)loginResult.token);

            var newProduct = new { name = "Integration Product", type = "Test", form = "Liquid", status = "draft" };
            var postContent = new StringContent(JsonConvert.SerializeObject(newProduct), Encoding.UTF8, "application/json");
            var postResponse = await _client.PostAsync("products", postContent);
            Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var productJson = await postResponse.Content.ReadAsStringAsync();
            dynamic created = JsonConvert.DeserializeObject(productJson);
            int productId = created.id;

            var getResponse = await _client.GetAsync($"products/{productId}");
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task CreateRecipe_WithInvalidProductId_ReturnsBadRequest()
        {
            await AuthorizeAs(username, password);
            var recipe = new { product_id = 99999, version = 1, status = "draft" };
            var content = new StringContent(JsonConvert.SerializeObject(recipe), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("recipes", content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var text = await response.Content.ReadAsStringAsync();
            Assert.That(text, Does.Contain("не найден"));
        }

        [Test]
        public async Task ActivateRecipe_WhenComponentsSumNot100_ReturnsBadRequest()
        {
            await AuthorizeAs(username, password);
            var recipe = new { product_id = 1, version = 99, status = "draft" };
            var postResp = await _client.PostAsync("recipes", new StringContent(JsonConvert.SerializeObject(recipe), Encoding.UTF8, "application/json"));
            dynamic recipeObj = JsonConvert.DeserializeObject(await postResp.Content.ReadAsStringAsync());
            int recipeId = recipeObj.id;

            var comp = new { recipe_id = recipeId, raw_material_id = 1, percentage = 50, load_order = 1 };
            await _client.PostAsync("recipe_components", new StringContent(JsonConvert.SerializeObject(comp), Encoding.UTF8, "application/json"));

            var activateResp = await _client.PostAsync($"recipes/{recipeId}/activate", new StringContent("{}", Encoding.UTF8, "application/json"));
            Assert.That(activateResp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task CreateBatch_WithNonExistentOrder_ReturnsBadRequest()
        {
            await AuthorizeAs(username, password);
            var batch = new { batch_number = "INT-TEST-001", order_id = 99999, recipe_id = 1, tech_map_id = 1, status = "planned" };
            var content = new StringContent(JsonConvert.SerializeObject(batch), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("batches", content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        private async Task AuthorizeAs(string username, string password)
        {
            var loginData = new { username, password };
            var content = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("auth/login", content);
            var json = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(json);
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", (string)result.token);
        }

        [OneTimeTearDown]
        public void Dispose()
        {
            _client.Dispose();
        }
    }
}