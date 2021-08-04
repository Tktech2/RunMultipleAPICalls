using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParallelAPICalls
{
   
    /// <summary>
    /// A common Customer class to deserialize JSON info returned by both APIs
    /// </summary>
    public sealed class CustomerData 
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("data")]
        public Dictionary<string,  object> Custdata { get; set; }
    }

    /// <summary>
    /// Parallel processing of both APIs
    /// </summary>
    public class APIParallerProccessor
    {
        #region Declarations
        private HttpClient client = new HttpClient();
        private string baseurl = "https://assessments.reliscore.com/api/";

        #endregion

        /// <summary>
        /// ctor to set the HttpClient base address
        /// </summary>
        public APIParallerProccessor()
        {
            client.BaseAddress = new Uri(baseurl);
        }
        public async Task<CustomerData> GetCustomers(string url)
        {
            try
            {
                var response = await client.GetAsync(url).ConfigureAwait(false);

                var customerBillinginfo = JsonConvert.DeserializeObject<CustomerData>(await response.Content.ReadAsStringAsync());

                return customerBillinginfo;
            }
            catch (Exception ex)
            {
                ex.Data.Add("APIError", "API Error");
                throw;
            }
        }
    }   
}
