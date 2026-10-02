namespace UniversityApi;

public static class DeploymentData
{
    public static void Load(UniversityDb db, string path, bool databaseCreated)
    {
        // Existing databases are authoritative, even when the bundled data changes.
        if (!databaseCreated) return;
        if (!File.Exists(path)) throw new FileNotFoundException("The deployed university dataset is missing", path);
        ImportData.Import(db, path);
    }
}
