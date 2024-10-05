using System.Collections;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ControlWork6;

public class Serializer : IEnumerable<ToDoList>
{
    public static List<ToDoList> _ToDoLists = new List<ToDoList>();
    private static string path = "../../../tasks.json";

    public static JsonSerializerOptions op = new JsonSerializerOptions()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    public static void SaveEmployee() => File.WriteAllText(path, JsonSerializer.Serialize(_ToDoLists, op));

    public static List<ToDoList> GetEmployees()
    {
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "[]");
            Console.WriteLine("------------- В файле ничего нету -----------");
            SaveEmployee();
        }
        else
        {
            _ToDoLists = JsonSerializer.Deserialize<List<ToDoList>>(File.ReadAllText(path));
            if (_ToDoLists.Count == 0)
            {
                Console.WriteLine("---------- В файле ничего нету ---------");
                SaveEmployee();
            }
        }
        return _ToDoLists;
    }
    public IEnumerator<ToDoList> GetEnumerator()
    {
        for (int i = 0; i < _ToDoLists.Count; i++)
        {
            yield return _ToDoLists[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}