using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Web;
using RazorEngine;
using RazorEngine.Templating;

namespace ControlWork6;

public class MyServer
{
    private string _siteDirectory;
    private HttpListener _listener;
    private int _port;
    private List<ToDoList> _toDoLists = JsonSerializer.Deserialize<List<ToDoList>>(File.ReadAllText("../../../tasks.json"));

    public async Task RunServerAsync(string path, int port)
    {
        _siteDirectory = path;
        _port = port;
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{_port.ToString()}/");
        _listener.Start();
        Console.WriteLine($"Server started on {_port} \nFiles in {_siteDirectory}");
        await ListenAsync();
    }

    private async Task ListenAsync()
    {
        try
        {
            while (true)
            {
                HttpListenerContext context = await _listener.GetContextAsync();
                Process(context);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    //-----------------------------------------------------
    private void Process(HttpListenerContext context)
    {
        Console.WriteLine(context.Request.HttpMethod);
        string filename = context.Request.Url.AbsolutePath;
        Console.WriteLine(filename);
        filename = _siteDirectory + filename;
        if (File.Exists(filename))
        {
            try
            {
                string content = BuildHtml(filename, new List<ToDoList>());
                if (context.Request.HttpMethod == "POST" && context.Request.Url.AbsolutePath == "/index.html")
                {
                    StreamReader st = new StreamReader(context.Request.InputStream);
                    content = BuildHtml(filename, new List<ToDoList>());
                }
                if (context.Request.HttpMethod == "POST" && context.Request.Url.AbsolutePath == "/index.html")
                {
                    StreamReader st = new StreamReader(context.Request.InputStream);
                    string[] emp = st.ReadToEnd().Split("&");
                    
                    ToDoList toDoList = new ToDoList()
                    {
                        Id = _toDoLists.Count + 1,
                        Header = HttpUtility.UrlDecode(emp[0].Split("=")[1]),
                        Username = HttpUtility.UrlDecode(emp[1].Split("=")[1]), 
                        CreateDate = DateTime.Now,
                        Status = "new"
                    };
                    _toDoLists.Add(toDoList);
                    JsonSerializerOptions op = new JsonSerializerOptions()
                    {
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        WriteIndented = true
                    };
                    
                    File.WriteAllText("../../../tasks.json", JsonSerializer.Serialize(_toDoLists, op));
                    context.Response.Redirect("/index.html");
                }
                
                if (context.Request.HttpMethod == "POST" && context.Request.Url.AbsolutePath == "/index.html")
                {
                    StreamReader reader = new StreamReader(context.Request.InputStream);
                    string formData = reader.ReadToEnd();
                    string taskIdStr = HttpUtility.ParseQueryString(formData)["taskId"];

                    if (int.TryParse(taskIdStr, out int taskId))
                    {
                        ToDoList taskToRemove = _toDoLists.FirstOrDefault(t => t.Id == taskId);
                        if (taskToRemove != null)
                        {
                            _toDoLists.Remove(taskToRemove);

                            JsonSerializerOptions op = new JsonSerializerOptions
                            {
                                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                                WriteIndented = true
                            };

                            File.WriteAllText("../../../tasks.json", JsonSerializer.Serialize(_toDoLists, op));
                        }
                    }

                    context.Response.Redirect("/index.html");
                }
                
                
                if (context.Request.Url.AbsolutePath == "/index.html")
                {
                    List<ToDoList> toDoLists = new List<ToDoList>(_toDoLists);
                    if (context.Request.QueryString["IdFrom"] != null)
                    {
                        toDoLists = toDoLists.Where(e => e.Id >= Convert.ToInt32(context.Request.QueryString["IdFrom"])).ToList();
                    }
                    if (context.Request.QueryString["IdTo"] != null)
                    {
                        toDoLists = toDoLists.Where(e => e.Id <= Convert.ToInt32(context.Request.QueryString["IdTo"])).ToList();
                    }
                    content = BuildHtml(filename, toDoLists);
                }
                context.Response.ContentType = GetContentType(filename);
                context.Response.ContentLength64 = System.Text.Encoding.UTF8.GetBytes(content).Length;
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.OutputStream.Write(new byte[0]);
            }
        }
        else
        {
            context.Response.StatusCode = 404;
            context.Response.OutputStream.Write(new byte[0]);
        }
        context.Response.OutputStream.Close();
    }
    
    //-------------------------------------------------
    private string BuildHtml(string filename, List<ToDoList> toDoLists)
    {
        string html = "";
        string layoutPath = _siteDirectory + "/layout.html";
        var razorService = Engine.Razor;
        if (!razorService.IsTemplateCached("layout", null))
            razorService.AddTemplate("layout", File.ReadAllText(layoutPath));
        if (!razorService.IsTemplateCached(filename, null))
        {
            razorService.AddTemplate(filename, File.ReadAllText(filename));
            razorService.Compile(filename);
        }
        var viewModel = new { ToDoList = toDoLists };
        html = razorService.Run(filename, null, viewModel);
        return html;
    }
    //--------------------------------------------------
    private string? GetContentType(string filename)
    {
        var Dictionary = new Dictionary<string, string>()
        {
            {".css", "text/css"},
            {".js", "application/javascript"},
            {".png", "image/png"},
            {".jpg", "image/jpeg"},
            {".gif", "image/gif"},
            {".html", "text/html"},
            {".json", "application/json"}
        };
        string contentype = "";
        string extension = Path.GetExtension(filename);
        Dictionary.TryGetValue(extension, out contentype);
        return contentype;
    }
    public void Stop()
    {
        _listener.Abort();
        _listener.Stop();
    }
}