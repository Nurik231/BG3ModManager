﻿using System.IO;
using System.Net;
using System.Net.Http;

namespace DivinityModManager.Util
{
	public struct WebRequestHeaderValue
	{
		public HttpRequestHeader HttpRequestHeader { get; set; }
		public string Value { get; set; }
	}
	public static class WebHelper
	{
		public static readonly HttpClient Client = new();

		public static void SetupClient()
		{
			// Required for GitHub permissions
			Client.DefaultRequestHeaders.Add("User-Agent", "BG3ModManager");
		}

		public static async Task<Stream> DownloadFileAsStreamAsync(string downloadUrl, CancellationToken token)
		{
			try
			{
				using (var webClient = new WebClient())
				{
					int receivedBytes = 0;

					Stream stream = await webClient.OpenReadTaskAsync(downloadUrl);
					MemoryStream ms = new();
					var buffer = new byte[128000];
					int read = 0;
					var totalBytes = int.Parse(webClient.ResponseHeaders[HttpResponseHeader.ContentLength]);

					while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
					{
						ms.Write(buffer, 0, read);
						receivedBytes += read;
					}
					stream.Close();
					return ms;
				}
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"Error downloading url ({downloadUrl}):\n{ex}");
			}
			return null;
		}

		public static string DownloadUrlAsString(string downloadUrl)
		{
			using (System.Net.WebClient webClient = new())
			{
				try
				{
					return webClient.DownloadString(downloadUrl);
				}
				catch (Exception ex)
				{
					DivinityApp.Log($"Error downloading '{downloadUrl}' as string:\n{ex}");
				}
				return "";
			}
		}

		public static async Task<string> DownloadUrlAsStringAsync(string downloadUrl)
		{
			using (System.Net.WebClient webClient = new())
			{
				try
				{
					return await webClient.DownloadStringTaskAsync(downloadUrl);
				}
				catch (Exception ex)
				{
					DivinityApp.Log($"Error downloading '{downloadUrl}' as string:\n{ex}");
				}
				return "";
			}
		}

		#region OLD

		// Get/Post sources from here: https://stackoverflow.com/a/27108442
		/*
        public static string Get(string uri, params WebRequestHeaderValue[] webRequestHeaders)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.UseDefaultCredentials = true;
            request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
            if (webRequestHeaders != null)
            {
                foreach (var x in webRequestHeaders)
                {
                    if (x.HttpRequestHeader == HttpRequestHeader.UserAgent)
                    {
                        request.UserAgent = x.Value;
                    }
                    else
                    {
                        request.Headers.Add(x.HttpRequestHeader, x.Value);
                    }
                }
            }
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static async Task<string> GetAsync(string uri, params WebRequestHeaderValue[] webRequestHeaders)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.UseDefaultCredentials = true;
            request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
            if (webRequestHeaders != null)
            {
                foreach (var x in webRequestHeaders)
                {
                    if (x.HttpRequestHeader == HttpRequestHeader.UserAgent)
                    {
                        request.UserAgent = x.Value;
                    }
                    else
                    {
                        request.Headers.Add(x.HttpRequestHeader, x.Value);
                    }
                }
            }
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public static string Post(string uri, string data, string contentType, string method = "POST")
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.UseDefaultCredentials = true;
            request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ContentLength = dataBytes.Length;
            request.ContentType = contentType;
            request.Method = method;

            try
            {
                using (Stream requestBody = request.GetRequestStream())
                {
                    requestBody.Write(dataBytes, 0, dataBytes.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                DivinityApp.LogMessage($"Error reading stream:\n{ex}");
            }

            return "";
        }

        public static async Task<string> PostAsync(string uri, string data, string contentType = "", string accept = "", string method = "POST")
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.UseDefaultCredentials = true;
            request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ContentLength = dataBytes.Length;
            if (!String.IsNullOrEmpty(contentType)) request.ContentType = contentType;
            if (!String.IsNullOrEmpty(accept)) request.Accept = accept;
            request.Method = method;

            try
            {
                using (Stream requestBody = request.GetRequestStream())
                {
                    await requestBody.WriteAsync(dataBytes, 0, dataBytes.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                DivinityApp.LogMessage($"Error reading stream:\n{ex}");
            }

            return "";
        }
        */
		#endregion
	}
}
