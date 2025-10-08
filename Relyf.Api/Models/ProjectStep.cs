namespace Relyf.Api.Models;

public class ProjectStep
{
    public int ProjectStepId { get; set; } // PK
    public int ProjectId { get; set; }     // FK -> app.Project
    public int StepNumber { get; set; }    // 1..N
    public string Instruction { get; set; } = "";
}
