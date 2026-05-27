using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Interfaces;

public interface IMeasurementStore
{
    Task AddAsync(Measurement m);

    Task<IEnumerable<Measurement>> QueryAsync(string? type, DateTimeOffset since);
}
