using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace ConsoleApplication6
{
  class Program
  {
    public class Country
    {
      public string Name { get; set; }
      public string RegionCode { get; set; }
      public string Index { get; set; }
    }
    public class City
    {
      public string Latitude { get; set; }
      public string Longitude { get; set; }
      public string Name { get; set; }
      public string Index { get; set; }
    }
    
 

    static string getResponse(string uri)
    {
      StringBuilder sb = new StringBuilder();
      byte[] buf = new byte[8192];
      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
      HttpWebResponse response = (HttpWebResponse)request.GetResponse();
      Stream resStream = response.GetResponseStream();
      int count = 0;
      do
      {
        count = resStream.Read(buf, 0, buf.Length);
        if (count != 0)
        {
          sb.Append(Encoding.Default.GetString(buf, 0, count));
        }
      }
      while (count > 0);
      return sb.ToString();
    }
    public static void Main()
    {
      try
      {
      String[] arguments = Environment.GetCommandLineArgs();
        string key = "";
          for (int i = 1; i < arguments.Length; i++)
          {
            key += arguments[i] + "%20";
          }
        string url = "http://www.google.com/trends/trendsReport?hl=en&q=" + key;
        string code = getResponse(url);

        string codeCity =  code.Replace(@",""city"":{""gvizData"":", @",{""city"":{""gvizData"":");
        Regex regCountry = new Regex(@"{""country"".*]]");
        Regex RegCity = new Regex(@"{""city"".*]]");
        MatchCollection matchesCountry = regCountry.Matches(code);
        MatchCollection matchesCity = RegCity.Matches(codeCity);

        string resultCountry = "";
        string resultCity = "";
        foreach (Match match  in matchesCountry)
        {
            string p1 = match.Value;
            resultCountry += p1;
        }
        resultCountry += "}}";

         foreach (Match match  in matchesCity)
        {
            string p1 = match.Value;
            resultCity += p1;
        }
        resultCity += "}}";       
        string json =resultCountry.Replace(@"[""regionCode"",""name"",""Search volume index""],", "");
        string jsonCity = resultCity.Replace(@"[""lat"",""lng"",""city"",""Search volume index""],","");

        JObject JList = JObject.Parse(json);
        JObject JCountry = (JObject)JList["country"];
        JArray CountriesArray = (JArray)JCountry["gvizData"];
        Country C = new Country();

        JObject JListCity = JObject.Parse(jsonCity);
        JObject JCity = (JObject)JListCity["city"];
        JArray CitysArray = (JArray)JCity["gvizData"];
        City City = new City();

        string html_code = @"<html>   
                            <head>
                                <link rel=""stylesheet"" href=""https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0-alpha/css/bootstrap.min.css"">
                                <script src=""https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0-alpha/js/bootstrap.min.js""></script>                      
                             </head> 
                             <body STYLE=""background-color: #00cdcd; padding: 30px 200px; ""> 
                              <h1 style=""color: #404040"">Results for """ + key.Replace("%20", "") + @"""</h1>                 
                              <table STYLE=""background-color: #fff"" class=""table table-hover"" align = ""center"">";

        StringBuilder builder = new StringBuilder();
        builder.Append(html_code);
        var header = string.Format(@"<thead class=""thead-inverse""><tr><th>Region code</th><th>Country name</th><th>Search volume index</th></tr></thead>");
        builder.Append(header);
        foreach (JArray J in CountriesArray)
        {
          C.RegionCode = (string)J[0];
          C.Name = (string)J[1];
          C.Index = (string)J[2];
          var row = string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", C.RegionCode, C.Name, C.Index);
          builder.Append(row);
        }
        builder.Append("</table></body></html>");
        key = key.Replace("%20", " ");

        using (var writer = new StreamWriter(key + "country results.html"))
        {
          writer.Write(builder.ToString());
        }

        StringBuilder builderCity = new StringBuilder();
        builderCity.Append(html_code);
        var headerCity = string.Format(@"<thead class=""thead-inverse""><tr><th>Latitude</th><th>Longitude</th><th>City name</th><th>Search volume index</th></tr></thead>");
        builderCity.Append(headerCity);
        foreach (JArray J in CitysArray)
        {
          City.Latitude = (string)J[0];
          City.Longitude= (string)J[1];
          City.Name = (string)J[2];
          City.Index = (string)J[3];
          var row = string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", City.Latitude, City.Longitude, City.Name, City.Index);
          builderCity.Append(row);
        }
        builderCity.Append("</table></body></html>");

        using (var writer = new StreamWriter(key + "city results.html"))
        {
          writer.Write(builderCity.ToString());
        }

        System.Diagnostics.Process.Start(key + "country results.html");
        System.Diagnostics.Process.Start(key + "city results.html");   
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
        Console.ReadKey();
      }
    }
  }
}
