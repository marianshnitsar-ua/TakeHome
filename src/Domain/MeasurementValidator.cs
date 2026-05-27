using Domain.Interfaces;

namespace Domain;

public class MeasurementValidator : IMeasurementValidator
{
    public bool IsValid(Measurement m) => 
        m.MeasurementId != Guid.Empty && 
        m.Timestamp != default && 
        !string.IsNullOrWhiteSpace(m.DeviceId) && 
        !string.IsNullOrWhiteSpace(m.Type);
}
