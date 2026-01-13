using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace Congratulator
{
    class DbContext
    {
        public string connection_string = "Host=localhost;Username=postgres;Password=1454;Database=birthdays;Port=5432";
    }
    internal class Program
    {
        static void ViewTodaysUpcomingBirthdays(DbContext db)
        {
            using (var connection = new NpgsqlConnection(db.connection_string))
            {
                connection.Open();
                var sql = "select * from birthdays;";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    Console.WriteLine("\t--- Список сегодняшних и ближайших дней рождения ---");
                    Console.WriteLine("{0, -5} {1, -20}", "ID", "Birthday");
                    Console.WriteLine(new string('-', 37));
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime YearOff(DateTime date) => new DateTime(2000, date.Month, date.Day);
                            int id = reader.GetInt32(0);
                            DateTime birthday = reader.GetDateTime(1);
                            if (YearOff(birthday) == YearOff(DateTime.Now) || YearOff(birthday) > YearOff(DateTime.Now) && YearOff(birthday) < YearOff(DateTime.Now.AddDays(31)))
                            {
                                Console.WriteLine(id + ", " + birthday);
                            }

                        }
                    }
                }
            }
        }
        static void ViewAllBirthDays(DbContext db)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(db.connection_string))
            {
                try
                {
                    connection.Open();
                    var sql = "select * from birthdays;";
                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            Console.WriteLine("\t--- Список всех дней рождения ---");
                            Console.WriteLine("{0, -5} {1, -20}", "ID", "Birthday");
                            Console.WriteLine(new string('-', 37));
                            while (reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                var birthday = reader.GetDateTime(1);
                                Console.WriteLine(id + ", " + birthday);
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    Console.Clear();
                    Console.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    Console.Clear();
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
        }
        static void Menu(DbContext db)
        {
            ViewTodaysUpcomingBirthdays(db);
            Console.WriteLine("\n  --- Меню ---");
            Console.WriteLine("1. Добавить день рождения");
            Console.WriteLine("2. Изменить день рождения");
            Console.WriteLine("3. Удалить день рождения");
            Console.WriteLine("4. Выйти");
            Console.WriteLine("\nВведите индекс нужного вам действия: ");
            int action = 0;
            try
            {
                var value = Console.ReadLine();
                action = int.Parse(value);
            }
            catch
            {
                Console.WriteLine("Неверно введен индекс. Попробуйте снова...");
                Console.ReadKey();
                Console.Clear();
                Menu(db);
            }
            switch (action)
            {
                case 1:
                    AddBirthDay(db);
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    Console.Clear();
                    Menu(db);
                    break;
                case 2:
                    ChangeBirthDay(db);
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    Console.Clear();    
                    Menu(db);
                    break;
                case 3:
                    DeleteBirthDay(db);
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    Console.Clear();
                    Menu(db);
                    break;
                case 4:
                    return;
            }
        }
        static void AddBirthDay(DbContext db)
        {
            Console.Clear();
            ViewAllBirthDays(db);
            Console.WriteLine("\nКакую дату вы хотите добавить?");
            Console.WriteLine("Вернуться - q");
            DateTime date;
            try
            {

                var value = Console.ReadLine();
                if (value == "q")
                {
                    return;
                }
                date = DateTime.Parse(value);
                using (var connection = new NpgsqlConnection(db.connection_string))
                {
                    try
                    {
                        connection.Open();
                        var sql = "insert into birthdays (birthday) values (@birthday);";
                        using (var command = new NpgsqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@birthday", date);
                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                Console.WriteLine($"Добавлена новая дата {date}");
                            }
                            else
                            {
                                Console.WriteLine("Ошибка добавления!");
                            }
                        }
                        connection.Close();
                    }
                    catch (NpgsqlException ex)
                    {
                        Console.Clear();
                        Console.WriteLine(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.Clear();
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        if (connection.State == System.Data.ConnectionState.Open)
                        {
                            connection.Close();
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Неверно введена дата. Попробуйте снова...");
                Console.ReadKey();
                AddBirthDay(db);
            }            
        }
        static void ChangeBirthDay(DbContext db)
        {
            Console.Clear();
            ViewAllBirthDays(db);
            Console.WriteLine("\nВведите индекс даты, которую вы хотите изменить:");
            Console.WriteLine("Вернуться - q");
            int id;
            DateTime date;
            var value1 = Console.ReadLine();
            if (value1 == "q")
            {
                return;
            }
            else
            {
                try
                {
                    id = int.Parse(value1);
                    Console.WriteLine("Введите новое значение даты:");
                    try
                    {
                        var value2 = Console.ReadLine();
                        date = DateTime.Parse(value2);
                        using (var connection = new NpgsqlConnection(db.connection_string))
                        {
                            try
                            {
                                connection.Open();
                                var sql = "update birthdays set birthday = @birthday where id = @condition";
                                using (var command = new NpgsqlCommand(sql, connection))
                                {
                                    command.Parameters.AddWithValue("@birthday", date);
                                    command.Parameters.AddWithValue("@condition", id);
                                    var rowsaffected = command.ExecuteNonQuery();
                                    if (rowsaffected != 0)
                                    {
                                        Console.WriteLine($"Дата успешно изменена на {date} !");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Ошибка изменения!");
                                    }
                                }
                            }
                            catch (NpgsqlException ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            finally
                            {
                                if (connection.State == System.Data.ConnectionState.Open)
                                {
                                    connection.Close();
                                }
                            }
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Неверно введена дата. Попробуйте снова...");
                        Console.ReadKey();
                        ChangeBirthDay(db);
                    }
                }
                catch
                {
                    Console.WriteLine("Неверно введен индекс. Попробуйте снова...");
                    Console.ReadKey();
                    ChangeBirthDay(db);
                }
            }                    
        }
        static void DeleteBirthDay(DbContext db)
        {
            Console.Clear();
            ViewAllBirthDays(db);
            Console.WriteLine("\nВведите индекс даты, которую вы хотите удалить:");
            Console.WriteLine("Вернуться - q");
            int id;
            try
            {
                var value = Console.ReadLine();
                if (value == "q")
                {
                    return;
                }
                id = int.Parse(value);
                using (var connection = new NpgsqlConnection(db.connection_string))
                {
                    connection.Open();
                    var sql = "delete from birthdays where id = @condition";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@condition", id);
                        var rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected != 0)
                        {
                            Console.WriteLine("Дата успешно удалена!");
                        }
                        else
                        {
                            Console.WriteLine("Ошибка удаления!");
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Неверно введен индекс. Попробуйте снова...");
                Console.ReadKey();
                DeleteBirthDay(db);
            }            
        }
        static void Main()
        {
            DbContext db = new DbContext();
            Menu(db);
        }
    }
}