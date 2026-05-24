using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Extensions;
using TaskFlow.Application.DTOs.Comments;
using TaskFlow.Application.Interfaces.Services;

namespace TaskFlow.Api.Controllers;

[Authorize]
[ApiController]
public sealed class CommentsController(ICommentService commentService) : ControllerBase
{
    [HttpGet("api/tasks/{taskId:guid}/comments")]
    [ProducesResponseType(typeof(IReadOnlyList<CommentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByTaskId(Guid taskId, CancellationToken cancellationToken) =>
        (await commentService.GetByTaskIdAsync(taskId, cancellationToken)).ToActionResult();

    [HttpPost("api/tasks/{taskId:guid}/comments")]
    [ProducesResponseType(typeof(CommentResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        Guid taskId,
        CreateCommentRequest request,
        CancellationToken cancellationToken) =>
        (await commentService.CreateAsync(taskId, request, cancellationToken)).ToActionResult();

    [HttpDelete("api/comments/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        (await commentService.DeleteAsync(id, cancellationToken)).ToActionResult();
}
