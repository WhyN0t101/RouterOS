﻿using LTI_RouterOS.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LTI_RouterOS.Controller
{
    internal class MethodsController
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl;

        public MethodsController(string username, string password, string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ArgumentException("Base URL cannot be null or empty.");
            }

            this.baseUrl = baseUrl.StartsWith("https://") ? baseUrl : "https://" + baseUrl;

            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}")));
        }

        public async Task<string> Retrieve(string endpoint)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(baseUrl + endpoint);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Error retrieving data: " + ex.Message);
            }
        }

        public async Task<string> GetBridges(string endpoint)
        {
            try
            {
                string response = await Retrieve(endpoint);
                List<string> bridgeNames = ParseNamesFromJsonArray(response, "name");
                return bridgeNames.Count > 0 ? string.Join(Environment.NewLine, bridgeNames) : "No Bridges Found";
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving bridge data: " + ex.Message);
            }
        }

        public async Task TestConnection()
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(baseUrl);
                response.EnsureSuccessStatusCode(); // Ensure success status code
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Error testing connection: " + ex.Message);
            }
        }
        public async Task DeactivateBridge(string selectedID)
        {
            try
            {
                string apiUrl = $"{baseUrl}/rest/interface/bridge/port/{selectedID}";

                HttpResponseMessage response = await httpClient.DeleteAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                MessageBox.Show("Bridge deactivated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Error deactivating bridge: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task CreateBridge(string bridgeName, int mtu, string arpEnabled, string arpTimeout, string ageingTime, bool igmpSnooping, bool dhcpSnooping, bool fastForward)
        {
            try
            {
                string apiUrl = baseUrl + "/rest/interface/bridge/add";

                JObject payload = new JObject
                {
                    ["name"] = bridgeName,
                    ["mtu"] = mtu,
                    ["arp"] = arpEnabled,
                    ["arp-timeout"] = arpTimeout,
                    ["ageing-time"] = ageingTime,
                    ["igmp-snooping"] = igmpSnooping ? "true" : "false",
                    ["dhcp-snooping"] = dhcpSnooping ? "true" : "false",
                    ["fast-forward"] = fastForward ? "true" : "false"
                };

                HttpResponseMessage response = await SendPostRequest(apiUrl, payload);
                response.EnsureSuccessStatusCode();

                MessageBox.Show("Bridge created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Error creating bridge: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task UpdateBridge(string bridgeId, string bridgeName, int mtu, string arpEnabled, string arpTimeout, string ageingTime, bool igmpSnooping, bool dhcpSnooping, bool fastForward)
        {
            try
            {
                string apiUrl = $"{baseUrl}/rest/interface/bridge/{bridgeId}";

                JObject payload = new JObject
                {
                    ["name"] = bridgeName,
                    ["mtu"] = mtu,
                    ["arp"] = arpEnabled,
                    ["arp-timeout"] = arpTimeout,
                    ["ageing-time"] = ageingTime,
                    ["igmp-snooping"] = igmpSnooping ? "true" : "false",
                    ["dhcp-snooping"] = dhcpSnooping ? "true" : "false",
                    ["fast-forward"] = fastForward ? "true" : "false",
                };

                HttpResponseMessage response = await SendPatchRequest(apiUrl, payload);
                response.EnsureSuccessStatusCode();

                MessageBox.Show("Bridge updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Error updating bridge: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task AssociateBridge(string selectedInterface, string selectedBridge, int horizonValue, string learnOption, bool unknownUnicastFlood, bool broadcastFlood, bool hardwareOffload, bool unknownMulticastFlood, bool trusted, string multicastRouter, bool fastLeave)
        {
            try
            {
                string apiUrl = $"{baseUrl}/rest/interface/bridge/port/{selectedInterface}";

                JObject payload = new JObject
                {
                    ["bridge"] = selectedBridge,
                    ["horizon"] = horizonValue,
                    ["learn"] = learnOption,
                    ["multicast-router"] = multicastRouter,
                    ["unknown-unicast-flood"] = unknownUnicastFlood ? "true" : "false",
                    ["broadcast-flood"] = broadcastFlood ? "true" : "false",
                    ["hw"] = hardwareOffload ? "true" : "false",
                    ["unknown-multicast-flood"] = unknownMulticastFlood ? "true" : "false",
                    ["trusted"] = trusted ? "true" : "false",
                    ["fast-leave"] = fastLeave ? "true" : "false",
                };

                HttpResponseMessage response = await SendPatchRequest(apiUrl, payload);
                response.EnsureSuccessStatusCode();

                MessageBox.Show("Bridge associated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Error associating bridge: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task CreatePortToBridgeConnection(string selectedInterface, string selectedBridge, int horizonValue, string learnOption, bool unknownUnicastFlood, bool broadcastFlood, bool hardwareOffload, bool unknownMulticastFlood, bool trusted, string multicastRouter, bool fastLeave)
        {
            try
            {
                string apiUrl = $"{baseUrl}/rest/interface/bridge/port/add";

                JObject payload = new JObject
                {
                    ["interface"] = selectedInterface,
                    ["bridge"] = selectedBridge
                };

                HttpResponseMessage response = await SendPostRequest(apiUrl, payload);
                response.EnsureSuccessStatusCode();

                string res = await Retrieve("/rest/interface/bridge/port?interface=" + selectedInterface + "");
                JArray jsonArray = JArray.Parse(res);
                List<string> list = ParseNamesFromJsonArray(res, ".id");
                string id = list[0].ToString();


                await AssociateBridge(id, selectedBridge, horizonValue, learnOption, unknownUnicastFlood, broadcastFlood, hardwareOffload, unknownMulticastFlood, trusted, multicastRouter, fastLeave);
                MessageBox.Show("Port-to-bridge connection created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Error creating port-to-bridge connection: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

 



        public async Task DeleteBridge(string bridgeName)
        {
            try
            {
                string apiUrl = baseUrl + $"/rest/interface/bridge/{bridgeName}";

                // Send a DELETE request to delete the bridge
                HttpResponseMessage response = await httpClient.DeleteAsync(apiUrl);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("Bridge deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                // Handle exceptions
                MessageBox.Show("Error deleting bridge: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public async Task DeactivateWirelessInterface(string id)
        {
            try
            {
                string apiUrl = baseUrl + $"/rest/interface/wireless/{id}";

                JObject payload = new JObject
                {
                    ["disabled"] = "true"
                };

                HttpResponseMessage response = await SendPatchRequest(apiUrl, payload);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("Wireless Interface Deactivated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                // Handle exceptions
                MessageBox.Show("Error deactivating wireless Interface: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        public async Task ActivateWirelessInterface(string id)
        {
            try
            {
                string apiUrl = baseUrl + $"/rest/interface/wireless/{id}";

                JObject payload = new JObject
                {
                    ["disabled"] = "false"
                };

                // Send the PATCH request
                HttpResponseMessage response = await SendPatchRequest(apiUrl, payload);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("Wireless Interface Activated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                // Handle exceptions
                MessageBox.Show("Error Activating wireless Interface: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        public async Task<HttpResponseMessage> SendPatchRequest(string apiUrl, JObject payload)
        {
            string jsonPayload = payload.ToString();
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), apiUrl);
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            return await httpClient.SendAsync(request);
        }

        public async Task<HttpResponseMessage> SendPostRequest(string apiUrl, JObject payload)
        {
            string jsonPayload = payload.ToString();
            return await httpClient.PostAsync(apiUrl, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));
        }
        public async Task<HttpResponseMessage> SendPutRequest(string apiUrl, JObject payload)
        {
            string jsonPayload = payload.ToString();
            return await httpClient.PutAsync(apiUrl, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));
        }

        private List<string> ParseNamesFromJsonArray(string json, string propertyName)
        {
            var names = new List<string>();
            JArray jsonArray = JArray.Parse(json);
            foreach (JObject jsonObject in jsonArray)
            {
                if (jsonObject.TryGetValue(propertyName, out var value) && value.Type == JTokenType.String)
                {
                    names.Add(value.ToString());
                }
            }
            return names;
        }

        public async Task ConfigureWirelessSettings(string id, JObject payload)
        {
            try
            {
                
                string apiUrl = baseUrl + $"/rest/interface/wireless/{id}";

                HttpResponseMessage response = await SendPatchRequest(apiUrl, payload);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("Wireless Interface Configured successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                // Handle exceptions
                MessageBox.Show("Error Configuring wireless Interface: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task CreateWirelessSecurity(JObject payload)
        {
            try
            {
                string apiUrl = baseUrl + "/rest/interface/wireless/security-profiles";


                HttpResponseMessage response = await SendPostRequest(apiUrl, payload);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("Security profile created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show("Error creating security profile: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task CreatWirelessSecurity(JObject payload)
        {
            try
            {
                string apiUrl = baseUrl + "/rest/interface/wireless/security-profiles";


                HttpResponseMessage response = await SendPostRequest(apiUrl, payload);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("Security profile created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show("Error creating security profile: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task EditWirelessSecurity(JObject payload, string id)
        {
            try
            {
                string apiUrl = baseUrl + $"/rest/interface/wireless/security-profiles/{id}";


                HttpResponseMessage response = await SendPatchRequest(apiUrl, payload);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("Security profile Edited successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show("Error Editing security profile: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task DeleteSecProfile(string id)
        {
            try
            {
                string apiUrl = baseUrl + $"/rest/interface/wireless/security-profiles/{id}";

                // Send a DELETE request to delete the bridge
                HttpResponseMessage response = await httpClient.DeleteAsync(apiUrl);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("Security Profile deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                // Handle exceptions
                MessageBox.Show("Error deleting Security Profile: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}



