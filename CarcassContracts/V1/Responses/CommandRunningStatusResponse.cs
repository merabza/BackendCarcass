namespace CarcassContracts.V1.Responses;

public sealed class CommandRunningStatusResponse
{
    public string? CommandKey;
    public bool Finished;
    public int ProgressMaxValue;
    public int ProgressValue;
    public bool Success;
}