using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace ScribanLiveEditor
{
    public class DatabaseManager
    {
        private const string ConnectionString = "Data Source=ScribanTemplates.db";

        public DatabaseManager()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Templates (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        TemplateText TEXT NOT NULL,
                        JsonText TEXT NOT NULL
                    )";
                command.ExecuteNonQuery();
            }
        }

        public void SaveTemplate(string name, string templateText, string jsonText)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT OR REPLACE INTO Templates (Name, TemplateText, JsonText)
                    VALUES (@name, @templateText, @jsonText)";
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@templateText", templateText);
                command.Parameters.AddWithValue("@jsonText", jsonText);
                command.ExecuteNonQuery();
            }
        }

        public (string TemplateText, string JsonText) LoadTemplate(string name)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT TemplateText, JsonText FROM Templates WHERE Name = @name";
                command.Parameters.AddWithValue("@name", name);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return (reader.GetString(0), reader.GetString(1));
                    }
                }
            }
            return (null, null);
        }

        public List<string> GetTemplateNames()
        {
            var names = new List<string>();
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Name FROM Templates";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        names.Add(reader.GetString(0));
                    }
                }
            }
            return names;
        }
    }
}
