using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Lisa.Handlers;
public class CustomCircuitHandler : CircuitHandler
{
    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Connection established: {circuit.Id}");
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Connection lost: {circuit.Id}");
        return Task.CompletedTask;
    }
}