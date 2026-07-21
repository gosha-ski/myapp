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
                InputEtalonId INTEGER,
                OutputEtalonId INTEGER,
                FOREIGN KEY (InputEtalonId) REFERENCES Templates(Id) ON DELETE SET NULL,
                FOREIGN KEY (OutputEtalonId) REFERENCES Templates(Id) ON DELETE SET NULL,
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

                PRIMARY KEY (VerificationId, InstrumentId),

                FOREIGN KEY (VerificationId) REFERENCES Verification(Id) ON DELETE CASCADE,
                FOREIGN KEY (InstrumentId)   REFERENCES Instruments(Id)   ON DELETE CASCADE
            );";

        using var cmdProbing = new SqliteCommand(createProbingTableSql, conn);
        cmdProbing.ExecuteNonQuery();

        const string createLoaderRangeTableSql = @"
            CREATE TABLE IF NOT EXISTS LoaderRange (
                Id                 INTEGER PRIMARY KEY AUTOINCREMENT,
                VerificationId     INTEGER NOT NULL UNIQUE,
                Unit               TEXT,

                FOREIGN KEY (VerificationId) REFERENCES Verification(Id) ON DELETE CASCADE
            );";

        using var cmdLoaderRange = new SqliteCommand(createLoaderRangeTableSql, conn);
        cmdLoaderRange.ExecuteNonQuery();


        const string createLoadingPointsDefaultTableSql = @"
            CREATE TABLE IF NOT EXISTS LoadingPointsDefault (
                Id                 INTEGER PRIMARY KEY AUTOINCREMENT,
                LoaderRangeId      INTEGER NOT NULL,
                PointIndex         INTEGER NOT NULL,
                PointValue         REAL NOT NULL,

                FOREIGN KEY (LoaderRangeId) REFERENCES LoaderRange(Id) ON DELETE CASCADE,
                UNIQUE (LoaderRangeId, PointIndex)
            );
            ";

        using var cmdLoadingPointsDefault = new SqliteCommand(createLoadingPointsDefaultTableSql, conn);
        cmdLoadingPointsDefault.ExecuteNonQuery();
        //FOREIGN KEY (InstrumentId) REFERENCES Instruments(Id) ON DELETE CASCADE
        //FOREIGN KEY (DefaultPointId) REFERENCES LoadingPointsDefault(Id) ON DELETE CASCADE

        const string createLoadingPointsTableSql = @"
            CREATE TABLE IF NOT EXISTS LoadingPoints (
                Id                 INTEGER PRIMARY KEY AUTOINCREMENT,
                DefaultPointId      INTEGER NOT NULL,
                InstrumentId         INTEGER NOT NULL,
       
                
                TemplateValue      REAL,
                CalcValue          REAL,
                InstrumentValue    REAL,
                Error              REAL,
                Variation          REAL,
                Approved           INTEGER NOT NULL DEFAULT 0,  -- 0 = не утверждено, 1 = утверждено

                FOREIGN KEY (DefaultPointId) REFERENCES LoadingPointsDefault(Id) ON DELETE CASCADE,
                FOREIGN KEY (InstrumentId) REFERENCES Instruments(Id) ON DELETE CASCADE                  
            );
            ";

        using var cmdLoadingPoints = new SqliteCommand(createLoadingPointsTableSql, conn);
        cmdLoadingPoints.ExecuteNonQuery();

        Console.WriteLine("БД готова!");
    }

    public static void Initialize() { /* Пустой метод, нужен только чтобы вызвать статический конструктор */ }

    public static int SaveLoaderRangeForVerification(int verificationId, string unit)
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            // 1. Удаляем существующий ряд для этой поверки (если есть)
            using var cmdDelete = conn.CreateCommand();
            cmdDelete.Transaction = transaction;
            cmdDelete.CommandText = @"
                DELETE FROM LoaderRange
                WHERE VerificationId = @VerificationId";
            cmdDelete.Parameters.AddWithValue("@VerificationId", verificationId);
            cmdDelete.ExecuteNonQuery();

            // 2. Вставляем новый ряд
            using var cmdInsert = conn.CreateCommand();
            cmdInsert.Transaction = transaction;
            cmdInsert.CommandText = @"
                INSERT INTO LoaderRange (VerificationId, Unit)
                VALUES (@VerificationId, @Unit);
                SELECT last_insert_rowid();";
            cmdInsert.Parameters.AddWithValue("@VerificationId", verificationId);
            cmdInsert.Parameters.AddWithValue("@Unit", unit);

            // Получаем ID только что вставленной строки
            var result = cmdInsert.ExecuteScalar();
            int newId = Convert.ToInt32(result);

            transaction.Commit();
            return newId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }


    public static int AddLoadingPointDefault(int loaderRangeId, int pointIndex, double pointValue)
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sql = @"
            INSERT INTO LoadingPointsDefault (
                LoaderRangeId, 
                PointIndex, 
                PointValue
            ) VALUES (
                @LoaderRangeId, 
                @PointIndex, 
                @PointValue
            );
            SELECT last_insert_rowid();";

        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.AddWithValue("@LoaderRangeId", loaderRangeId);
        cmd.Parameters.AddWithValue("@PointIndex", pointIndex);
        cmd.Parameters.AddWithValue("@PointValue", pointValue);

        // ExecuteScalar вернёт значение из SELECT last_insert_rowid()
        var result = cmd.ExecuteScalar();
        return Convert.ToInt32(result);
    }


    public static void AddLoadingPoint(
        int defaultPointId, 
        int instrumentId,
  
        double? templateValue, 
        double? calcValue, 
        double? instrumentValue, 
        double? error, 
        double? variation,
        bool approved)
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sql = @"
            INSERT INTO LoadingPoints (
                DefaultPointId, 
                InstrumentId, 
              
                TemplateValue, 
                CalcValue, 
                InstrumentValue, 
                Error, 
                Variation,
                Approved
            ) VALUES (
                @DefaultPointId, 
                @InstrumentId, 
              
                @TemplateValue, 
                @CalcValue, 
                @InstrumentValue, 
                @Error, 
                @Variation,
                @Approved
            )";

        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.AddWithValue("@DefaultPointId", defaultPointId);
        cmd.Parameters.AddWithValue("@InstrumentId", instrumentId);
     
        
        cmd.Parameters.AddWithValue("@TemplateValue", (object?)templateValue ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CalcValue", (object?)calcValue ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@InstrumentValue", (object?)instrumentValue ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Error", (object?)error ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Variation", (object?)variation ?? DBNull.Value);

        // SQLite хранит bool как 0/1
        cmd.Parameters.AddWithValue("@Approved", approved ? 1 : 0);

        cmd.ExecuteNonQuery();
    }

    public static List<CalibrationPointModel> GetLoadingPointsByVerificationAndInstrument(
            int verificationId,
            int instrumentId)
    {
        var points = new List<CalibrationPointModel>();

        const string sql = @"
                SELECT
                    lp.Id AS LoadingPointId,
                    lpd.PointIndex,
                    lpd.PointValue AS PointValueMpa,
                    lp.TemplateValue,
                    lp.CalcValue,
                    lp.InstrumentValue,
                    lp.Error,
                    lp.Variation,
                    lp.Approved
                FROM LoadingPoints AS lp
                JOIN LoadingPointsDefault AS lpd ON lp.DefaultPointId = lpd.Id
                JOIN LoaderRange AS lr ON lpd.LoaderRangeId = lr.Id
                WHERE lr.VerificationId = @VerificationId
                  AND lp.InstrumentId = @InstrumentId
                ORDER BY lpd.PointIndex;
            ";

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@VerificationId", verificationId);
        command.Parameters.AddWithValue("@InstrumentId", instrumentId);

        using var reader = command.ExecuteReader();

        // Получаем индексы колонок один раз для скорости
        int colLoadingPointId = reader.GetOrdinal("LoadingPointId");
        int colPointIndex = reader.GetOrdinal("PointIndex");
        int colPointValueMpa = reader.GetOrdinal("PointValueMpa");
        int colTemplateValue = reader.GetOrdinal("TemplateValue");
        int colCalcValue = reader.GetOrdinal("CalcValue");
        int colInstrumentValue = reader.GetOrdinal("InstrumentValue");
        int colError = reader.GetOrdinal("Error");
        int colVariation = reader.GetOrdinal("Variation");
        int colApproved = reader.GetOrdinal("Approved");

        while (reader.Read())
        {
            // Читаем nullable double напрямую с проверкой IsDBNull
            double? templateValue = reader.IsDBNull(colTemplateValue) ? null : reader.GetDouble(colTemplateValue);
            double? calcValue = reader.IsDBNull(colCalcValue) ? null : reader.GetDouble(colCalcValue);
            double? instrumentValue = reader.IsDBNull(colInstrumentValue) ? null : reader.GetDouble(colInstrumentValue);
            double? error = reader.IsDBNull(colError) ? null : reader.GetDouble(colError);
            double? variation = reader.IsDBNull(colVariation) ? null : reader.GetDouble(colVariation);

            points.Add(new CalibrationPointModel
            {
                LoadingPointId = reader.GetInt32(colLoadingPointId),
                PointIndex = reader.GetInt32(colPointIndex),
                PointValueMpa = reader.GetDouble(colPointValueMpa),

                TemplateValue = templateValue,
                CalcValue = calcValue,
                InstrumentValue = instrumentValue,
                Error = error,
                Variation = variation,
                Approved = reader.GetInt32(colApproved) == 1
            });
        }

        return points;
    }

    public static List<LoadingPointDefaultModel> GetLoadingPointsByVerificationId(int verificationId)
    {
        var result = new List<LoadingPointDefaultModel>();

        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        // JOIN: LoaderRange (по VerificationId) -> LoadingPointsDefault
        const string sql = @"
            SELECT 
                lp.Id,
                lp.LoaderRangeId,
                lp.PointIndex,
                lp.PointValue
            FROM LoadingPointsDefault AS lp
            JOIN LoaderRange AS lr ON lp.LoaderRangeId = lr.Id
            WHERE lr.VerificationId = @VerificationId
            ORDER BY lp.PointIndex";

        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@VerificationId", verificationId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new LoadingPointDefaultModel
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                LoaderRangeId = reader.GetInt32(reader.GetOrdinal("LoaderRangeId")),
                PointIndex = reader.GetInt32(reader.GetOrdinal("PointIndex")),
                PointValue = reader.GetDouble(reader.GetOrdinal("PointValue"))
            });
        }

        return result;
    }




    public static void SetInstrumentChannel(int verificationId, int instrumentId, int? channel)
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sql = @"
            INSERT OR REPLACE INTO VerificationInstrument (
                VerificationId, 
                InstrumentId, 
                Channel
            ) VALUES (
                @VerificationId, 
                @InstrumentId, 
                @Channel
            )";

        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.AddWithValue("@VerificationId", verificationId);
        cmd.Parameters.AddWithValue("@InstrumentId", instrumentId);

        // Если channel == null, запишется NULL (для SQLite это корректно)
        cmd.Parameters.AddWithValue("@Channel", (object?)channel ?? DBNull.Value);

        cmd.ExecuteNonQuery();
    }

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

    public static List<InstrumentModel> GetInstrumentsByVerificationId(int verificationId)
    {
        var result = new List<InstrumentModel>();

        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sql = @"
        SELECT i.*
        FROM Instruments AS i
        INNER JOIN VerificationInstrument AS vi
            ON i.Id = vi.InstrumentId
        WHERE vi.VerificationId = @VerificationId;";

        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@VerificationId", verificationId);

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var instrument = new InstrumentModel
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                TypeCode = reader.GetInt32(reader.GetOrdinal("TypeCode")),
                Model = reader.IsDBNull(reader.GetOrdinal("Model"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Model")),
                SerialNumber = reader.IsDBNull(reader.GetOrdinal("SerialNumber"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("SerialNumber")),
                InventoryNumber = reader.IsDBNull(reader.GetOrdinal("InventoryNumber"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("InventoryNumber")),
                IntervalYears = reader.IsDBNull(reader.GetOrdinal("IntervalYears"))
                    ? (int?)null
                    : reader.GetInt32(reader.GetOrdinal("IntervalYears")),
                Location = reader.IsDBNull(reader.GetOrdinal("Location"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Location")),
                InServiceDate = reader.IsDBNull(reader.GetOrdinal("InServiceDate"))
                    ? DateTime.Today
                    : DateTime.Parse(reader.GetString(reader.GetOrdinal("InServiceDate"))),

                // Читаем как int из БД, потом конвертируем в строку
                Units = reader.IsDBNull(reader.GetOrdinal("Units"))
                    ? null
                    : GetUnitStringByCode(reader.GetInt32(reader.GetOrdinal("Units"))),

                LowerLimit = reader.IsDBNull(reader.GetOrdinal("LowerLimit"))
                    ? (double?)null
                    : reader.GetDouble(reader.GetOrdinal("LowerLimit")),
                UpperLimit = reader.IsDBNull(reader.GetOrdinal("UpperLimit"))
                    ? (double?)null
                    : reader.GetDouble(reader.GetOrdinal("UpperLimit")),
                AccuracyClass = reader.IsDBNull(reader.GetOrdinal("AccuracyClass"))
                    ? (double?)null
                    : reader.GetDouble(reader.GetOrdinal("AccuracyClass")),
                VariationLimit = reader.IsDBNull(reader.GetOrdinal("VariationLimit"))
                    ? (double?)null
                    : reader.GetDouble(reader.GetOrdinal("VariationLimit")),
                AccuracyMethodCode = reader.GetInt32(reader.GetOrdinal("AccuracyMethodCode"))
            };

            result.Add(instrument);
        }

        return result;
    }

    // Заглушка конвертера: замени на свой Enum.ToString()
    private static string? GetUnitStringByCode(int code)
    {
        return code switch
        {
            0 => "Па",
            1 => "кПа",
            2 => "МПа",
            3 => "В",
            4 => "mA"
        };
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

    public static void DeleteTemplate(int templateId)
    {
        const string sql = "DELETE FROM Templates WHERE Id = @Id";

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var pragmaCmd = connection.CreateCommand();
        pragmaCmd.CommandText = "PRAGMA foreign_keys = ON";
        pragmaCmd.ExecuteNonQuery();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@Id", templateId);

        command.ExecuteNonQuery();
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

    public static void SetVerificationInputTemplate(int verificationId, int inputEtalonId)
    {
        const string sql = @"
            UPDATE Verification
            SET InputEtalonId = @InputEtalonId
            WHERE Id = @VerificationId";

        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        using var cmd = new SqliteCommand(sql, conn);

        cmd.Parameters.AddWithValue("@VerificationId", verificationId);
        cmd.Parameters.AddWithValue("@InputEtalonId", inputEtalonId);
            
          // null корректно запишется как NULL

        cmd.ExecuteNonQuery();
    }

    public static void SetVerificationOutputTemplate(int verificationId, int outputEtalonId)
    {
        const string sql = @"
            UPDATE Verification
            SET OutputEtalonId = @OutputEtalonId
            WHERE Id = @VerificationId";

        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        using var cmd = new SqliteCommand(sql, conn);

        cmd.Parameters.AddWithValue("@VerificationId", verificationId);
        cmd.Parameters.AddWithValue("@OutputEtalonId", outputEtalonId);
            
          // null корректно запишется как NULL

        cmd.ExecuteNonQuery();
    }




    public static TemplateModel? GetInputTemplateByVerificationId(int verificationId)
    {
        Console.WriteLine($"GetInputTemplateByVerificationId {verificationId}");
        const string sql = @"
            SELECT t.Id,
                   t.DeviceType,
                   t.FullName,
                   t.SerialNumber,
                   t.Inaccuracy,
                   t.InaccuracyMethodCode,
                   t.CurrentRange,
                   t.Units,
                   t.LowerLimit,
                   t.UpperLimit
            FROM Verification v
            LEFT JOIN Templates t ON v.InputEtalonId = t.Id
            WHERE v.Id = @VerificationId";

        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@VerificationId", verificationId);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            return null;

        // Если InputEtalonId был NULL, то все поля из Templates будут NULL
        if (reader.IsDBNull(reader.GetOrdinal("Id")))
            return null;

        return new TemplateModel
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            DeviceType = reader.IsDBNull(reader.GetOrdinal("DeviceType")) ? null : reader.GetString(reader.GetOrdinal("DeviceType")),
            FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? null : reader.GetString(reader.GetOrdinal("FullName")),
            SerialNumber = reader.IsDBNull(reader.GetOrdinal("SerialNumber")) ? null : reader.GetString(reader.GetOrdinal("SerialNumber")),
            Inaccuracy = reader.IsDBNull(reader.GetOrdinal("Inaccuracy")) ? null : reader.GetDouble(reader.GetOrdinal("Inaccuracy")),
            InaccuracyMethodCode = reader.IsDBNull(reader.GetOrdinal("InaccuracyMethodCode")) 
                ? 0 
                : reader.GetInt32(reader.GetOrdinal("InaccuracyMethodCode")),
            CurrentRange = reader.IsDBNull(reader.GetOrdinal("CurrentRange")) ? null : reader.GetString(reader.GetOrdinal("CurrentRange")),
            Units = reader.IsDBNull(reader.GetOrdinal("Units")) ? null : reader.GetString(reader.GetOrdinal("Units")),
            LowerLimit = reader.IsDBNull(reader.GetOrdinal("LowerLimit")) ? null : reader.GetDouble(reader.GetOrdinal("LowerLimit")),
            UpperLimit = reader.IsDBNull(reader.GetOrdinal("UpperLimit")) ? null : reader.GetDouble(reader.GetOrdinal("UpperLimit"))
        };
    }

    public static TemplateModel? GetOutputTemplateByVerificationId(int verificationId)
    {
        Console.WriteLine($"GetInputTemplateByVerificationId {verificationId}");
        const string sql = @"
            SELECT t.Id,
                   t.DeviceType,
                   t.FullName,
                   t.SerialNumber,
                   t.Inaccuracy,
                   t.InaccuracyMethodCode,
                   t.CurrentRange,
                   t.Units,
                   t.LowerLimit,
                   t.UpperLimit
            FROM Verification v
            LEFT JOIN Templates t ON v.OutputEtalonId = t.Id
            WHERE v.Id = @VerificationId";

        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@VerificationId", verificationId);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            return null;

        // Если InputEtalonId был NULL, то все поля из Templates будут NULL
        if (reader.IsDBNull(reader.GetOrdinal("Id")))
            return null;

        return new TemplateModel
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            DeviceType = reader.IsDBNull(reader.GetOrdinal("DeviceType")) ? null : reader.GetString(reader.GetOrdinal("DeviceType")),
            FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? null : reader.GetString(reader.GetOrdinal("FullName")),
            SerialNumber = reader.IsDBNull(reader.GetOrdinal("SerialNumber")) ? null : reader.GetString(reader.GetOrdinal("SerialNumber")),
            Inaccuracy = reader.IsDBNull(reader.GetOrdinal("Inaccuracy")) ? null : reader.GetDouble(reader.GetOrdinal("Inaccuracy")),
            InaccuracyMethodCode = reader.IsDBNull(reader.GetOrdinal("InaccuracyMethodCode")) 
                ? 0 
                : reader.GetInt32(reader.GetOrdinal("InaccuracyMethodCode")),
            CurrentRange = reader.IsDBNull(reader.GetOrdinal("CurrentRange")) ? null : reader.GetString(reader.GetOrdinal("CurrentRange")),
            Units = reader.IsDBNull(reader.GetOrdinal("Units")) ? null : reader.GetString(reader.GetOrdinal("Units")),
            LowerLimit = reader.IsDBNull(reader.GetOrdinal("LowerLimit")) ? null : reader.GetDouble(reader.GetOrdinal("LowerLimit")),
            UpperLimit = reader.IsDBNull(reader.GetOrdinal("UpperLimit")) ? null : reader.GetDouble(reader.GetOrdinal("UpperLimit"))
        };
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

    public static void EditInstrument(InstrumentModel instrument)
    {
        if (instrument.Id <= 0)
            throw new ArgumentException("Нельзя обновить инструмент с Id <= 0", nameof(instrument.Id));

        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        const string sql = @"
        UPDATE Instruments
        SET 
            TypeCode          = @TypeCode,
            Model             = @Model,
            SerialNumber      = @SerialNumber,
            InventoryNumber   = @InventoryNumber,
            IntervalYears     = @IntervalYears,
            Location          = @Location,
            InServiceDate     = @InServiceDate,
            Units             = @Units,
            LowerLimit        = @LowerLimit,
            UpperLimit        = @UpperLimit,
            AccuracyClass     = @AccuracyClass,
            VariationLimit    = @VariationLimit,
            AccuracyMethodCode= @AccuracyMethodCode
        WHERE Id = @Id;";

        using var cmd = new SqliteCommand(sql, conn);

        // Обязательные поля (не nullable)
        cmd.Parameters.AddWithValue("@Id", instrument.Id);
        cmd.Parameters.AddWithValue("@TypeCode", instrument.TypeCode);
        cmd.Parameters.AddWithValue("@AccuracyMethodCode", instrument.AccuracyMethodCode);

        // Nullable строковые поля
        cmd.Parameters.AddWithValue("@Model", (object?)instrument.Model ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SerialNumber", (object?)instrument.SerialNumber ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@InventoryNumber", (object?)instrument.InventoryNumber ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Location", (object?)instrument.Location ?? DBNull.Value);

        // Если Units в БД хранится как TEXT (рекомендуемый Вариант 2)
        cmd.Parameters.AddWithValue("@Units", (object?)instrument.Units ?? DBNull.Value);

        // Nullable числовые поля
        cmd.Parameters.AddWithValue("@IntervalYears", (object?)instrument.IntervalYears ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@LowerLimit", (object?)instrument.LowerLimit ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@UpperLimit", (object?)instrument.UpperLimit ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@AccuracyClass", (object?)instrument.AccuracyClass ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@VariationLimit", (object?)instrument.VariationLimit ?? DBNull.Value);

        // Дата: в SQLite лучше хранить как ISO-строку "yyyy-MM-dd"
        string dateValue = instrument.InServiceDate.ToString("yyyy-MM-dd");
        cmd.Parameters.AddWithValue("@InServiceDate", dateValue);

        int rowsAffected = cmd.ExecuteNonQuery();

        if (rowsAffected == 0)
        {
            // Это значит, что инструмент с таким ID не найден
            Console.WriteLine($"Ошибка: не удалось обновить инструмент. ID={instrument.Id} не найден в базе.");
            throw new InvalidOperationException($"Инструмент с ID {instrument.Id} не найден. Возможно, он был удалён.");
        }
        else
        {
            Console.WriteLine($"Инструмент ID={instrument.Id} успешно обновлён.");
        }
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