

using backend.client;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("[controller]")]
public class DeviceController : ControllerBase
{
    private MQFhirClient _fhirClient;
    
    public DeviceController(MQFhirClient fhirClient){
        _fhirClient = fhirClient;
    }

   [HttpPost]
   [Route("{patientId}")]
    public async Task<ActionResult<Device>> AddDevice([FromRoute] string patientId, [FromBody] backend.model.Device device)
    {
        var d = new Device();
        var dn = new Device.DeviceNameComponent{
            Name = device.Name
        };
        d.DeviceName.Add(dn);
        d.Patient = new ResourceReference($"Patient/{patientId}");

        var bundle = _fhirClient.SearchById<Patient>(patientId);

        if (bundle.Entry.Count == 0){
            return BadRequest($"could not find patient {patientId}");
        }

        d = await _fhirClient.CreateAsync(d);
        return Created("", d);
    }
}