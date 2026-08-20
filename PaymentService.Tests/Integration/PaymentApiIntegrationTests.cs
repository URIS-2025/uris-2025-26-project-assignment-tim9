using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PaymentService.Models.DTO.InvoiceDTOs;
using PaymentService.Models.DTO.InvoiceItemDTOs;
using PaymentService.Models.DTO.PaymentDTOs;
using PaymentService.Models.Enums;

namespace PaymentService.Tests.Integration
{
    //za razliku od unit testova, ovde se nista ne podmece - dize se cela aplikacija
    //sa pravim kontrolerima, repozitorijumima i MySQL bazom, i salju se stvarni HTTP zahtevi
    public class PaymentApiIntegrationTests : IClassFixture<PaymentApiFixture>
    {
        private static readonly Guid SeededInvoiceId = Guid.Parse("a1111111-1111-1111-1111-111111111111");

        private readonly PaymentApiFixture _fixture;
        private readonly HttpClient _client;

        public PaymentApiIntegrationTests(PaymentApiFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }

        // ---------- fakture ----------

        [Fact]
        public async Task GetInvoices_ReturnsSeededInvoice()
        {
            var response = await _client.GetAsync("/api/invoice");
            response.EnsureSuccessStatusCode();

            var invoices = await ReadAsync<List<InvoiceDTO>>(response);

            var seeded = Assert.Single(invoices!, i => i.InvoiceId == SeededInvoiceId);
            Assert.Equal(1500.00m, seeded.TotalAmount);
            Assert.Equal(2, seeded.Items.Count);
        }

