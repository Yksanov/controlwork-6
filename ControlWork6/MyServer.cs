using System.Net;
using RazorEngine;
using RazorEngine.Templating;

namespace ControlWork6;

public class MyServer
{
    private string _siteDirectory;
    private HttpListener _listener;
    private int _port;

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

    private void Process(HttpListenerContext context)
    {
        Console.WriteLine(context.Request.HttpMethod);
        string filename = context.Request.Url.AbsolutePath;
        Console.WriteLine(filename);
        string page = filename.Trim();
        if (!Path.HasExtension(page))
        {
            try
            {
                page = page.Substring(1);
                string responcepage = $"<html><head><meta charset='utf8'></head><body><h1>{page}</h1></body></html>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responcepage);
                context.Response.ContentType = GetContentType(filename);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.OutputStream.Write(new byte[0]);
            }
        }
        
        filename = _siteDirectory + filename;
        if (File.Exists(filename))
        {
            try
            {
                if (context.Request.HttpMethod == "POST")  // 2 Task
                {
                    StreamReader str = new StreamReader(context.Request.InputStream);
                    string result = str.ReadToEnd();
                    Console.WriteLine(result);
                }
                string content= "";
                string IdFrom = context.Request.QueryString["IdFrom"];
                string IdTo = context.Request.QueryString["IdTo"];

                // List<Employee> employees = Serializer.GetEmployees();
                // List<Employee> filterId = employees;
                //
                // if (int.TryParse(IdFrom, out int idFrom) && int.TryParse(IdTo, out int idTo))
                // {
                //     filterId = employees.Where(e => e.Id >= idFrom && e.Id <= idTo).ToList();
                // }
                //
                //content = BuildHtml(filename, IdFrom.Length);
                
                
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
    private string BuildHtml(string filename, int id)
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
        // var viewModel = new { Employees = employees };
        // html = razorService.Run(filename, null, viewModel);
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