using System;
using System.Collections.Generic;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace GPTConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var clientResponses = new List<ClientResponse>
            {
                new ClientResponse(1, "Было супер-пупер!", "Ответь-это положительный отзыв? Ответь-это отрицательный отзыв?"),
                new ClientResponse(2, "Это было ужасно!", "Ответь-это положительный отзыв? Ответь-это отрицательный отзыв?"),
                 new ClientResponse(2, "мне неособо понравилось!", "Ответь-это положительный отзыв? Ответь-это отрицательный отзыв?"),
               
            };

            string apiKey = "AQVNxE47XlcYBBqyaLEl3hJ00F9-IF1HA5nnESh5";
            string apiUrl = "https://llm.api.cloud.yandex.net/foundationModels/v1/completion"; // Обновите этот URL на правильный

            // Обработка данных и генерация ответов
            foreach (var clientResponse in clientResponses)
            {
                clientResponse.Response = GetGPTResponse(apiKey, apiUrl, clientResponse.Question);
                Console.WriteLine($"Клиент {clientResponse.Id}: {clientResponse.Question}");
                Console.WriteLine($"Ответ: {clientResponse.Response}");
                Console.WriteLine();
            }
        }

        static string GetGPTResponse(string apiKey, string apiUrl, string prompt)
        {
            var client = new RestClient(apiUrl);
            var request = new RestRequest();
            request.Method = Method.Post;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", $"Api-Key {apiKey}");

            var requestBody = new
            {
                modelUri = "gpt://b1gkhfs9oevj7gfoikpu/yandexgpt-lite",
                completionOptions = new
                {
                    stream = false,
                    temperature = 0.4,
                    maxTokens = 2000
                },
                messages = new[]
                {
                    new { role = "system", text = "Ты - ассистент. Определи, является ли следующий отзыв положительным или отрицательным. Ответь только 'да' или 'нет'." },
                    new { role = "user", text = prompt }
                }
            };

            request.AddJsonBody(requestBody);

            var response = client.Execute(request);

            if (response.Content == null)
            {
                return "Не удалось получить ответ от модели GPT.";
            }

            try
            {
                var content = JObject.Parse(response.Content);
                Console.WriteLine("Response Content:");
                Console.WriteLine(response.Content);

                if (content["result"]?["alternatives"] != null && content["result"]["alternatives"].HasValues && content["result"]["alternatives"][0]["message"]?["text"] != null)
                {
                    string gptResponse = content["result"]["alternatives"][0]["message"]["text"]?.ToString().Trim() ?? "";

                    // Возвращаем ответ модели напрямую
                    if (gptResponse.Equals("да", StringComparison.OrdinalIgnoreCase) || gptResponse.Equals("нет", StringComparison.OrdinalIgnoreCase))
                    {
                        return gptResponse;
                    }
                    else
                    {
                        return "Не удалось определить ответ.";
                    }
                }
                else
                {
                    return "Не удалось определить ответ.";
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка при обработке ответа: {ex.Message}";
            }
        }
    }

    class ClientResponse
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public string Response { get; set; }

        public ClientResponse(int id, string question, string answer)
        {
            Id = id;
            Question = question;
            Answer = answer;
            Response = string.Empty; // Инициализация пустой строки
        }
    }
}