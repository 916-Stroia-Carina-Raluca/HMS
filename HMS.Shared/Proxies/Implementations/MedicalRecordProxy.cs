using HMS.Shared.Entities;
using HMS.Shared.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HMS.Shared.Proxies.Implementations
{
    public class MedicalRecordProxy : IMedicalRecordRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = Config._base_api_url;
        private readonly string _token;
        private readonly JsonSerializerOptions _jsonOptions;

        public MedicalRecordProxy(HttpClient httpClient, string token)
        {
            this._httpClient = httpClient;
            this._token = token;
            this._jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        private void AddAuthorizationHeader()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this._token);
        }

        public async Task<MedicalRecord> AddAsync(MedicalRecord medicalRecord)
        {
            AddAuthorizationHeader();
            string recordJson = JsonSerializer.Serialize(medicalRecord, _jsonOptions);
            StringContent content = new StringContent(recordJson, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(_baseUrl + "medicalrecord", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<MedicalRecord>(json, _jsonOptions)!;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            AddAuthorizationHeader();
            HttpResponseMessage response = await _httpClient.DeleteAsync(_baseUrl + $"medicalrecord/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return false;

            response.EnsureSuccessStatusCode();
            return true;
        }

        private class RecordValues
        {
            [JsonPropertyName("$values")]
            public List<MedicalRecordDto> Values { get; set; } = new();
        }

        private class MedicalRecordResponse
        {
            [JsonPropertyName("$id")]
            public string Id { get; set; } = "";

            [JsonPropertyName("records")]
            public RecordValues Records { get; set; } = new();
        }

        private class MedicalRecordDto
        {
            [JsonPropertyName("$id")]
            public string RefId { get; set; } = "";
            public int Id { get; set; }
            public int PatientId { get; set; }
            public string PatientName { get; set; } = "";
            public int DoctorId { get; set; }
            public string DoctorName { get; set; } = "";
            public string DoctorDepartment { get; set; } = "";
            public int ProcedureId { get; set; }
            public string ProcedureName { get; set; } = "";
            public string ProcedureDepartment { get; set; } = "";
            public string Diagnosis { get; set; } = "";
            public DateTime CreatedAt { get; set; }
        }

        public async Task<IEnumerable<MedicalRecord>> GetAllAsync()
        {
            AddAuthorizationHeader();
            HttpResponseMessage response = await _httpClient.GetAsync(_baseUrl + "medicalrecord");
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<MedicalRecordResponse>(responseBody, _jsonOptions);

            if (result?.Records?.Values == null)
            {
                return new List<MedicalRecord>();
            }

            var records = new List<MedicalRecord>();
            foreach (var dto in result.Records.Values)
            {
                var record = new MedicalRecord
                {
                    Id = dto.Id,
                    PatientId = dto.PatientId,
                    Patient = new Patient { Id = dto.PatientId, Name = dto.PatientName },
                    DoctorId = dto.DoctorId,
                    Doctor = new Doctor { Id = dto.DoctorId, Name = dto.DoctorName, Department = new Department { Name = dto.DoctorDepartment } },
                    ProcedureId = dto.ProcedureId,
                    Procedure = new Procedure { Id = dto.ProcedureId, Name = dto.ProcedureName, Department = new Department { Name = dto.ProcedureDepartment } },
                    Diagnosis = dto.Diagnosis,
                    CreatedAt = dto.CreatedAt
                };
                records.Add(record);
            }

            return records;
        }

        public async Task<MedicalRecord?> GetByIdAsync(int id)
        {
            AddAuthorizationHeader();
            HttpResponseMessage response = await _httpClient.GetAsync(_baseUrl + $"medicalrecord/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<MedicalRecord>(responseBody, _jsonOptions);
        }

        public async Task<bool> UpdateAsync(MedicalRecord medicalRecord)
        {
            AddAuthorizationHeader();
            string recordJson = JsonSerializer.Serialize(medicalRecord, _jsonOptions);
            StringContent content = new StringContent(recordJson, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PutAsync(_baseUrl + $"medicalrecord/{medicalRecord.Id}", content);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return false;

            response.EnsureSuccessStatusCode();
            return true;
        }
    }
} 