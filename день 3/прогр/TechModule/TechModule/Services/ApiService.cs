using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TechModule.Models;

namespace TechModule.Services
{
    public static class ApiService
    {
        private static HttpClient client = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:53383/api/")   // !!! замените на свой адрес
        };

        public static string Token { get; set; }

        public static void SetAuthHeader()
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
        }

        // обобщённые методы для GET/POST/PUT
        public static async Task<T> GetAsync<T>(string url)
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static async Task<T> PostAsync<T>(string url, object data)
        {
            var jsonContent = new StringContent(JsonConvert.SerializeObject(data),
                Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, jsonContent);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static async Task PutAsync(string url, object data)
        {
            var jsonContent = new StringContent(JsonConvert.SerializeObject(data),
                Encoding.UTF8, "application/json");
            var response = await client.PutAsync(url, jsonContent);
            response.EnsureSuccessStatusCode();
        }

        public static async Task DeleteAsync(string url)
        {
            var response = await client.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
        }

        // Активация рецепта
        public static async Task<T> ActivateRecipeAsync<T>(int recipeId)
        {
            return await PostAsync<T>($"recipes/{recipeId}/activate", new { });
        }

        // Архивация рецепта
        public static async Task<T> ArchiveRecipeAsync<T>(int recipeId)
        {
            return await PostAsync<T>($"recipes/{recipeId}/archive", new { });
        }

        // Активация техкарты
        public static async Task<T> ActivateTechMapAsync<T>(int mapId)
        {
            return await PostAsync<T>($"tech-maps/{mapId}/activate", new { });
        }

        // Архивация техкарты
        public static async Task<T> ArchiveTechMapAsync<T>(int mapId)
        {
            return await PostAsync<T>($"tech-maps/{mapId}/archive", new { });
        }

        // Получить рецепт с компонентами
        public static async Task<(Recipe recipe, List<RecipeComponent> components)> GetRecipeFullAsync(int recipeId)
        {
            var result = await GetAsync<dynamic>($"recipes/{recipeId}/full");
            Recipe recipe = JsonConvert.DeserializeObject<Recipe>(result.recipe.ToString());
            var components = JsonConvert.DeserializeObject<List<RecipeComponent>>(result.components.ToString());
            return (recipe, components);
        }

        // Получить техкарту с шагами
        public static async Task<(TechMap map, List<TechMapStep> steps)> GetTechMapFullAsync(int mapId)
        {
            var result = await GetAsync<dynamic>($"tech-maps/{mapId}/full");
            TechMap map = JsonConvert.DeserializeObject<TechMap>(result.map.ToString());
            var steps = JsonConvert.DeserializeObject<List<TechMapStep>>(result.steps.ToString());
            return (map, steps);
        }

        // Старт партии
        public static async Task<T> StartBatchAsync<T>(int batchId, int userId, decimal? actualQuantity = null)
        {
            var req = new { user_id = userId, actual_quantity_kg = actualQuantity };
            return await PostAsync<T>($"batches/{batchId}/start", req);
        }

        // Завершение партии
        public static async Task<T> CompleteBatchAsync<T>(int batchId, int userId, decimal? actualQuantity = null)
        {
            var req = new { user_id = userId, actual_quantity_kg = actualQuantity };
            return await PostAsync<T>($"batches/{batchId}/complete", req);
        }

        // Старт шага партии
        public static async Task<T> StartBatchStepAsync<T>(int stepId, int userId)
        {
            var req = new { user_id = userId };
            return await PostAsync<T>($"batch-steps/{stepId}/start", req);
        }

        // Завершение шага партии
        public static async Task<T> FinishBatchStepAsync<T>(int stepId, int userId, decimal? actualTemp, decimal? actualPressure, int? actualDuration, string comment)
        {
            var req = new { user_id = userId, actual_temp_c = actualTemp, actual_pressure_bar = actualPressure, actual_duration_min = actualDuration, operator_comment = comment };
            return await PostAsync<T>($"batch-steps/{stepId}/finish", req);
        }

        // Получить детали партии (шаги и контроль качества)
        public static async Task<dynamic> GetBatchDetailsAsync(int batchId)
        {
            return await GetAsync<dynamic>($"batches/{batchId}/details");
        }

        // ApiService.cs - добавить эти методы
        public static async Task<Batch> CreateBatchAsync(Batch batch)
        {
            return await PostAsync<Batch>("batches", batch);
        }

        public static async Task<List<Recipe>> GetActiveRecipesAsync()
        {
            var all = await GetAsync<List<Recipe>>("recipes");
            return all.Where(r => r.status == "active").ToList();
        }

        public static async Task<List<TechMap>> GetActiveTechMapsAsync()
        {
            var all = await GetAsync<List<TechMap>>("tech_maps");
            return all.Where(t => t.status == "active").ToList();
        }
    }
}