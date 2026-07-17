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

        const string createInspectorsTableSql = @"
            CREATE TABLE IF NOT EXISTS Inspectors (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FirstName TEXT NOT NULL,
                LastName TEXT NOT NULL,
                MiddleName TEXT
            );";

        using var cmdInspectors = new SqliteCommand(createInspectorsTableSql, conn);
        cmdInspectors.ExecuteNonQuery();

        const string createTemplateTableSql = @"
            CREATE TABLE IF NOT EXISTS Templates (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                DeviceType TEXT,
                FullName TEXT,
                SerialNumber TEXT,
                Inaccuracy REAL,
                InaccuracyMethodCode INTEGER NOT NULL DEFAULT 0,
                CurrentRange TEXT,


                -- ИЗМЕНЕНИЕ: Units теперь INTEGER (код из Enum)
                Units INTEGER NOT NULL DEFAULT 0, 
                
                LowerLimit REAL,
                UpperLimit REAL
            );";

        using var cmdTemplate = new SqliteCommand(createTemplateTableSql, conn);
        cmdTemplate.ExecuteNonQuery();

        const string createVerificationTableSql = @"
            CREATE TABLE IF NOT EXISTS Verification (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Comment TEXT,
                AuthorId INTEGER,
                FOREIGN KEY (AuthorId) REFERENCES Inspectors(Id) ON DELETE SET NULL
            );";

        using var cmdVerification = new SqliteCommand(createVerificationTableSql, conn);
        cmdVerification.ExecuteNonQuery();

        const string createVerificationInstrumentTableSql = @"
            CREATE TABLE IF NOT EXISTS VerificationInstrument (
                VerificationId INTEGER NOT NULL,
                InstrumentId   INTEGER NOT NULL,
                Channel        INTEGER,                 -- номер канала прибора в этой поверке
                PRIMARY KEY (VerificationId, InstrumentId),
                FOREIGN KEY (VerificationId) REFERENCES Verification(Id) ON DELETE CASCADE,
                FOREIGN KEY (InstrumentId)   REFERENCES Instruments(Id)   ON DELETE CASCADE
            );";

        using var cmdVerificationInstrument = new SqliteCommand(createVerificationInstrumentTableSql, conn);
        cmdVerificationInstrument.ExecuteNonQuery();

        const string createProbingTableSql = @"
            CREATE TABLE IF NOT EXISTS Probing (
                Id                       INTEGER PRIMARY KEY AUTOINCREMENT,
                VerificationId           INTEGER NOT NULL,
                InstrumentId             INTEGER NOT NULL,

                -- Флаги результатов проверок
                ExternalInspection       BOOLEAN NOT NULL DEFAULT 0,
                Operability              BOOLEAN NOT NULL DEFAULT 0,
                ZeroSettingFunction      BOOLEAN NOT NULL DEFAULT 0,
                Tightness                BOOLEAN NOT NULL DEFAULT 0,

                -- Комментарии к проверкам
                ExternalInspectionComment   TEXT,
                OperabilityComment          TEXT,
                ZeroSettingFunctionComment  TEXT,
                TightnessComment            TEXT,

                FOREIGN KEY (VerificationId) REFERENCES Verification(Id) ON DELETE CASCADE,
                FOREIGN KEY (InstrumentId)   REFERENCES Instruments(Id)   ON DELETE CASCADE
            );";

        using var cmdProbing = new SqliteCommand(createProbingTableSql, conn);
        cmdProbing.ExecuteNonQuery();

        Console.WriteLine("БД готова!");
    }

    public static void Initialize() { /* Пустой метод, нужен только чтобы вызвать статический конструктор */ }

    public static int SaveVerification(string? comment)
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sql = @"
        INSERT INTO Verification (Comment)
        VALUES (@Comment);
        SELECT last_insert_rowid();";

        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Comment", (object?)comment ?? DBNull.Value);

        // Возвращаем ID новой поверки
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static List<VerificationModel> GetAllVerifications()
    {
        var list = new List<VerificationModel>();

        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sql = @"
        SELECT Id, Comment
        FROM Verification;";

        using var cmd = new SqliteCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(new VerificationModel
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Comment = reader.IsDBNull(reader.GetOrdinal("Comment"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Comment"))
            });
        }

        return list;
    }


    public static void AddInstrumentToVerification(int verificationId, int instrumentId, int? channel)
    {
        Console.WriteLine("AddInstrumentToVerification");
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sql = @"
        INSERT INTO VerificationInstrument (
            VerificationId,
            InstrumentId,
            Channel
        ) VALUES (
            @VerificationId,
            @InstrumentId,
            @Channel
        );";

        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@VerificationId", verificationId);
        cmd.Parameters.AddWithValue("@InstrumentId", instrumentId);
        cmd.Parameters.AddWithValue("@Channel", (object?)channel ?? DBNull.Value);

        cmd.ExecuteNonQuery();
    }

    public static void RemoveInstrumentFromVerification(int verificationId, int instrumentId)
    {
        Console.WriteLine($"RemoveInstrumentFromVerification: VerificationId={verificationId}, InstrumentId={instrumentId}");

        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sql = @"
        DELETE FROM VerificationInstrument
        WHERE VerificationId = @VerificationId
          AND InstrumentId   = @InstrumentId;";

        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@VerificationId", verificationId);
        cmd.Parameters.AddWithValue("@InstrumentId", instrumentId);

        int rowsAffected = cmd.ExecuteNonQuery();

        if (rowsAffected == 0)
        {
            Console.WriteLine("Запись не найдена — удалять нечего.");
        }
        else
        {
            Console.WriteLine($"Удалено строк: {rowsAffected}");
        }
    }



    public static void SaveTemplate(TemplateModel template)
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();
        
        const string sql = @"
                INSERT INTO Templates (
                    DeviceType, FullName, SerialNumber, Inaccuracy, InaccuracyMethodCode, CurrentRange, Units, LowerLimit, UpperLimit 
                    ) VALUES (
                    @DeviceType, @FullName, @SerialNumber, @Inaccuracy,  @InaccuracyMethodCode, @CurrentRange, @Units, @LowerLimit, @UpperLimit
                    );";

        using var cmd = new SqliteCommand(sql, conn);

        cmd.Parameters.AddWithValue("@DeviceType", (object?)template.DeviceType ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@FullName", (object?)template.FullName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SerialNumber", (object?)template.SerialNumber ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Inaccuracy", (object?)template.Inaccuracy ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@InaccuracyMethodCode", (object?)template.InaccuracyMethodCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CurrentRange", (object?)template.CurrentRange ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Units", (object?)template.Units ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@LowerLimit", (object?)template.LowerLimit ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@UpperLimit", (object?)template.UpperLimit ?? DBNull.Value);

        cmd.ExecuteNonQuery();

    }

    public static List<TemplateModel> GetAllTemplates()
    {
        var list = new List<TemplateModel>();
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sql = "SELECT * FROM Templates ORDER BY Id DESC";
        using var cmd = new SqliteCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(new TemplateModel
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                DeviceType = reader.IsDBNull(reader.GetOrdinal("DeviceType"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("DeviceType")),

                FullName = reader.IsDBNull(reader.GetOrdinal("FullName"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("FullName")),

                SerialNumber = reader.IsDBNull(reader.GetOrdinal("SerialNumber"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("SerialNumber")),

                Inaccuracy = reader.IsDBNull(reader.GetOrdinal("Inaccuracy"))
                    ? (double?)null
                    : reader.GetDouble(reader.GetOrdinal("Inaccuracy")),

                InaccuracyMethodCode = reader.GetInt32(reader.GetOrdinal("InaccuracyMethodCode")),

                CurrentRange = reader.IsDBNull(reader.GetOrdinal("CurrentRange"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("CurrentRange")),

                Units = reader.IsDBNull(reader.GetOrdinal("Units"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Units")),

                LowerLimit = reader.IsDBNull(reader.GetOrdinal("LowerLimit"))
                    ? (double?)null
                    : reader.GetDouble(reader.GetOrdinal("LowerLimit")),

                UpperLimit = reader.IsDBNull(reader.GetOrdinal("UpperLimit"))
                    ? (double?)null
                    : reader.GetDouble(reader.GetOrdinal("UpperLimit"))
            });
        }

        return list;
    }

    public static InstrumentModel? GetInstrumentById(int id)
    {
        const string sql = "SELECT * FROM instruments WHERE Id = @Id";

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = new SqliteCommand(sql, connection);      // сразу передаём SQL и соединение
        command.Parameters.AddWithValue("@Id", id);                 // или command.Parameters.Add("@Id", SqliteType.Integer).Value = id;

        using var reader = command.ExecuteReader();
        if (!reader.Read())
            return null;

        return new InstrumentModel
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
            };
    }


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

    public static void SaveInspector(InspectorModel inspector)
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sql = @"
        INSERT INTO Inspectors (FirstName, LastName, MiddleName)
        VALUES (@FirstName, @LastName, @MiddleName);
        ";

        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@FirstName", inspector.FirstName);
        cmd.Parameters.AddWithValue("@LastName", inspector.LastName);
        cmd.Parameters.AddWithValue("@MiddleName", (object?)inspector.MiddleName ?? DBNull.Value);
        cmd.ExecuteNonQuery();

    }

    /// <summary>
    /// Назначает или меняет поверителя для существующей поверки.
    /// </summary>
    public static void SetInspectorForVerification(int verificationId, int? inspectorId)
    {
        Console.WriteLine($"SetInspectorForVerification: VerificationId={verificationId}, InspectorId={inspectorId}");

        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sql = @"
        UPDATE Verification
        SET AuthorId = @AuthorId
        WHERE Id = @VerificationId;";

        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@VerificationId", verificationId);
        cmd.Parameters.AddWithValue("@AuthorId", (object?)inspectorId ?? DBNull.Value);

        int rowsAffected = cmd.ExecuteNonQuery();

        if (rowsAffected == 0)
        {
            Console.WriteLine("Ошибка: поверка с таким ID не найдена!");
        }
        else
        {
            Console.WriteLine(inspectorId.HasValue
                ? $"Поверитель успешно назначен для поверки {verificationId}"
                : $"Поверитель снят с поверки {verificationId}");
        }
    }


    public static List<InspectorModel> GetAllInspectors()
    {
        var list = new List<InspectorModel>();
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sql = "SELECT * FROM Inspectors ORDER BY LastName, FirstName, MiddleName";
        using var cmd = new SqliteCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(new InspectorModel
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                MiddleName = reader.IsDBNull(reader.GetOrdinal("MiddleName"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("MiddleName"))
            });
        }
        return list;
    }

    public static void DeleteInspector(int id)
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sql = "DELETE FROM Inspectors WHERE Id = @Id";
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);

        int rowsAffected = cmd.ExecuteNonQuery();
        if (rowsAffected == 0)
            throw new InvalidOperationException("Не удалось удалить: запись с таким Id не найдена.");
    }

    public static InspectorModel? GetInspectorById(int id)
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sql = @"
        SELECT Id, FirstName, LastName, MiddleName
        FROM Inspectors
        WHERE Id = @Id;";

        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            return null; // Инспектор не найден

        return new InspectorModel
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName"))
                ? null
                : reader.GetString(reader.GetOrdinal("FirstName")),
            LastName = reader.IsDBNull(reader.GetOrdinal("LastName"))
                ? null
                : reader.GetString(reader.GetOrdinal("LastName")),
            MiddleName = reader.IsDBNull(reader.GetOrdinal("MiddleName"))
                ? null
                : reader.GetString(reader.GetOrdinal("MiddleName"))
        };
    }

    public static InspectorModel? GetInspectorByVerificationId(int verificationId)
    {
        Console.WriteLine($"GetInspectorByVerificationId verificationId:{verificationId}");
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sql = @"
        SELECT i.Id, i.FirstName, i.LastName, i.MiddleName
        FROM Verification AS v
        LEFT JOIN Inspectors AS i ON v.AuthorId = i.Id
        WHERE v.Id = @VerificationId;";

        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@VerificationId", verificationId);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            Console.WriteLine("Поверка не найдена либо нет данных");
            return null; // Поверка не найдена либо нет данных
        }

        // Если AuthorId был NULL, то все поля инспектора будут NULL — корректно обработаем
        if (reader.IsDBNull(reader.GetOrdinal("Id")))
        {
            Console.WriteLine("Поверка не найдена либо нет данных");
            return null;
        }

        return new InspectorModel
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName"))
                ? null
                : reader.GetString(reader.GetOrdinal("FirstName")),
            LastName = reader.IsDBNull(reader.GetOrdinal("LastName"))
                ? null
                : reader.GetString(reader.GetOrdinal("LastName")),
            MiddleName = reader.IsDBNull(reader.GetOrdinal("MiddleName"))
                ? null
                : reader.GetString(reader.GetOrdinal("MiddleName"))
        };
    }




}