namespace Rms.Infrastructure.Notifications;

/// <summary>Simulated notifications: prints to the console in colour.</summary>
public sealed class ConsoleNotifier : INotifier
{
    public void EmployeeAllocated(Employee employee, Project project, Allocation allocation) =>
        Write(ConsoleColor.Green,
            $"[NOTIFY] {employee.Name} allocated to '{project.Name}' at {allocation.Percent} " +
            $"({allocation.Period.Start:yyyy-MM-dd} to {allocation.Period.End:yyyy-MM-dd}).");

    public void EmployeeReleased(Employee employee, Project project, Allocation allocation) =>
        Write(ConsoleColor.Yellow,
            $"[NOTIFY] {employee.Name} released from '{project.Name}' (allocation #{allocation.Id}).");

    private static void Write(ConsoleColor color, string message)
    {
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine($"  {message}");
        Console.ForegroundColor = previous;
    }
}
