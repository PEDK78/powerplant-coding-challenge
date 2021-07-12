using Engie.PCC.Api.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using ApiServerStartup = Engie.PCC.Api.Startup;
using System.Threading;
using System;

namespace Engie.PCC.Api.Tests.Tests
{
    public class ApiServerTests : IClassFixture<HttpServerFixture<ApiServerStartup>>
    {
        private const string BaseUri = "/productionplan";

        private readonly HttpServerFixture<ApiServerStartup> _fixture;
        private readonly HttpClient _client;
        private readonly ApiClient _webSocketClient;

        public ApiServerTests(HttpServerFixture<ApiServerStartup> fixture)
        {
            _client = fixture.EngieApiClient;
            _webSocketClient = fixture.WebSocketClient;
            _fixture = fixture;
        }

        private async Task<HttpResponseMessage> PostPayLoad(string json)
        {
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var request = $"{BaseUri}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _client.PostAsync(request, content);
        }

        private async Task<List<PowerplantResult>> GetPowerplantResults(HttpResponseMessage message)
        {
            message.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<List<PowerplantResult>>(await message.Content.ReadAsStringAsync());
        }

        private Task<string> WaitForWebSocketMessage(CancellationToken cancelationToken)
        {
            return Task.Run(() =>
            {
                return _webSocketClient.Receive(cancelationToken);
            });
        }

        [Fact]
        public async Task SubmitPayLoad01_ExpectLoadReturned()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromSeconds(30));
            var notificationTask = WaitForWebSocketMessage(source.Token);

            var json = Resources.ReadResourceFile("PayLoads.payload1.json");
            var response = await PostPayLoad(json);
            response.EnsureSuccessStatusCode();

            var payload = JsonConvert.DeserializeObject<PayLoad>(json);
            var results = await GetPowerplantResults(response);

            results.Sum(x => x.Power).Should().Be(payload.Load);

            var notification = await notificationTask;
            var notificationResult = JsonConvert.DeserializeObject<WebSocketMessage>(notification);
            notificationResult.Input.Should().BeEquivalentTo(payload);
            notificationResult.Output.Should().BeEquivalentTo(results);
        }

        [Fact]
        public async Task SubmitPayLoad02_ExpectLoadReturned()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromSeconds(30));
            var notificationTask = WaitForWebSocketMessage(source.Token);

            var json = Resources.ReadResourceFile("PayLoads.payload2.json");
            var response = await PostPayLoad(json);
            response.EnsureSuccessStatusCode();

            var payload = JsonConvert.DeserializeObject<PayLoad>(json);
            var results = await GetPowerplantResults(response);

            results.Sum(x => x.Power).Should().Be(payload.Load);

            var notification = await notificationTask;
            var notificationResult = JsonConvert.DeserializeObject<WebSocketMessage>(notification);
            notificationResult.Input.Should().BeEquivalentTo(payload);
            notificationResult.Output.Should().BeEquivalentTo(results);
        }

        [Fact]
        public async Task SubmitPayLoad03_ExpectLoadReturned()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromSeconds(30));
            var notificationTask = WaitForWebSocketMessage(source.Token);

            var json = Resources.ReadResourceFile("PayLoads.payload3.json");
            var response = await PostPayLoad(json);
            response.EnsureSuccessStatusCode();

            var payload = JsonConvert.DeserializeObject<PayLoad>(json);
            var results = await GetPowerplantResults(response);

            results.Sum(x => x.Power).Should().Be(payload.Load);

            var notification = await notificationTask;
            var notificationResult = JsonConvert.DeserializeObject<WebSocketMessage>(notification);
            notificationResult.Input.Should().BeEquivalentTo(payload);
            notificationResult.Output.Should().BeEquivalentTo(results);
        }

        [Fact]
        public async Task SubmitPayLoadError01_ExpectBadRequest()
        {
            var json = Resources.ReadResourceFile("PayLoads.payload-error1.json");
            var response = await PostPayLoad(json);
            response.StatusCode.Should().Be(400);
        }
    }
}
