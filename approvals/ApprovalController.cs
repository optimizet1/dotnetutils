using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApprovalRequestAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApprovalController : ControllerBase
    {
        private readonly ILogger<ApprovalController> _logger;
        private static readonly List<ApprovalRequest> ApprovalRequests = new();

        public ApprovalController(ILogger<ApprovalController> logger)
        {
            _logger = logger;
        }

        [HttpPost("submit")]
        public IActionResult SubmitRequest([FromBody] ApprovalRequestDto requestDto)
        {
            _logger.LogInformation("Submitting a new approval request.");

            // Business validation
            if (string.IsNullOrEmpty(requestDto.ProjectId) || string.IsNullOrEmpty(requestDto.UserId))
            {
                _logger.LogWarning("Invalid request: Project ID and User ID are required.");
                return BadRequest(new { Status = "Error", Message = "Project ID and User ID are required." });
            }

            var newRequest = new ApprovalRequest
            {
                Id = Guid.NewGuid().ToString(),
                ProjectId = requestDto.ProjectId,
                UserId = requestDto.UserId,
                Status = RequestStatus.Pending,
                CreatedDate = DateTime.UtcNow
            };

            ApprovalRequests.Add(newRequest);
            _logger.LogInformation("Approval request submitted successfully.");
            return Ok(new { Status = "Success", Message = "Approval request submitted successfully." });
        }

        [HttpGet("user-requests")]
        public IActionResult GetUserRequests(string userId, RequestStatus status, DateTime? maxDate = null)
        {
            _logger.LogInformation("Fetching user requests for UserID: {UserId}, Status: {Status}.", userId, status);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Invalid request: User ID is required.");
                return BadRequest(new { Status = "Error", Message = "User ID is required." });
            }

            var today = DateTime.UtcNow.Date;
            var requests = ApprovalRequests
                .Where(r => r.UserId == userId && r.Status == status && r.CreatedDate.Date >= today && (maxDate == null || r.CreatedDate.Date <= maxDate.Value.Date))
                .ToList();

            _logger.LogInformation("Found {Count} requests for UserID: {UserId}.", requests.Count, userId);
            return Ok(requests);
        }

        [HttpPost("update-status")]
        public IActionResult UpdateStatus([FromBody] UpdateStatusDto statusDto)
        {
            _logger.LogInformation("Updating status for RequestID: {RequestId} by ApproverUserID: {ApproverUserId}.", statusDto.RequestId, statusDto.ApproverUserId);

            // Business validation
            if (string.IsNullOrEmpty(statusDto.RequestId) || string.IsNullOrEmpty(statusDto.ApproverUserId) || (statusDto.Status != RequestStatus.Approved && statusDto.Status != RequestStatus.Rejected))
            {
                _logger.LogWarning("Invalid request: Valid Request ID, Approver User ID, and Status are required.");
                return BadRequest(new { Status = "Error", Message = "Valid Request ID, Approver User ID, and Status are required." });
            }

            var request = ApprovalRequests.FirstOrDefault(r => r.Id == statusDto.RequestId);
            if (request == null)
            {
                _logger.LogWarning("Request not found for RequestID: {RequestId}.", statusDto.RequestId);
                return NotFound(new { Status = "Error", Message = "Approval request not found." });
            }

            if (request.Status != RequestStatus.Pending)
            {
                _logger.LogWarning("Invalid operation: Only pending requests can be updated. RequestID: {RequestId}.", statusDto.RequestId);
                return BadRequest(new { Status = "Error", Message = "Only pending requests can be updated." });
            }

            request.Status = statusDto.Status;
            request.Reason = statusDto.Reason;
            request.UpdatedDate = DateTime.UtcNow;

            _logger.LogInformation("Request {RequestId} updated to status {Status} by {ApproverUserId}.", statusDto.RequestId, statusDto.Status, statusDto.ApproverUserId);
            return Ok(new { Status = "Success", Message = $"Request {statusDto.Status.ToString().ToLower()} successfully." });
        }
    }

    public class ApprovalRequestDto
    {
        public string ProjectId { get; set; }
        public string UserId { get; set; }
    }

    public class UpdateStatusDto
    {
        public string RequestId { get; set; }
        public string ApproverUserId { get; set; }
        public RequestStatus Status { get; set; }
        public string Reason { get; set; }
    }

    public class ApprovalRequest
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string UserId { get; set; }
        public RequestStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string Reason { get; set; }
    }

    public class Approval
    {
        public Guid Id { get; set; }
        public Guid ProjectGUID { get; set; }
        public Guid UserGUID { get; set; }
        public RequestStatus Status { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class EmailHistory
    {
        public Guid Id { get; set; }
        public Guid ProjectGUID { get; set; }
        public Guid RequestorUserGUID { get; set; }
        public string Receivers { get; set; }
        public DateTime Created { get; set; }
        public string EmailTemplate { get; set; }
        public string ReasonStatus { get; set; }
    }

    public enum RequestStatus
    {
        New = 0,
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }
}
