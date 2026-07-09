using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

using MyAvaloniaApp.Models;

namespace MyAvaloniaApp;

public static class DbHelper
{
    // Путь к файлу базы данных
    private static readonly string ConnectionString = "Data Source=maindb.db";

    static DbHelper()
    {
        Console.WriteLine("DbHelper constructor: Инициализация БД...");
        
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string createTableSql = @"
            CREATE TABLE IF NOT EXISTS Instruments (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                TypeCode INTEGER NOT NULL DEFAULT 0,
                Model TEXT,
                SerialNumber TEXT,
                InventoryNumber TEXT,
                IntervalYears INTEGER,
                Location TEXT,
                InServiceDate DATE,
                
                -- ИЗМЕНЕНИЕ: Units теперь INTEGER (код из Enum)
                Units INTEGER NOT NULL DEFAULT 0, 
                
                LowerLimit REAL,
                UpperLimit REAL,
                AccuracyClass REAL,
                VariationLimit REAL,
                AccuracyMethodCode INTEGER NOT NULL DEFAULT 0
            );";

        using var cmd = new SqliteCommand(createTableSql, conn);
        cmd.ExecuteNonQuery();
        
        Console.WriteLine("БД готова!");
    }

    public static void Initialize() { /* Пустой метод, нужен только чтобы вызвать статический конструктор */ }

    public static void SaveInstrument(InstrumentModel instrument)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            const string sql = @"
                INSERT INTO Instruments (
                    TypeCode, Model, SerialNumber, InventoryNumber, IntervalYears, Location, InServiceDate,
                    Units, LowerLimit, UpperLimit, AccuracyClass, VariationLimit, AccuracyMethodCode
                ) VALUES (
                    @TypeCode, @Model, @SerialNumber, @InventoryNumber, @IntervalYears, @Location, @InServiceDate,
                    @Units, @LowerLimit, @UpperLimit, @AccuracyClass, @VariationLimit, @AccuracyMethodCode
                );";

            using var cmd = new SqliteCommand(sql, conn);

            cmd.Parameters.AddWithValue("@TypeCode", instrument.TypeCode);
            cmd.Parameters.AddWithValue("@Model", (object?)instrument.Model ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SerialNumber", (object?)instrument.SerialNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@InventoryNumber", (object?)instrument.InventoryNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IntervalYears", (object?)instrument.IntervalYears ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Location", (object?)instrument.Location ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@InServiceDate", instrument.InServiceDate.ToString("yyyy-MM-dd"));

            // Units — строка: кПа, МПа, Па и т.д.
            cmd.Parameters.AddWithValue("@Units", (object?)instrument.Units ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@LowerLimit", (object?)instrument.LowerLimit ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UpperLimit", (object?)instrument.UpperLimit ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AccuracyClass", (object?)instrument.AccuracyClass ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@VariationLimit", (object?)instrument.VariationLimit ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AccuracyMethodCode", instrument.AccuracyMethodCode);

            cmd.ExecuteNonQuery();
        }

    public static List<InstrumentModel> GetAllInstruments()
    {
        var list = new List<InstrumentModel>();
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sql = "SELECT * FROM Instruments ORDER BY Id DESC";
        using var cmd = new SqliteCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(new InstrumentModel
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                TypeCode = reader.GetInt32(reader.GetOrdinal("TypeCode")),
                Model = reader.IsDBNull(reader.GetOrdinal("Model")) ? null : reader.GetString(reader.GetOrdinal("Model")),
                SerialNumber = reader.IsDBNull(reader.GetOrdinal("SerialNumber")) ? null : reader.GetString(reader.GetOrdinal("SerialNumber")),
                InventoryNumber = reader.IsDBNull(reader.GetOrdinal("InventoryNumber")) ? null : reader.GetString(reader.GetOrdinal("InventoryNumber")),
                IntervalYears = reader.IsDBNull(reader.GetOrdinal("IntervalYears")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("IntervalYears")),
                Location = reader.IsDBNull(reader.GetOrdinal("Location")) ? null : reader.GetString(reader.GetOrdinal("Location")),
                
                // Важно: SQLite хранит дату как строку "yyyy-MM-dd", ParseExact превратит её обратно в DateTime
                InServiceDate = reader.IsDBNull(reader.GetOrdinal("InServiceDate")) 
                    ? DateTime.Today 
                    : DateTime.ParseExact(reader.GetString(reader.GetOrdinal("InServiceDate")), "yyyy-MM-dd", null),

                Units = reader.IsDBNull(reader.GetOrdinal("Units")) ? null : reader.GetString(reader.GetOrdinal("Units")),
                LowerLimit = reader.IsDBNull(reader.GetOrdinal("LowerLimit")) ? (double?)null : reader.GetDouble(reader.GetOrdinal("LowerLimit")),
                UpperLimit = reader.IsDBNull(reader.GetOrdinal("UpperLimit")) ? (double?)null : reader.GetDouble(reader.GetOrdinal("UpperLimit")),
                AccuracyClass = reader.IsDBNull(reader.GetOrdinal("AccuracyClass")) ? (double?)null : reader.GetDouble(reader.GetOrdinal("AccuracyClass")),
                VariationLimit = reader.IsDBNull(reader.GetOrdinal("VariationLimit")) ? (double?)null : reader.GetDouble(reader.GetOrdinal("VariationLimit")),
                AccuracyMethodCode = reader.GetInt32(reader.GetOrdinal("AccuracyMethodCode"))
            });
        }
        return list;
    }

    public static void DeleteInstrument(int id)
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sql = "DELETE FROM Instruments WHERE Id = @Id";
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);

        int rowsAffected = cmd.ExecuteNonQuery();
        if (rowsAffected == 0)
            throw new InvalidOperationException("Не удалось удалить: запись с таким Id не найдена.");
    }



}