using System.Text.Json;

namespace Dfe.Edis.SourceAdapter.Sld.Infrastructure.SubmitLearnerDataApi.UnitTests
{
    public class SystemTextMtwJsonSerializer : MockTheWeb.IJsonSerializer
    {
        public string Serialize(object item)
        {
            return JsonSerializer.Serialize(item);
        }
    }
}