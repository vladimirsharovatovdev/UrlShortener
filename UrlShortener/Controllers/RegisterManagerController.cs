using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml;
using UrlShortener.Data.Services;


namespace UrlShortener.Controllers
{
    [ApiController]
    [Route("managers")]
    public class RegisterManagerController : ControllerBase
    {
        private readonly CurrentUserService _currentUserService;
        private readonly ManagerService _managerService;

        public RegisterManagerController(CurrentUserService currentUserService, ManagerService managerService)
        {
            _currentUserService = currentUserService;
            _managerService = managerService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<ManagerDto>> GetManagers()
        {
            return await _managerService.GetManagers();
        }

        [HttpPost]
        public async Task<IActionResult> CreateManager([FromBody] CreateManagerModel model)
        {
            var managerDto = await _managerService.CreateManager(model.Name, model.Password);
            
            if (managerDto == null)
                return BadRequest();

            return Created((string?)null, value: managerDto);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateCredentials([FromBody] UpdateManagerCredentialsModel model)
        {
            var currentUser = _currentUserService.GetCurrentUser();
            ManagerDto? managerDto;
            
            try
            {
                managerDto = await _managerService.UpdateCredentials(
                    model.Password,
                    model.NewName,
                    model.NewPassword,
                    currentUser.ManagerId);
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized();
            }
            
            if (managerDto == null)
                return NotFound();
            
            return Ok(managerDto);
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteManager()
        {
            var currentUser = _currentUserService.GetCurrentUser();
            var result = await _managerService.DeleteManager(currentUser.ManagerId);
            
            if (result == false)
                return NotFound();
            
            return Ok();
        }
    }

    
}
