using System.Text;
using System.Text.Json;
using backend.queue;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
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
        var serializer = new FhirJsonSerializer();
        return Task<TResource>.Run(() => {
            var res = fc.Create<TResource>(resource);
            fhirDataQueue.BasicPublish(exchange: "device",
                        routingKey: "",
                        basicProperties: null,
                        body: serializer.SerializeToBytes(res));
            Console.WriteLine(" [x] Sent {0}", res);
            return res;
        });
    }
}
