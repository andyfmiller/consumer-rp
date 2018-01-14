using System;
using System.Linq;
using System.Threading.Tasks;
using Consumer.Models;
using LtiLibrary.AspNetCore.Extensions;
using LtiLibrary.AspNetCore.Outcomes.v1;
using LtiLibrary.NetCore.Lti.v1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Consumer.Controllers
{
    [Route("[controller]")]
    public class OutcomesController : OutcomesControllerBase
    {
        private readonly Data.ApplicationDbContext _context;

        public OutcomesController(Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public class ResultSourcedId
        {
            public int AssignmentId { get; set; }
            public int CourseId { get; set; }
            public string UserId { get; set; }
        }

        protected override Func<DeleteResultRequest, Task<DeleteResultResponse>> OnDeleteResultAsync => DeleteResultAsync;
        protected override Func<ReadResultRequest, Task<ReadResultResponse>> OnReadResultAsync => ReadResultAsync;
        protected override Func<ReplaceResultRequest, Task<ReplaceResultResponse>> OnReplaceResultAsync => ReplaceResultAsync;

        private async Task<DeleteResultResponse> DeleteResultAsync(DeleteResultRequest request)
        {
            var response = new DeleteResultResponse();

            var resultSourcedId = JsonConvert.DeserializeObject<ResultSourcedId>(request.LisResultSourcedId);

            var assignment = await _context.Assignment.SingleOrDefaultAsync(a => a.Id == resultSourcedId.AssignmentId);
            if (assignment == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                return response;
            }

            var ltiRequest = await Request.ParseLtiRequestAsync();
            var signature = ltiRequest.GenerateSignature(assignment.ConsumerSecret);
            if (!ltiRequest.Signature.Equals(signature))
            {
                response.StatusCode = StatusCodes.Status401Unauthorized;
                return response;
            }

            var score = await _context.Score
                .AsNoTracking()
                .Where(s => s.AssignmentId == resultSourcedId.AssignmentId)
                .Where(s => s.CourseId == resultSourcedId.CourseId)
                .SingleOrDefaultAsync(s => s.UserId == resultSourcedId.UserId);
            if (score != null)
            {
                _context.Score.Remove(score);
                await _context.SaveChangesAsync();
            }

            return response;
        }

        private async Task<ReadResultResponse> ReadResultAsync(ReadResultRequest request)
        {
            var response = new ReadResultResponse();

            var resultSourcedId = JsonConvert.DeserializeObject<ResultSourcedId>(request.LisResultSourcedId);

            var assignment = await _context.Assignment.SingleOrDefaultAsync(a => a.Id == resultSourcedId.AssignmentId);
            if (assignment == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                return response;
            }

            var ltiRequest = await Request.ParseLtiRequestAsync();
            var signature = ltiRequest.GenerateSignature(assignment.ConsumerSecret);
            if (!ltiRequest.Signature.Equals(signature))
            {
                response.StatusCode = StatusCodes.Status401Unauthorized;
                return response;
            }

            var score = await _context.Score
                .AsNoTracking()
                .Where(s => s.AssignmentId == resultSourcedId.AssignmentId)
                .Where(s => s.CourseId == resultSourcedId.CourseId)
                .SingleOrDefaultAsync(s => s.UserId == resultSourcedId.UserId);
            if (score == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                return response;
            }

            response.Result = new Result {Score = score.Value, SourcedId = request.LisResultSourcedId};

            return response;
        }

        private async Task<ReplaceResultResponse> ReplaceResultAsync(ReplaceResultRequest request)
        {
            var response = new ReplaceResultResponse();

            var resultSourcedId = JsonConvert.DeserializeObject<ResultSourcedId>(request.Result.SourcedId);

            var assignment = await _context.Assignment.SingleOrDefaultAsync(a => a.Id == resultSourcedId.AssignmentId);
            if (assignment == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                return response;
            }

            var ltiRequest = await Request.ParseLtiRequestAsync();
            var signature = ltiRequest.GenerateSignature(assignment.ConsumerSecret);
            if (!ltiRequest.Signature.Equals(signature))
            {
                response.StatusCode = StatusCodes.Status401Unauthorized;
                return response;
            }

            var score = await _context.Score
                .Where(s => s.AssignmentId == resultSourcedId.AssignmentId)
                .Where(s => s.CourseId == resultSourcedId.CourseId)
                .SingleOrDefaultAsync(s => s.UserId == resultSourcedId.UserId);
            if (score == null)
            {
                score = new Score
                {
                    AssignmentId = resultSourcedId.AssignmentId,
                    CourseId = resultSourcedId.CourseId,
                    UserId = resultSourcedId.UserId
                };
                await _context.Score.AddAsync(score);
                await _context.SaveChangesAsync();
            }

            if (request.Result.Score.HasValue)
            {
                score.Value = request.Result.Score.Value;
            }
            await _context.SaveChangesAsync();
            return response;
        }
    }
}