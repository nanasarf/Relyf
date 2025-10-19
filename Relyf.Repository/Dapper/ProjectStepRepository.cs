using System.Data;
using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class ProjectStepRepository : BaseRepository, IProjectStepRepository
{
    public ProjectStepRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<IReadOnlyList<ProjectStepRecord>> ListAsync(int projectId, int authUserId, CancellationToken ct = default) =>
    WithConnection(async conn =>
    {
        const string sql = @"
        SELECT s.ProjectStepId, s.ProjectId, s.StepNumber, s.Instruction
        FROM app.ProjectStep s
        JOIN app.Project p ON p.ProjectId = s.ProjectId
        WHERE s.ProjectId = @projectId AND p.UserId = @authUserId AND p.IsDeleted = 0
        ORDER BY s.StepNumber ASC;";

        var rowsList = (await conn.QueryAsync<ProjectStepRecord>(
            new CommandDefinition(sql, new { projectId, authUserId }, cancellationToken: ct)
        )).ToList();

        IReadOnlyList<ProjectStepRecord> rows = rowsList; // explicit upcast fixes CS0029
        return rows;
    });


    public Task UpsertStepsAsync(int projectId, int authUserId, IEnumerable<string> steps, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            // ownership guard
            const string ownSql = "SELECT COUNT(1) FROM app.Project WHERE ProjectId=@projectId AND UserId=@authUserId AND IsDeleted=0;";
            var owns = await conn.ExecuteScalarAsync<int>(
                new CommandDefinition(ownSql, new { projectId, authUserId }, cancellationToken: ct));
            if (owns == 0) throw new UnauthorizedAccessException("Project not found or not owned by user.");

            using var tx = conn.BeginTransaction();

            // replace existing steps
            await conn.ExecuteAsync(
                new CommandDefinition("DELETE FROM app.ProjectStep WHERE ProjectId=@projectId;",
                    new { projectId }, tx, cancellationToken: ct));

            int n = 0;
            foreach (var instr in steps.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                n++;
                await conn.ExecuteAsync(
                    new CommandDefinition(
                        @"INSERT INTO app.ProjectStep (ProjectId, StepNumber, Instruction)
                          VALUES (@projectId, @n, @instruction);",
                        new { projectId, n, instruction = instr.Trim() }, tx, cancellationToken: ct));
            }

            tx.Commit();
        });
}
