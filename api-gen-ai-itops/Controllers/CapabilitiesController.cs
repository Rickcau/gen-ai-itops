using api_gen_ai_itops.Interfaces;
using api_gen_ai_itops.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Net.Mime;

namespace api_gen_ai_itops.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CapabilitiesController : ControllerBase
    {
        private readonly ILogger<CapabilitiesController> _logger;
        private readonly ICosmosDbService _cosmosDbService;

        public CapabilitiesController(
            ILogger<CapabilitiesController> logger,
            ICosmosDbService cosmosDbService)
        {
            _logger = logger;
            _cosmosDbService = cosmosDbService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogDebug("Retrieving all capabilities");
                var capabilities = await _cosmosDbService.GetCapabilitiesAsync();
                return Ok(capabilities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving capabilities");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                _logger.LogDebug("Retrieving capability with id: {Id}", id);
                var capability = await _cosmosDbService.GetCapabilityAsync(id);
                if (capability is null)
                {
                    _logger.LogWarning("Capability not found with id: {Id}", id);
                    return NotFound();
                }
                return Ok(capability);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving capability with id: {Id}", id);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] Capability capability)
        {
            try
            {
                if (string.IsNullOrEmpty(capability.Id))
                {
                    capability.Id = Guid.NewGuid().ToString();
                }

                _logger.LogDebug("Creating new capability with id: {Id}", capability.Id);
                await _cosmosDbService.CreateCapabilityAsync(capability);

                return CreatedAtAction(nameof(Get), new { id = capability.Id }, capability);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating capability");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPut("{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(string id, [FromBody] Capability capability)
        {
            try
            {
                _logger.LogDebug("Updating capability with id: {Id}", id);
                var existingCapability = await _cosmosDbService.GetCapabilityAsync(id);
                if (existingCapability is null)
                {
                    _logger.LogWarning("Capability not found with id: {Id}", id);
                    return NotFound();
                }

                capability.Id = id; // Ensure ID matches route
                await _cosmosDbService.UpdateCapabilityAsync(id, capability);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating capability with id: {Id}", id);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                _logger.LogDebug("Deleting capability with id: {Id}", id);
                var capability = await _cosmosDbService.GetCapabilityAsync(id);
                if (capability is null)
                {
                    _logger.LogWarning("Capability not found with id: {Id}", id);
                    return NotFound();
                }

                await _cosmosDbService.DeleteCapabilityAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting capability with id: {Id}", id);
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}

