using Microsoft.AspNetCore.Mvc;
using Niles.Chores.Abstractions.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Niles.Chores.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChoresController : ControllerBase
    {
        private IChoreService _choreService;
        public ChoresController(IChoreService choreService)
        {
            _choreService = choreService;
        }
        // GET: api/chores
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<ChoresController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ChoresController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ChoresController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ChoresController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
