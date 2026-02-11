using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace MESSystem;

/// <summary>
/// 데이터베이스 마이그레이션 헬퍼
/// 실행 방법: dotnet run --project MESSystem -- migrate
/// </summary>
public class DatabaseMigrator
{
    private readonly string _connectionString;

    public DatabaseMigrator(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void Migrate()
    {
        Console.WriteLine("=== 데이터베이스 마이그레이션 시작 ===\n");

        try
        {
            // 1. ERP 테이블 생성
            Console.WriteLine("1. ERP 테이블 생성 중...");
            ExecuteSqlFile("migrate_erp.sql");
            Console.WriteLine("   ✅ 완료\n");

            // 2. 더미 데이터 삽입
            Console.WriteLine("2. 더미 데이터 삽입 중...");
            ExecuteSqlFile("seed_data.sql");
            Console.WriteLine("   ✅ 완료\n");

            // 3. 확인
            Console.WriteLine("3. 마이그레이션 결과 확인:");
            VerifyMigration();

            Console.WriteLine("\n=== 마이그레이션 성공! ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ 오류 발생: {ex.Message}");
            Console.WriteLine($"상세: {ex.StackTrace}");
        }
    }

    private void ExecuteSqlFile(string fileName)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"파일을 찾을 수 없습니다: {filePath}");
        }

        var sql = File.ReadAllText(filePath);
        
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // SQL을 명령문별로 분리하여 실행
        var statements = sql.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var statement in statements)
        {
            var trimmed = statement.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("--"))
                continue;

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = trimmed;
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                // "already exists" 오류는 무시 (이미 마이그레이션됨)
                if (!ex.Message.Contains("already exists") && 
                    !ex.Message.Contains("duplicate column name"))
                {
                    Console.WriteLine($"   ⚠️  경고: {ex.Message}");
                }
            }
        }
    }

    private void VerifyMigration()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // 테이블 수 확인
        var tableCount = ExecuteScalar(connection, 
            "SELECT COUNT(*) FROM sqlite_master WHERE type='table'");
        Console.WriteLine($"   - 총 테이블 수: {tableCount}");

        // 주요 테이블 확인
        var tables = new[] { "SalesClosings", "TaxInvoices", "Payments", "BankTransactions" };
        foreach (var table in tables)
        {
            var exists = ExecuteScalar(connection, 
                $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{table}'");
            var status = exists > 0 ? "✅" : "❌";
            Console.WriteLine($"   - {table}: {status}");
        }

        // 데이터 수 확인
        Console.WriteLine("\n   데이터 확인:");
        Console.WriteLine($"   - 거래처: {ExecuteScalar(connection, "SELECT COUNT(*) FROM Clients")}개");
        Console.WriteLine($"   - 품목: {ExecuteScalar(connection, "SELECT COUNT(*) FROM Products")}개");
        Console.WriteLine($"   - 주문서: {ExecuteScalar(connection, "SELECT COUNT(*) FROM Orders")}개");
        Console.WriteLine($"   - 사용자: {ExecuteScalar(connection, "SELECT COUNT(*) FROM Users")}개");
    }

    private long ExecuteScalar(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        var result = command.ExecuteScalar();
        return result != null ? Convert.ToInt64(result) : 0;
    }

    public static void RunMigration(string[] args)
    {
        if (args.Length > 0 && args[0] == "migrate")
        {
            Console.WriteLine("데이터베이스 마이그레이션 모드\n");
            
            var connectionString = "Data Source=mes.db";
            var migrator = new DatabaseMigrator(connectionString);
            migrator.Migrate();
            
            Environment.Exit(0);
        }
    }
}
