using Microsoft.EntityFrameworkCore;
using ComplianceControlCenter.Web.Core.Data;
using ComplianceControlCenter.Web.Modules.Oea.Domain.Entities;

namespace ComplianceControlCenter.Web.Modules.Oea.Services;

public interface ICommentService
{
    Task<IReadOnlyList<Comment>> GetForActivityAsync(int activityId, CancellationToken ct = default);
    Task<Comment> AddAsync(int activityId, string author, string? authorUserId, string text, CancellationToken ct = default);
    Task DeleteAsync(int commentId, CancellationToken ct = default);
}

public class CommentService : ICommentService
{
    private readonly AppDbContext _db;
    private readonly IChecklistNotifier _notifier;

    public CommentService(AppDbContext db, IChecklistNotifier notifier)
    {
        _db = db;
        _notifier = notifier;
    }

    public Task<IReadOnlyList<Comment>> GetForActivityAsync(int activityId, CancellationToken ct = default) =>
        _db.Comments.AsNoTracking()
            .Where(c => c.ActivityId == activityId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<Comment>)t.Result, ct);

    public async Task<Comment> AddAsync(int activityId, string author, string? authorUserId, string text, CancellationToken ct = default)
    {
        var c = new Comment
        {
            ActivityId = activityId,
            Author = author,
            AuthorUserId = authorUserId,
            Text = text,
            CreatedAt = DateTime.UtcNow
        };
        _db.Comments.Add(c);
        await _db.SaveChangesAsync(ct);
        await _notifier.CommentAddedAsync(activityId, c.Id, author);
        return c;
    }

    public async Task DeleteAsync(int commentId, CancellationToken ct = default)
    {
        var c = await _db.Comments.FirstOrDefaultAsync(x => x.Id == commentId, ct);
        if (c is null) return;
        var activityId = c.ActivityId;
        _db.Comments.Remove(c);
        await _db.SaveChangesAsync(ct);
        await _notifier.CommentDeletedAsync(activityId, commentId);
    }
}
