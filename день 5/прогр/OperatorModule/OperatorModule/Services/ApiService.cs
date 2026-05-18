using Newtonsoft.Json;
using OperatorModule.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OperatorModule.Services
{
    public static class ApiService
    {
        private static HttpClient client = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:53383/api/")
        };

        public static string Token { get; set; }

        public static void SetAuthHeader()
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
        }

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

        // Auth
        public static async Task<AuthResponse> Login(LoginRequest request)
        {
            return await PostAsync<AuthResponse>("auth/login", request);
        }

        public static async Task<dynamic> Register(RegisterRequest request)
        {
            return await PostAsync<dynamic>("auth/register", request);
        }

        // Batches
        public static async Task<List<Batch>> GetBatches()
        {
            return await GetAsync<List<Batch>>("batches");
        }

        public static async Task<dynamic> GetBatchDetails(int id)
        {
            return await GetAsync<dynamic>($"batches/{id}/details");
        }

        public static async Task<Batch> StartBatch(int id, decimal? actualQuantity)
        {
            var req = new { actual_quantity_kg = actualQuantity, user_id = App.CurrentUser?.id };
            return await PostAsync<Batch>($"batches/{id}/start", req);
        }

        public static async Task<Batch> CompleteBatch(int id, decimal? actualQuantity)
        {
            var req = new { actual_quantity_kg = actualQuantity, user_id = App.CurrentUser?.id };
            return await PostAsync<Batch>($"batches/{id}/complete", req);
        }

        // Batch steps
        public static async Task<BatchStep> StartBatchStep(int stepId, int userId)
        {
            var req = new { user_id = userId };
            return await PostAsync<BatchStep>($"batch-steps/{stepId}/start", req);
        }

        public static async Task<BatchStep> FinishBatchStep(int stepId, int userId, decimal? actualTemp, decimal? actualPressure, int? actualDuration, string comment)
        {
            var req = new
            {
                user_id = userId,
                actual_temp_c = actualTemp,
                actual_pressure_bar = actualPressure,
                actual_duration_min = actualDuration,
                operator_comment = comment
            };
            return await PostAsync<BatchStep>($"batch-steps/{stepId}/finish", req);
        }

        public static async Task UpdateBatchStep(int stepId, BatchStep step)
        {
            await PutAsync($"batch_steps/{stepId}", step);
        }

        // Tech map steps
        public static async Task<List<TechMapStep>> GetTechMapSteps(int techMapId)
        {
            // Используем фильтр на стороне клиента (все шаги, потом фильтруем)
            var all = await GetAsync<List<TechMapStep>>("tech_map_steps");
            return all.FindAll(s => s.tech_map_id == techMapId);
        }

        // --- Справочники для регистрации ---
        public static async Task<List<Role>> GetRoles()
        {
            return await GetAsync<List<Role>>("roles");
        }

        public static async Task<List<Department>> GetDepartments()
        {
            return await GetAsync<List<Department>>("departments");
        }
    }
}