using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LabModule.Models;

namespace LabModule.Services
{
    public static class ApiService
    {
        private static HttpClient client = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:53383/api/")   // измените при необходимости
        };

        public static string Token { get; set; }

        public static void SetAuthHeader()
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
        }

        // Обобщённые методы
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

        // --- Партии сырья ---
        public static async Task<List<RawMaterialBatch>> GetRawMaterialBatches()
        {
            return await GetAsync<List<RawMaterialBatch>>("raw_material_batches");
        }

        public static async Task<RawMaterialBatch> GetRawMaterialBatch(int id)
        {
            return await GetAsync<RawMaterialBatch>($"raw_material_batches/{id}");
        }

        public static async Task<RawMaterialBatch> CreateRawMaterialBatch(RawMaterialBatch batch)
        {
            return await PostAsync<RawMaterialBatch>("raw_material_batches", batch);
        }

        public static async Task UpdateRawMaterialBatch(int id, RawMaterialBatch batch)
        {
            await PutAsync($"raw_material_batches/{id}", batch);
        }

        public static async Task<RawMaterialBatch> ApproveRawMaterialBatch(int id)
        {
            return await PostAsync<RawMaterialBatch>($"raw_material_batches/{id}/approve", new { });
        }

        public static async Task<RawMaterialBatch> BlockRawMaterialBatch(int id)
        {
            return await PostAsync<RawMaterialBatch>($"raw_material_batches/{id}/block", new { });
        }

        // --- Партии готовой продукции ---
        public static async Task<List<Batch>> GetBatches()
        {
            return await GetAsync<List<Batch>>("batches");
        }

        public static async Task<Batch> GetBatch(int id)
        {
            return await GetAsync<Batch>($"batches/{id}");
        }

        // --- Контроль качества ---
        public static async Task<List<QualityControl>> GetQualityControls()
        {
            return await GetAsync<List<QualityControl>>("quality_controls");
        }

        public static async Task<QualityControl> CreateQualityControl(QualityControl qc)
        {
            return await PostAsync<QualityControl>("quality_controls", qc);
        }

        public static async Task<QualityControl> SetQualityControlDecision(int id, int analystId, string decision, string comment)
        {
            var req = new { analyst_id = analystId, decision = decision, analyst_comment = comment };
            await PutAsync($"quality_controls/{id}/decision", req);
            return await GetQualityControl(id);
        }

        public static async Task<QualityControl> GetQualityControl(int id)
        {
            return await GetAsync<QualityControl>($"quality_controls/{id}");
        }

        // --- Вспомогательные методы для получения связанных данных ---
        public static async Task<List<QualityControl>> GetQualityControlsForRawMaterialBatch(int rawMaterialBatchId)
        {
            var all = await GetQualityControls();
            return all.FindAll(qc => qc.raw_material_batch_id == rawMaterialBatchId);
        }

        public static async Task<List<QualityControl>> GetQualityControlsForBatch(int batchId)
        {
            var all = await GetQualityControls();
            return all.FindAll(qc => qc.batch_id == batchId);
        }

        // --- Сырьё (справочник) ---
        public static async Task<List<RawMaterial>> GetRawMaterials()
        {
            return await GetAsync<List<RawMaterial>>("raw_materials");
        }

        // --- Аудит ---
        public static async Task<AuditLog> CreateAuditLog(AuditLog log)
        {
            return await PostAsync<AuditLog>("audit_log", log);
        }
    }
}