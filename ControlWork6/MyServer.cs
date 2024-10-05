using System.Net;

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
        _listener.Prefixes.Add($"http://localhost : {port.ToString()}/");
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