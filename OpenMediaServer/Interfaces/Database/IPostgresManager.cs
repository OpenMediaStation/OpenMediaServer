namespace OpenMediaServer.Interfaces.Database;

public interface IPostgresManager
{
    void InitializeDatabase(string connectionString);
}
