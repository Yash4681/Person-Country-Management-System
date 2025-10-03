using Fizzler.Systems.HtmlAgilityPack;
using FluentAssertions;
using HtmlAgilityPack;

namespace CRUDUnitTest
{
    public class PersonsControllerIntegrationTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _httpClient;
        public PersonsControllerIntegrationTest(CustomWebApplicationFactory factory)
        {
            _httpClient = factory.CreateClient();
        }

        [Fact]
        public async Task Index_ToReturnView()
        {
            //Act
            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync("/Persons/Index");

            //Assert
            httpResponseMessage.IsSuccessStatusCode.Should().BeTrue();

            string responeBody = await httpResponseMessage.Content.ReadAsStringAsync();
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(responeBody);

            HtmlNode document = htmlDocument.DocumentNode;

            document.QuerySelectorAll("table.persons").Should().NotBeNull();
        }
    }
}
