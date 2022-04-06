using GoogleTranslator.Configuration;
using GoogleTranslator.TranslatorService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace GoogleTranslator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TranslatorController : ControllerBase
    {
        public IConfiguration _configuration;
        public readonly IOptions<Translator> _options;

        string key1 = "";
        string key2 = "";
        string key3 = "";
        string key4 = "";
        string file = "TranslatorFile";

        List<string> keys = new List<string>();

        public TranslatorController(IOptions<Translator> options, IConfiguration configuration)
        {
            _configuration = configuration;
            _options = options;
           
            var values = _configuration.GetSection("Translator");
            foreach (IConfigurationSection section in values.GetChildren())
            {
                var key = section.GetValue<string>("ApiKey");
                keys.Add(key);
            }
           
            key1 = keys[0];
            key2 = keys[1];
            key3 = keys[2];
            key4 = keys[3];
        }
           
        [HttpGet]
        [Route("TranslateText")]
        public async Task<ActionResult<string>> Translate(string word, string fromLanguage, string toLanguage)
        {
            List<ApiKeyJson> list = ReadFromFile();
            if (list == null)
            {
                List<ApiKeyJson> list2 = new List<ApiKeyJson>();
                list = list2;
            }
            int count = 0;
            var keyx = key1;
            var counter = CounterKeyJson(keyx);
            if (counter > 21)
            {
                keyx = key2;
                counter = CounterKeyJson(keyx);
                if (counter > 21)
                {
                    keyx = key3;
                    counter = CounterKeyJson(keyx);
                    if (counter > 21)
                    {
                        keyx = key4;
                    }
                }
            }
            var basePath = Path.Combine(Directory.GetCurrentDirectory() + "\\Translator\\");
            if (!System.IO.Directory.Exists(basePath))
            {
                System.IO.Directory.CreateDirectory(basePath);
            }
            string TranslateFile = Path.Combine(basePath, $"{file}.txt");

            for (int i = 0; i < word.Length; i++)
            {
                count++;
            }
            var countRes = CounterAddJson(count, keyx);
            if (countRes == 0)
            {
                var json = new ApiKeyJson()
                {
                    ApykeyId = keyx,
                    Counter = count,
                };

                list.Add(json);
                var jsonSer = JsonConvert.SerializeObject(list, Formatting.Indented);
                await System.IO.File.WriteAllTextAsync(TranslateFile, jsonSer);
                return count.ToString();
            }
            var keyapi = list.FirstOrDefault(x => x.ApykeyId == keyx);
            if (keyapi == null)
            {
                var json = new ApiKeyJson()
                {
                    ApykeyId = keyx,
                    Counter = count,
                };

                list.Add(json);
                var jsonSer = JsonConvert.SerializeObject(list, Formatting.Indented);
                await System.IO.File.WriteAllTextAsync(TranslateFile, jsonSer);
                return count.ToString();
            }
            keyapi.Counter = countRes;

            var jsonSer2 = JsonConvert.SerializeObject(list);
            await System.IO.File.WriteAllTextAsync(TranslateFile, jsonSer2);

            string url = $"https://www.googleapis.com/language/translate/v2?key={keyx}&source={fromLanguage}&target={toLanguage}&q=" + word;
            var httpClient = new HttpClient();
            var result = httpClient.GetStringAsync(url).Result;
           
            try
            {
                return result;
            }
            catch
            {
                return BadRequest();
            }

           
        }
        [HttpGet("counter")]
        public ActionResult<string> Translate(string Key)
        {
            return CounterKeyJson(Key).ToString();
        }
        [HttpGet("read")]
        public ActionResult<List<ApiKeyJson>> Read()
        {
            return ReadFromFile();
        }


        [HttpGet("reset")]
        public ActionResult<bool> Reset()
        {
            try
            {
               bool x = ResetCaracter();
                if (x == true)
                {
                    return true;
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception)
            {

                return BadRequest();
            }
          
        }

        private bool ResetCaracter()
        {
            List<ApiKeyJson> list = ReadFromFile();
            if (list == null)
            {
                return false;
            }
            foreach (ApiKeyJson item in list)
            {
                item.ApykeyId = item.ApykeyId;
                item.Counter = 0;
            }
            try
            {
                var basePath2 = Path.Combine(Directory.GetCurrentDirectory(), "Translator");
                string TranslateFile = Path.Combine(basePath2, $"{file}.txt");
                var jsonSer2 = JsonConvert.SerializeObject(list);
                System.IO.File.WriteAllTextAsync(TranslateFile, jsonSer2);
                return true;
            }
            catch (Exception)
            {

                return false;
            }
           
        }

        private int CounterAddJson(int count, string key)
        {
            var basePath2 = Path.Combine(Directory.GetCurrentDirectory(), "Translator");
            string TranslateFile2 = Path.Combine(basePath2, $"{file}.txt");
            if (System.IO.File.Exists(TranslateFile2))
            {
                using (StreamReader sr = new StreamReader(TranslateFile2))
                {
                    var result = CounterKeyJson(key);
                    var countRes = count + result;
                    return countRes;
                }
            }
            return 0;
        }

        private int CounterKeyJson(string key)
        {
            var basePath2 = Path.Combine(Directory.GetCurrentDirectory(), "Translator");
            if (!System.IO.Directory.Exists(basePath2))
            {
                return 0;
            }
            string TranslateFile2 = Path.Combine(basePath2, $"{file}.txt");
            using (StreamReader sr = new StreamReader(TranslateFile2))
            {
                string json = sr.ReadToEnd();
                var items = JsonConvert.DeserializeObject<List<ApiKeyJson>>(json);
                var keyfile = items.FirstOrDefault(x=>x.ApykeyId == key);
                if (keyfile != null)
                {
                    return keyfile.Counter;
                }
               
            }
            return 0;
        }
        private List<ApiKeyJson> ReadFromFile()
        {
            var basePath2 = Path.Combine(Directory.GetCurrentDirectory(), "Translator");
            if (!System.IO.Directory.Exists(basePath2))
            {
                return null;
            }
            string TranslateFile2 = Path.Combine(basePath2, $"{file}.txt");
            string fileJson = System.IO.File.ReadAllText(TranslateFile2);
            var items = JsonConvert.DeserializeObject<List<ApiKeyJson>>(fileJson);
            return items;
        }
    }
}
