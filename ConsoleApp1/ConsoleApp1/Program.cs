using System;
using Npgsql;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ConsoleApp1;

public class Booking
{
    public string id { get; set; }
    public string name { get; set; }
    public string date { get; set; }
    public string phone { get; set; }
}

class Reader
{
    static void ClearDatabase(string connString) //очищение бд
    {
        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();
            using (var cmd = new NpgsqlCommand("DELETE FROM bookingtable", conn))
            {
                cmd.ExecuteNonQuery();
            }

            conn.Close();
        }
    }

    static string FormatPhoneNumber(string phone) //форматирование номеров
    {
        // Удаление всех символов, кроме цифр
        phone = Regex.Replace(phone, @"\D", "");

        // Если первая цифра 8, заменяем на 7
        if (phone.StartsWith("8"))
        {
            phone = "7" + phone.Substring(1);
        }

        // оставляем 11 цифр номера
        if (phone.Length > 11)
        {
            phone = phone.Substring(0, 11);
        }

        //обнуление, если меньше 11 цифр
        else if (phone.Length < 11)
        {
            phone = "0";
        }

        return phone;
    }

    static string FormatName(string name) //форматирование имен
    {
        name = Regex.Replace(name, "[^а-яА-Яa-zA-Z ]", ""); //убрать все символы кроме букв и пробелов
        if (!string.IsNullOrEmpty(name) && name[0] == ' ') //убрать пробел в начале
        {
            name = name.Substring(1);
        }

        return name;
    }

    static void Main()
    {
        string connString = "Host=178.208.81.134; Port=5432; Username=postgres; Password=password; Database=postgres;";
        string csvfile = @"C:\Users\ROOT\Desktop\test1.csv";
        var listbooking = new List<Booking>();
        //ClearDatabase(connString);

        fastCSV.ReadFile<Booking>( //чтение csv файла
            csvfile, // filename
            true, // has header
            ',', // delimiter
            (o, c) => // to object function o : cars object, c : columns array read
            {
                var booking = new Booking();
                booking.id = c[0];
                booking.name = FormatName(c[10]);
                booking.date = c[6];
                booking.phone = FormatPhoneNumber(c[13]);
                ;
                listbooking.Add(booking);
                return true;
            });
        foreach (var booking in listbooking)

        {
            Console.WriteLine($"ID: {booking.id}, Name: {booking.name}, Date: {booking.date}, Phone: {booking.phone}");
        }

        using (var conn = new NpgsqlConnection(connString)) //перенос в бд
        {
            conn.Open();
            foreach (var booking in listbooking)
            {
                using (var cmd = new NpgsqlCommand(
                           "INSERT INTO bookingtable (id, name, date, phone) VALUES (@id, @name, @date, @phone)", conn))
                {
                    //DateTime date = DateTime.ParseExact(booking.date, "dd.M.yyyy HH:mm",
                       // System.Globalization.CultureInfo.InvariantCulture);
                    cmd.Parameters.AddWithValue("id", booking.id);
                    cmd.Parameters.AddWithValue("name", booking.name);
                    cmd.Parameters.AddWithValue("date", booking.date);
                    cmd.Parameters.AddWithValue("phone", booking.phone);
                    cmd.ExecuteNonQuery();
                }
            }
            conn.Close();
        }
    }
}