using Logging.API;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace PBJKModBase.PiShock
{
    /// <summary>
    /// Allows interfacing with an instance of a PiShock device
    /// </summary>
    public class PiShockDevice
    {
        private readonly ILogger logger;

        private readonly string apiUserName;
        private readonly string apiKey;
        private readonly string apiShareCode;

        private static string apiEndpoint = "https://do.pishock.com/api/apioperate/";
        private static string apiRequestName = "PBJKMod_PiShock";

        private HttpClient client;

        /// <summary>
        /// Ctor for creating a <see cref="PiShockDevice"/>
        /// </summary>
        /// <param name="logger">An <see cref="ILogger"/> implementation for logging</param>
        /// <param name="apiUserName">The username we should send to the PiShock API to interface with this device</param>
        /// <param name="apiKey">The API Key we should send to he PiShock API to interface with this device</param>
        /// <param name="apiShareCode">The share code we should send to the PiShock API to interface with this device</param>
        /// <exception cref="ArgumentNullException"></exception>
        public PiShockDevice(ILogger logger, string apiUserName, string apiKey, string apiShareCode)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.apiUserName = apiUserName ?? throw new ArgumentNullException(nameof(apiUserName));
            this.apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            this.apiShareCode = apiShareCode ?? throw new ArgumentNullException(nameof(apiShareCode));

            client = new HttpClient();
            client.BaseAddress = new Uri(apiEndpoint);

            // There's no API end point to check if you have valid credentials, you just need to try
            // to send a command and see if it succeeds- even worse even if they aren't valid you get
            // an okay response back from the API, but you need to check the message (which is just a string)
            // Given the above I'm not gonna bother to check if the API response is valid, the user should
            // just check the console output to see the API responses
        }

        /// <summary>
        /// Send a shock to the PiShock device
        /// <param name="Duration">The durtion of the shock in seconds, should be a value 0 to 15</param>
        /// <param name="Intensity">The intensity of the shock, should be a value 0 to 100</param>
        /// </summary>
        public async void Shock(int Duration, int Intensity)
        {
            // Ignore empty requests, this will make the API return an error anyways
            if ((Duration <= 0) || (Intensity <= 0))
            {
                logger.Information($"PiShock - Ignoring bad shock | Duration: {Duration} | Intensity: {Intensity}");
                return;
            }

            try
            {
                logger.Information($"PiShock - Sending shock | Duration: {Duration} | Intensity: {Intensity}");
                var PayloadData = new
                {
                    Username = apiUserName,
                    Apikey = apiKey,
                    Code = apiShareCode,
                    Name = apiRequestName,
                    Op = 0,
                    Duration = Math.Min(Duration, 15),      // Clamp the values passed to the API as it will error out if they are out of bounds
                    Intensity = Math.Min(Intensity, 100)
                };
                StringContent JsonContent = new StringContent(JsonConvert.SerializeObject(PayloadData), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("", JsonContent);
                logger.Information("PiShock API Response for Shock-");
                LogResponse(response);
            }
            catch (Exception e)
            {
                logger.Error($"Error sending shock {e.ToString()}");
            }
        }

        /// <summary>
        /// Send a vibration to the PiShock device
        /// <param name="Duration">The durtion of the vibration in seconds, should be a value greater than 0</param>
        /// <param name="Intensity">The intensity of the shock, should be a value 0 to 100</param>
        /// </summary>
        public async void Vibrate(int Duration, int Intensity)
        {
            // Ignore empty requests, this will make the API return an error anyways
            if ((Duration <= 0) || (Intensity <= 0))
            {
                logger.Information($"PiShock - Ignoring bad vibrate | Duration: {Duration} | Intensity: {Intensity}");
                return;
            }

            try
            {
                logger.Information($"PiShock - Sending vibration | Duration: {Duration} | Intensity: {Intensity}");
                var PayloadData = new
                {
                    Username = apiUserName,
                    Apikey = apiKey,
                    Code = apiShareCode,
                    Name = apiRequestName,
                    Op = 1,
                    Duration = Duration,
                    Intensity = Math.Min(Intensity, 100)    // Clamp the values passed to the API as it will error out if they are out of bounds
                };
                StringContent JsonContent = new StringContent(JsonConvert.SerializeObject(PayloadData), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("", JsonContent);
                logger.Information("PiShock API Response for Vibrate-");
                LogResponse(response);
            }
            catch (Exception e)
            {
                logger.Error($"Error sending vibrate {e.ToString()}");
            }
        }

        /// <summary>
        /// Send a beep to the PiShock device
        /// <param name="Duration">The durtion of the beepin seconds, should be a value greater than 0</param>
        /// </summary>
        public async void Beep(int Duration)
        {
            // Ignore empty requests, this will make the API return an error anyways
            if (Duration <= 0)
            {
                logger.Information($"PiShock - Ignoring bad beep | Duration: {Duration}");
                return;
            }

            try
            {
                logger.Information($"PiShock - Sending beep | Duration: {Duration}");
                var PayloadData = new
                {
                    Username = apiUserName,
                    Apikey = apiKey,
                    Code = apiShareCode,
                    Name = apiRequestName,
                    Op = 2,
                    Duration = Duration
                };
                StringContent JsonContent = new StringContent(JsonConvert.SerializeObject(PayloadData), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("", JsonContent);
                logger.Information("PiShock API Response for Beep-");
                LogResponse(response);
            }
            catch (Exception e)
            {
                logger.Error($"Error sending beep {e.ToString()}");
            }
        }

        /// <summary>
        /// Logs the response we got back from the PiShock API for debugging/confirming the device is working
        /// </summary>
        public async void LogResponse(HttpResponseMessage response)
        {
            logger.Information($"\tStatus Code: '{response.StatusCode}'");
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                logger.Information($"\tMessage: '{responseContent}'");
            }
        }
    }
}
