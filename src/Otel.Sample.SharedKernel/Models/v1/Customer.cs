namespace Otel.Sample.SharedKernel.Models.v1;

public record CustomerRequest(string Name, string LastName);

public record CustomerMessage(Guid Id, string Name, string LastName, DateTime Birthday);