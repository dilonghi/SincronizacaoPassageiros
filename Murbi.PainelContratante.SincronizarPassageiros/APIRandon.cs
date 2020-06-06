using Murbi.PainelContratante.SincronizarPassageiros.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Murbi.PainelContratante.SincronizarPassageiros
{
    public class APIRandon
    {
        public APIRandon()
        {
            Response = new ApiResponse();
            Passageiros = new List<PassageiroRandon>();

        }

        public ApiResponse Response { get; set; }
        public IEnumerable<PassageiroRandon> Passageiros { get; set; }

        private string Token { get; set; }


        public async Task<IEnumerable<PassageiroRandon>> ObterPassageiros()
        {
            await AutenticarApi();

            using var httpClient = new HttpClient();

            var url = $"https://terceiros.randon.com.br:8443/pontosoft-bh-randon-api/funcionario/?dataReferencia={ DataAtualFormatoApi() }";

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("Auth-Token", Token);


            try
            {
                var response = await httpClient.GetAsync(url);

                try
                {
                    var retorno = JsonConvert.DeserializeObject<ApiResponseRandon>(await response.Content.ReadAsStringAsync());

                    if (response.IsSuccessStatusCode)
                    {
                        Response.Success = true;
                        Response.Data = retorno.Data;
                    }

                    Response.Errors = retorno.Message;

                }
                catch (WebException wexc)
                {
                    HttpWebResponse webResponse = wexc.Response as HttpWebResponse;
                    StreamReader responseStream = new StreamReader(webResponse.GetResponseStream());

                    Response.Errors = await responseStream.ReadToEndAsync();

                    responseStream.Close();
                    responseStream.Dispose();
                }
            }
            catch (Exception ex)
            {
                Response.Errors = ex.Message;
            }

            return Passageiros;
        }


        public async Task<APIRandon> AutenticarApi()
        {
            using var httpClient = new HttpClient();

            var url = @"https://terceiros.randon.com.br:8443/pontosoft-bh-randon-api/auth";

            HttpContent camposFormulario = new FormUrlEncodedContent(
                        new Dictionary<string, string>() {
                            { "usuario", "murbi" },
                            { "senha", "randon@123" }
                        });

            camposFormulario.Headers.Clear();
            camposFormulario.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            camposFormulario.Headers.Add("Api-Key", "df6a2fa5-da83-43c9-801a-b2b6a341a92d");

            try
            {
                var response = await httpClient.PostAsync(url, camposFormulario);
                response.EnsureSuccessStatusCode();

                try
                {
                    var retorno = JsonConvert.DeserializeObject<ApiResponseRandon>(await response.Content.ReadAsStringAsync());

                    if (response.IsSuccessStatusCode)
                    {
                        Token = retorno.Token;
                        return this;
                    }

                    Response.Errors = retorno.Message;

                }
                catch (WebException wexc)
                {
                    HttpWebResponse webResponse = wexc.Response as HttpWebResponse;
                    StreamReader responseStream = new StreamReader(webResponse.GetResponseStream());

                    Response.Errors = await responseStream.ReadToEndAsync();

                    responseStream.Close();
                    responseStream.Dispose();
                }
            }
            catch (Exception ex)
            {
                Response.Errors = ex.Message;
            }



            return this;


        }


        private string DataAtualFormatoApi()
        {
            return $"{ DateTime.Now.Year }-{ DateTime.Now.Month }-{ DateTime.Now.Day }";
        }
    }
}
