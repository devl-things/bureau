using Microsoft.AspNetCore.Mvc;
using Niles.Etl.Jobs;

namespace Niles.Controllers
{
    [ApiController]
    [Route("api/etl/jobs")]
    public class EtlController : ControllerBase
    {
        private const string GetJobByIdRouteName = "GetJobById";

        private readonly IJobManager _jobs;

        public EtlController(IJobManager jobs)
        {
            _jobs = jobs;
        }

        [HttpPost("{type}")]
        public async Task<IActionResult> StartAsync([FromRoute] JobType type, [FromBody] StartJobRequest? request, CancellationToken cancellationToken = default)
        {
            string id = await _jobs.EnqueueAsync(type, request?.Args, cancellationToken);
            return AcceptedAtRoute(GetJobByIdRouteName, new { id = id }, new { id });
        }

        [HttpGet("{id}", Name = GetJobByIdRouteName)]
        public ActionResult<JobStatus> GetByIdAsync([FromRoute] string id)
        {
            if (_jobs.TryGetStatus(id, out JobStatus status))
            {
                return Ok(status);
            }
            return NotFound();
        }

        // GET /api/etl/jobs?type=Downloader&state=Running
        [HttpGet]
        public ActionResult<IEnumerable<JobStatus>> ListAsync([FromQuery] JobType? type, [FromQuery] JobState? state)
        {
            return Ok(_jobs.GetAll(type, state));
        }

        // POST /api/jobs/{id}/cancel
        [HttpPost("{id}/cancel")]
        public IActionResult Cancel([FromRoute] string id)
        {
            return _jobs.Cancel(id) ? Accepted() : NotFound();
        }
    }
}
