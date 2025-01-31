using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECPI_Client_MAUI
{
    public partial class swaggerClient
    {
        private string _baseUrl; // Define _baseUrl

        public swaggerClient()
        {
            _baseUrl = "http://localhost:5182"; // Set base API URL
        }

        public string GetBaseUrl()
        {
            return _baseUrl;
        }
    }
}
