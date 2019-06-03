using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using playermanagement.Data;
using playermanagement.Entity;

namespace playermanagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        private readonly MongoService _mongoService;
        public PlayersController(MongoService mongoService)
        {
            _mongoService = mongoService;
        }

        // GET api/values
        [HttpGet]
        public async Task<IEnumerable<Player>> Get()
        {
            var players = await _mongoService.GetAllDocumentsAsync<Player>("Players");
            return players;
        }

        // POST api/values
        [HttpPost]
        public async Task Post([FromBody] Player player)
        {
            player.Id = new Guid();
            await _mongoService.CreateAsync("Players", player);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task Delete(Guid id)
        {
            await _mongoService.DeleteDocumentByIdAsync<Player>(id, "Players");
        }
    }
}