        [Fact]
        public async Task GetInvoiceById_WhenInvoiceDoesNotExist_Returns404()
        {
            var response = await _client.GetAsync($"/api/invoice/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateInvoice_WithoutUserHeader_Returns400()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/invoice")
            {
                Content = JsonContent.Create(NewInvoicePayload())
            };

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateInvoice_WithoutItems_Returns400()
        {
            var payload = new
            {
                projectId = _fixture.KnownProjectId,
                issueDate = "2026-08-19T00:00:00",
                items = Array.Empty<object>()
            };

            var response = await PostAsync("/api/invoice", payload, withUserHeader: true);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateInvoice_ComputesTotalAndFillsDataFromOtherServices()
        {
            var response = await PostAsync("/api/invoice", NewInvoicePayload(), withUserHeader: true);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var confirmation = await ReadAsync<InvoiceConfirmationDTO>(response);

            //80 * 5 + 60 * 8 = 880
            Assert.Equal(880.00m, confirmation!.TotalAmount);
            Assert.Equal(2, confirmation.ItemCount);
            Assert.Equal(InvoiceStatus.Unpaid, confirmation.Status);
            Assert.Equal(PaymentApiFixture.KnownProjectName, confirmation.ProjectName);
            Assert.Equal(PaymentApiFixture.KnownUsername, confirmation.IssuedByUsername);
        }

        [Fact]
        public async Task EnumsAreSerializedAsText()
        {
            var response = await _client.GetAsync($"/api/invoice/{SeededInvoiceId}");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Contains("\"Unpaid\"", json);
            Assert.DoesNotContain("\"status\":0", json);
        }

        // ---------- stavke ----------

        [Fact]
        public async Task AddItem_RecalculatesInvoiceTotal()
        {
            var invoiceId = await CreateInvoiceAsync();

            var response = await PostAsync($"/api/invoice/{invoiceId}/items",
                new { description = "Dodatna obuka", unitPrice = 120.00m, quantity = 2 });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var confirmation = await ReadAsync<InvoiceItemConfirmationDTO>(response);

            Assert.Equal(240.00m, confirmation!.TotalAmount);
            Assert.Equal(1120.00m, confirmation.InvoiceTotalAmount);
        }

        [Fact]
        public async Task AddItem_WithInvalidQuantity_Returns400()
        {
            var invoiceId = await CreateInvoiceAsync();

            var response = await PostAsync($"/api/invoice/{invoiceId}/items",
                new { description = "Neispravna stavka", unitPrice = 10.00m, quantity = 0 });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteInvoice_AlsoRemovesItsItems()
        {
            var invoiceId = await CreateInvoiceAsync();

            var deleteResponse = await _client.DeleteAsync($"/api/invoice/{invoiceId}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var invoiceResponse = await _client.GetAsync($"/api/invoice/{invoiceId}");
            Assert.Equal(HttpStatusCode.NotFound, invoiceResponse.StatusCode);

            var itemsResponse = await _client.GetAsync($"/api/invoice/{invoiceId}/items");
            var items = await ReadAsync<List<InvoiceItemDTO>>(itemsResponse);

            Assert.Empty(items!);
        }

        // ---------- uplate ----------

        [Fact]
        public async Task CreatePayment_ExceedingDebt_Returns409()
        {
            var invoiceId = await CreateInvoiceAsync();

            var response = await PostAsync("/api/payment",
                new { invoiceId, amount = 99999.00m }, withUserHeader: true);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task PayingInvoiceInFull_MarksItPaidAndLocksIt()
        {
            var invoiceId = await CreateInvoiceAsync();

            var paymentResponse = await PostAsync("/api/payment",
                new { invoiceId, amount = 880.00m }, withUserHeader: true);

            Assert.Equal(HttpStatusCode.Created, paymentResponse.StatusCode);

            var payment = await ReadAsync<PaymentConfirmationDTO>(paymentResponse);
            Assert.Equal(PaymentStatus.Completed, payment!.Status);
            Assert.Equal(PaymentApiFixture.KnownUsername, payment.PaidByUsername);
            Assert.Equal(PaymentApiFixture.KnownProjectName, payment.ProjectName);

            //faktura je presla u Paid
            var invoiceResponse = await _client.GetAsync($"/api/invoice/{invoiceId}");
            var invoice = await ReadAsync<InvoiceDTO>(invoiceResponse);
            Assert.Equal(InvoiceStatus.Paid, invoice!.Status);

            //i vise ne moze da se menja
            var updateResponse = await _client.PutAsJsonAsync($"/api/invoice/{invoiceId}",
                new { status = "Cancelled" });
            Assert.Equal(HttpStatusCode.Conflict, updateResponse.StatusCode);

            //niti da se na nju dodaju stavke
            var itemResponse = await PostAsync($"/api/invoice/{invoiceId}/items",
                new { description = "Naknadna stavka", unitPrice = 10.00m, quantity = 1 });
            Assert.Equal(HttpStatusCode.Conflict, itemResponse.StatusCode);
        }

        [Fact]
        public async Task PartialPayment_LeavesInvoiceUnpaid_AndAllowsOnlyRemainder()
        {
            var invoiceId = await CreateInvoiceAsync();

            var first = await PostAsync("/api/payment",
                new { invoiceId, amount = 300.00m }, withUserHeader: true);
            Assert.Equal(HttpStatusCode.Created, first.StatusCode);

            var invoiceResponse = await _client.GetAsync($"/api/invoice/{invoiceId}");
            var invoice = await ReadAsync<InvoiceDTO>(invoiceResponse);
            Assert.Equal(InvoiceStatus.Unpaid, invoice!.Status);

            //ostalo je 580, pokusaj sa 600 mora da padne
            var tooMuch = await PostAsync("/api/payment",
                new { invoiceId, amount = 600.00m }, withUserHeader: true);
            Assert.Equal(HttpStatusCode.Conflict, tooMuch.StatusCode);

            var exact = await PostAsync("/api/payment",
                new { invoiceId, amount = 580.00m }, withUserHeader: true);
            Assert.Equal(HttpStatusCode.Created, exact.StatusCode);
        }

        [Fact]
        public async Task CreatePayment_ForUnknownInvoice_Returns404()
        {
            var response = await PostAsync("/api/payment",
                new { invoiceId = Guid.NewGuid(), amount = 10.00m }, withUserHeader: true);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ---------- pomocne metode ----------

        private object NewInvoicePayload() => new
        {
            projectId = _fixture.KnownProjectId,
            issueDate = "2026-08-19T00:00:00",
            items = new object[]
            {
                new { description = "Konsultacije", unitPrice = 80.00m, quantity = 5 },
                new { description = "Testiranje", unitPrice = 60.00m, quantity = 8 }
            }
        };

        //vraca id nove fakture na 880, da svaki test radi nad svojom fakturom
        private async Task<Guid> CreateInvoiceAsync()
        {
            var response = await PostAsync("/api/invoice", NewInvoicePayload(), withUserHeader: true);
            response.EnsureSuccessStatusCode();

            var confirmation = await ReadAsync<InvoiceConfirmationDTO>(response);
            return confirmation!.InvoiceId;
        }

        private async Task<HttpResponseMessage> PostAsync(string url, object payload, bool withUserHeader = false)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(payload)
            };

            if (withUserHeader)
            {
                request.Headers.Add("X-User-Id", _fixture.KnownUserId.ToString());
            }

            return await _client.SendAsync(request);
        }

        private static async Task<T?> ReadAsync<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, PaymentApiFixture.JsonOptions);
        }
    }
}
