using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
namespace UniversityApi;

// One small, additive classroom upgrade; not a general migration framework.
public static class DatabaseSchema
{
    public static void Upgrade(UniversityDb db)
    {
        var connection = (SqliteConnection)db.Database.GetDbConnection();
        connection.Open();
        try
        {
            using var inspect = connection.CreateCommand();
            inspect.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Students') WHERE name = 'RegistrationNumber'";
            if (Convert.ToInt32(inspect.ExecuteScalar()) > 0)
            {
                using var columns = connection.CreateCommand();
                columns.CommandText = "SELECT COUNT(*) FROM pragma_table_info('CourseSections') WHERE name = 'School'";
                if (Convert.ToInt32(columns.ExecuteScalar()) == 0)
                {
                    using var sectionTransaction = connection.BeginTransaction();
                    using var command = connection.CreateCommand();
                    command.Transaction = sectionTransaction;
                    command.CommandText = "ALTER TABLE CourseSections ADD COLUMN School TEXT NULL; ALTER TABLE CourseSections ADD COLUMN Program TEXT NULL; ALTER TABLE CourseSections ADD COLUMN Department TEXT NULL; ALTER TABLE CourseSections ADD COLUMN Level TEXT NULL; ALTER TABLE CourseSections ADD COLUMN Ects TEXT NULL; ALTER TABLE CourseSections ADD COLUMN TimetableInstructor TEXT NULL;";
                    command.ExecuteNonQuery(); sectionTransaction.Commit();
                }
                return;
            }
            // Keep a full SQLite backup, including committed WAL data, before modifying version 1.
            if (File.Exists(connection.DataSource))
            {
                using var backup = new SqliteConnection($"Data Source={connection.DataSource}.v1-backup-{DateTime.UtcNow:yyyyMMddHHmmss}.db");
                backup.Open();
                connection.BackupDatabase(backup);
            }
            using (var command = connection.CreateCommand())
            { command.CommandText = "PRAGMA foreign_keys = OFF"; command.ExecuteNonQuery(); }
            using var transaction = connection.BeginTransaction();
            try
            {
                // Rebuild these three tables to make missing email/teacher fields nullable.
                var creation = db.Database.GenerateCreateScript();
                foreach (var table in new[] { "Students", "Teachers", "Courses" })
                {
                    var create = Regex.Match(creation, $"CREATE TABLE \"{table}\" \\([\\s\\S]*?\\);", RegexOptions.None).Value;
                    if (string.IsNullOrEmpty(create)) throw new InvalidOperationException($"No schema for {table}");
                    Execute(create.Replace($"CREATE TABLE \"{table}\"", $"CREATE TABLE \"{table}_upgrade\""));
                    var columns = table == "Courses" ? "Id,Code,Title,TeacherId" : "Id,FullName,Email";
                    Execute($"INSERT INTO {table}_upgrade ({columns}) SELECT {columns} FROM {table}; DROP TABLE {table}; ALTER TABLE {table}_upgrade RENAME TO {table};");
                }
                Execute(Regex.Match(creation, "CREATE TABLE \"CourseSections\" \\([\\s\\S]*?\\);").Value);
                Execute("ALTER TABLE Enrollments ADD COLUMN CourseSectionId INTEGER NULL REFERENCES CourseSections(Id) ON DELETE RESTRICT;");
                foreach (Match match in Regex.Matches(creation, "CREATE (?:UNIQUE )?INDEX [\\s\\S]*?;"))
                    Execute(match.Value.Replace("CREATE UNIQUE INDEX", "CREATE UNIQUE INDEX IF NOT EXISTS").Replace("CREATE INDEX", "CREATE INDEX IF NOT EXISTS"));
                // foreign_key_check still works while enforcement is temporarily off.
                using var check = connection.CreateCommand();
                check.Transaction = transaction; check.CommandText = "PRAGMA foreign_key_check";
                using (var reader = check.ExecuteReader())
                    if (reader.Read()) throw new InvalidOperationException("Schema upgrade would break a database relationship");
                transaction.Commit();
                void Execute(string sql)
                {
                    using var command = connection.CreateCommand();
                    command.Transaction = transaction; command.CommandText = sql; command.ExecuteNonQuery();
                }
            }
            catch { transaction.Rollback(); throw; }
            finally
            {
                using var command = connection.CreateCommand();
                command.CommandText = "PRAGMA foreign_keys = ON"; command.ExecuteNonQuery();
            }
        }
        finally { connection.Close(); }
    }
}
