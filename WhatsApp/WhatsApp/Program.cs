using System.Text;
using Npgsql;

namespace WhatsApp;


class Program
{
    static async Task Main(string[] args)
    {
        string url =
            "https://1103.api.green-api.com/waInstance1103947345/sendMessage/8d9e1f0dd0d54a4d9ba23d9e9f8959c21ea56814176d4954aa";
        string connString = "Host=178.208.81.134; Port=5432; Username=postgres; Password=password; Database=postgres;";

        using (var conn = new NpgsqlConnection(connString))
        {
            var conn2 = new NpgsqlConnection(connString);
            var connwrite2 = new NpgsqlConnection(connString);
            var connwrite = new NpgsqlConnection(connString);
            connwrite.Open();
            conn.Open();
            conn2.Open();
            connwrite2.Open();

            while (true)
            {
                var sqlCommand = "SELECT id, name, phone FROM bookingtable WHERE message is null";
                var command = new NpgsqlCommand(sqlCommand, conn);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = reader["name"].ToString();
                        var phone = reader["phone"].ToString();
                        var id = reader["id"].ToString();

                        if (!string.IsNullOrEmpty(phone))
                        {
                            string json = $@"{{
                            ""chatId"": ""{phone}@c.us"",
                            ""message"": ""Здравствуйте, {name}, Вы забронировали номер.""
                        }}";

                            HttpClient client = new HttpClient();
                            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                            content.Headers.ContentType =
                                new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                            // Send POST request
                            HttpResponseMessage response = await client.PostAsync(url, content);

                            // Handle response
                            if (response.IsSuccessStatusCode)
                            {
                                string responseContent = await response.Content.ReadAsStringAsync();
                                Console.WriteLine($"Message sent to {name} ({phone})");
                                Console.WriteLine(responseContent);

                                var cmd = new NpgsqlCommand($"UPDATE bookingtable SET message = '1' WHERE id = '{id}'",
                                    connwrite);
                                cmd.ExecuteNonQuery();

                            }
                            else
                            {
                                Console.WriteLine(
                                    $"Failed to send message to {name} ({phone}), status code: {response.StatusCode}");
                            }
                        }

                        await Task.Delay(1000);
                    }
                }

                var sqlCommand2 = "SELECT id, name, date, phone FROM bookingtable WHERE message2 is null";
                var command2 = new NpgsqlCommand(sqlCommand2, conn2);
                using (var reader2 = command2.ExecuteReader())
                {
                    while (reader2.Read())
                    {
                        var id2 = reader2["id"].ToString();
                        var name2 = reader2["name"].ToString();
                        var dateString = reader2["date"].ToString();
                        var phone2 = reader2["phone"].ToString();

                        if (!string.IsNullOrEmpty(phone2) && DateTime.TryParse(dateString, out var date))
                        {
                            var currentTimeUtcPlus8 = DateTime.UtcNow.AddHours(8);
                            if (date <= currentTimeUtcPlus8)
                            {
                                string json = $@"{{
                            ""chatId"": ""{phone2}@c.us"",
                            ""message"": ""Здравствуйте, {name2}, Вы выехали из номера, вам всё понравилось?""
                        }}";
                                HttpClient client = new HttpClient();
                                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                                HttpResponseMessage response = await client.PostAsync(url, content);

                                if (response.IsSuccessStatusCode)
                                {
                                    string responseContent = await response.Content.ReadAsStringAsync();
                                    Console.WriteLine($"Message sent to {name2} ({phone2})");
                                    Console.WriteLine(responseContent);

                                    var updateCmd = new NpgsqlCommand(
                                        $"UPDATE bookingtable SET message2 = '1' WHERE id = '{id2}'", connwrite2);
                                    updateCmd.ExecuteNonQuery();
                                }
                                else
                                {
                                    Console.WriteLine(
                                        $"Failed to send message to {name2} ({phone2}), status code: {response.StatusCode}");
                                }
                            }
                        }

                        await Task.Delay(1000);
                    }
                }
            }
        }
    }
}

