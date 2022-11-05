using Hl7.Fhir.Rest;
using Hl7.Fhir.Model;
using System.Text.Json.Serialization;
using System.Text.Json;
using Hl7.Fhir.Serialization;

namespace model.fhir;
// Represents a FHIR event
public class FhirEvent<T> where T: Resource
{
    // Determines if the resource was added, removed, updated and among others.
    public HTTPVerb HTTPVerb {get; set;}
    // The resource itself.
    public T? FhirResource {get; set;}
}