using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using RestSharp;

using System.Text.Json;
using System.Text.Json.Serialization;
namespace content_type_octet_stream
{
    static class Program
    {
        // Adicione sua subscription key do Computer Vision às variáveis ​​de ambiente.
        static string subscriptionKey = Environment.GetEnvironmentVariable("COMPUTER_VISION_SUBSCRIPTION_KEY");
        

        static string uriBase = "https://brazilsouth.api.cognitive.microsoft.com/vision/v3.0/read/analyze?language=pt";

        // Imagem que deseja realizar o OCR
        static string imageFilePath = Environment.CurrentDirectory + @"\Algumtexto.png";

        public static void Main()
        {
            // Chamada das APIs
            MakeAnalysisRequest(imageFilePath).Wait();

            Console.WriteLine("\nPress Enter to exit...");
            Console.ReadLine();
        }

        /// <summary>
        /// recupera o texto contido em uma imagem utilizando
        /// Computer Vision REST API.
        /// </summary>
        /// <param name="imageFilePath">O arquivo da imagem para analizar</param>
        static async Task MakeAnalysisRequest(string imageFilePath)
        {
            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                string uri = uriBase; 

                HttpResponseMessage response;

                // Converte a image para Byte Array
                byte[] byteData = GetImageAsByteArray(imageFilePath);

                // Adiciona o byte array em um octet stream no corpo do Request.
                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // use "application/octet-stream" content type.
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");

                    response = await client.PostAsync(uri, content);

                    string id = string.Empty;

                    try
                    {
                        id = ((string[])response.Headers.GetValues("apim-request-id"))[0];
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Não retornou ID " + ex.Message); ;
                    }


                    // Pega o ID retornado pela requisição anterior e chama a API para obter o resultado da leitura. 
                    var readclient = new RestClient($"https://brazilsouth.api.cognitive.microsoft.com/vision/v3.0/read/analyzeResults/{id}");
                    readclient.Timeout = -1;
                    var request = new RestRequest(Method.GET);
                    request.AddHeader("Content-Type", "application/json");
                    request.AddHeader("Ocp-Apim-Subscription-Key", subscriptionKey);
                    IRestResponse readresponse = readclient.Execute(request);


                    var objetoresult = JsonSerializer.Deserialize<ReadReturn>(readresponse.Content);

                    foreach (var r in objetoresult.analyzeResult.readResults)
                    {
                        foreach(var l in r.lines)
                        {
                            Console.WriteLine(l.text);
                        }
                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
        }

        /// <summary>
        /// Converte o Arquivo para Byte Array
        /// </summary>
        /// <param name="imageFilePath"></param>
        /// <returns></returns>
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            using (FileStream fileStream =
                new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }
    }
}