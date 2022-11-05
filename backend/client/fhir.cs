using System.Text;
using System.Text.Json;
using backend.queue;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using model.fhir;
using RabbitMQ.Client;

namespace backend.client;

public interface IFhirClient {
    Task<TResource> CreateAsync<TResource>(TResource resource) where TResource : Resource;
    Bundle SearchById<TResource>(string id, string[] includes = null, int? pageSize = null, string[] revIncludes = null) where TResource : Resource, new();

}

public class MQFhirClient: IFhirClient
{
    private FhirClient fc;
    private IModel fhirDataQueue;

    public MQFhirClient(FhirClient fc, IModel fQueue){
        this.fc = fc;
        this.fhirDataQueue = fQueue;
    }

    public Bundle SearchById<TResource>(string id, string[] includes = null, int? pageSize = null, string[] revIncludes = null) where TResource : Resource, new(){
        return fc.SearchById<TResource>(id, includes, pageSize, revIncludes);
    }
    

    public Task<TResource> CreateAsync<TResource>(TResource resource) where TResource : Resource
    {
        return Task<TResource>.Run(() => {
            var res = fc.Create<TResource>(resource);
            var fhirEvent = new FhirEvent<TResource> {
                HTTPVerb = HTTPVerb.POST,
                FhirResource = res,
            };
            var options = new JsonSerializerOptions().ForFhir(typeof(TResource).Assembly);
            var fhirEventJson = JsonSerializer.Serialize(fhirEvent, options);
            fhirDataQueue.BasicPublish(exchange: res.TypeName.ToLower(),
                        routingKey: "",
                        basicProperties: null,
                        body: Encoding.ASCII.GetBytes(fhirEventJson));
            Console.WriteLine(" [x] Sent {0}", res);
            return res;
        });
    }
}
