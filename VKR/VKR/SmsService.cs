using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VKR
{
    public class SmsService
    {
        private readonly HttpClient _httpClient;

        public SmsService(string apiKey)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://platform.clickatell.com/v1/")
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<bool> SendMessagesAsync(List<Message> messages)
        {
            var body = new
            {
                messages
            };

            var jsonBody = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("message", content);

            return response.IsSuccessStatusCode;
        }
    }
}