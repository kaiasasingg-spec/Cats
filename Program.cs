using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var dataDir = Environment.GetEnvironmentVariable("DATA_DIR")
    ?? Path.Combine(app.Environment.ContentRootPath, "data");
Directory.CreateDirectory(dataDir);
var connectionString = $"Data Source={Path.Combine(dataDir, "cats.db")}";

using (var db = new SqliteConnection(connectionString))
{
    db.Open();
    var create = db.CreateCommand();
    create.CommandText = "CREATE TABLE IF NOT EXISTS Cats (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Breed TEXT NOT NULL)";
    create.ExecuteNonQuery();

    var count = db.CreateCommand();
    count.CommandText = "SELECT COUNT(*) FROM Cats";
    if (Convert.ToInt32(count.ExecuteScalar()) == 0)
    {
        var seed = db.CreateCommand();
        seed.CommandText = "INSERT INTO Cats (Name, Breed) VALUES ('Мурзик', 'Британська'), ('Багіра', 'Сфінкс'), ('Рудий', 'Мейн-кун')";
        seed.ExecuteNonQuery();
    }
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/cats", () =>
{
    var cats = new List<Cat>();
    using var db = new SqliteConnection(connectionString);
    db.Open();
    var cmd = db.CreateCommand();
    cmd.CommandText = "SELECT Id, Name, Breed FROM Cats ORDER BY Id";
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        cats.Add(new Cat(reader.GetInt32(0), reader.GetString(1), reader.GetString(2)));
    }
    return Results.Ok(cats);
});

app.MapPost("/api/cats", (NewCat cat) =>
{
    if (string.IsNullOrWhiteSpace(cat.Name) || string.IsNullOrWhiteSpace(cat.Breed))
    {
        return Results.BadRequest();
    }
    using var db = new SqliteConnection(connectionString);
    db.Open();
    var cmd = db.CreateCommand();
    cmd.CommandText = "INSERT INTO Cats (Name, Breed) VALUES ($name, $breed)";
    cmd.Parameters.AddWithValue("$name", cat.Name.Trim());
    cmd.Parameters.AddWithValue("$breed", cat.Breed.Trim());
    cmd.ExecuteNonQuery();
    return Results.Ok();
});

app.MapDelete("/api/cats/{id:int}", (int id) =>
{
    using var db = new SqliteConnection(connectionString);
    db.Open();
    var cmd = db.CreateCommand();
    cmd.CommandText = "DELETE FROM Cats WHERE Id = $id";
    cmd.Parameters.AddWithValue("$id", id);
    cmd.ExecuteNonQuery();
    return Results.Ok();
});

app.Run();

record Cat(int Id, string Name, string Breed);
record NewCat(string Name, string Breed);
