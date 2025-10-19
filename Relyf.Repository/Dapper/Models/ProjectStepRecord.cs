namespace Relyf.Repository.Dapper.Models;

public sealed class ProjectStepRecord
{
    public int ProjectStepId { get; init; }
    public int ProjectId { get; init; }
    public int StepNumber { get; init; }
    public string Instruction { get; init; } = "";
}
