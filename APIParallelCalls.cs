using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParallelAPICalls
{
    public partial class APIParallelCalls : Form
    {
        #region Constant variables
        private const string PlanB = "bronze";
        private int PlanLimit = 600;
        #endregion

        /// <summary>
        /// The class which will do paralle and async processing of APIs
        /// </summary>
        APIParallerProccessor client = new APIParallerProccessor();
        public APIParallelCalls()
        {
            InitializeComponent();
        }

        private async void GetBillingCustomersinParallelFromAPI(IEnumerable<string> urls)
        {
            try
            {
                var tasks = urls.Select(id => client.GetCustomers(id));
                var users = await Task.WhenAll(tasks);

                textBox1.Text = string.Empty;
                if (APICallsSucceed(users))
                    ProcessCustomerBillingInfo(users);

                return;
            }
            catch (Exception ex)
            {
                if (ex.Data.Contains("APIError"))
                {
                    textBox1.Text = $"{ ex.Data["APIError"].ToString()}: { ex.Message}";
                }
                else
                    textBox1.Text = $"Error processing billing and cutomer info: {ex.Message}";
            }
        }

        private bool APICallsSucceed(IEnumerable<CustomerData> users)
        {
            bool checkApiCalls = false;

            if (users == null || users.Count() < 2)
            {
                textBox1.Text += $"Calls to APIs have returned an error.{Environment.NewLine }";
                return checkApiCalls;
            }

            var ApiStatus = users.Where(p => p.Status == "error");
            if (ApiStatus.Any())
                textBox1.Text += $"API Error. Invalid Storeid passed to Billing Info API";
            else
                checkApiCalls = true;

            return checkApiCalls;
        }

        private void ProcessCustomerBillingInfo(IEnumerable<CustomerData> customers)
        {

            try
            {
                CustomerData api1 = customers.First();
                CustomerData api2 = customers.Last();
                foreach (var itemapi1 in api1.Custdata)
                {
                    var api1key = itemapi1.Key;
                    var api1value = itemapi1.Value;

                    var api2Detail =
                     api2.Custdata.Where(itemapi2 => itemapi2.Key == api1key).ToDictionary(p => p.Key, p => p.Value);
                    if (api2Detail.Any())
                    {
                        object value = "";
                        var api2HasValue = ((Dictionary<string, object>)api2Detail).TryGetValue(api1key, out value);
                        if (api2HasValue)
                        {
                            if (PlanB.Equals(value.ToString(), StringComparison.InvariantCultureIgnoreCase)
                                && Convert.ToInt32(api1value) > PlanLimit)
                                textBox1.Text += $" Customer Id : {api1key } has a Plan B > Rs 600. His plan amount is Rs {api1value}{Environment.NewLine }";
                        }
                    }
                    else
                        textBox1.Text += $" Warning: Plan info missing for customer-id {itemapi1.Key}";

                }
                return;
            }
            catch (Exception ex)
            {
                textBox1.Text += $"Error processing billing and cutomer info {ex.Message}";
                throw;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox1.Text == string.Empty)
                {
                    textBox1.Text = "Enter a valid Customer ID";
                    return;
                }

                //add the 2 APIs URI 
                var ListAPIs = new List<string>() { "billing/" + textBox1.Text, "customers/" };

                // Start processing them in Parallel
                GetBillingCustomersinParallelFromAPI(ListAPIs);
                textBox1.Text = "Processing in Progress";
            }
            catch (Exception ex)
            {
                textBox1.Text += $"Error processing billing and cutomer info: {ex.Message}";
            }
        } 
    }
}
