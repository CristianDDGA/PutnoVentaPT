using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Application.Constants;
using PuntoVenta.Application.DTOs.User;
using PuntoVenta.Application.Interfaces.Services;

namespace PuntoVenta.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppRoles.Admin)]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<CreateUserDto> _createUserValidator;
    private readonly IValidator<UpdateUserDto> _updateUserValidator;
    private readonly IValidator<ChangeUserPasswordDto> _changePasswordValidator;

    public UsersController(
        IUserService userService,
        IValidator<CreateUserDto> createUserValidator,
        IValidator<UpdateUserDto> updateUserValidator,
        IValidator<ChangeUserPasswordDto> changePasswordValidator)
    {
        _userService = userService;
        _createUserValidator = createUserValidator;
        _updateUserValidator = updateUserValidator;
        _changePasswordValidator = changePasswordValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _userService.GetAllAsync());

    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetById(int userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        return user is null ? NotFound($"User with id {userId} not found.") : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var validationResult = await _createUserValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(error => error.ErrorMessage));

        var savedUser = await _userService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { userId = savedUser.UserId }, savedUser);
    }

    [HttpPut("{userId:int}")]
    public async Task<IActionResult> Update(int userId, [FromBody] UpdateUserDto dto)
    {
        var validationResult = await _updateUserValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(error => error.ErrorMessage));

        var modifiedBy = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? User.Identity?.Name;
        var success = await _userService.UpdateAsync(userId, dto, modifiedBy);
        return success ? NoContent() : NotFound($"User with id {userId} not found.");
    }

    [HttpPut("{userId:int}/password")]
    public async Task<IActionResult> ChangePassword(int userId, [FromBody] ChangeUserPasswordDto dto)
    {
        var validationResult = await _changePasswordValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(error => error.ErrorMessage));

        var success = await _userService.ChangePasswordAsync(userId, dto);
        return success ? NoContent() : NotFound($"User with id {userId} not found.");
    }

    [HttpPut("{userId:int}/activate")]
    public async Task<IActionResult> Activate(int userId)
        => await _userService.ActivateAsync(userId) ? NoContent() : NotFound($"User with id {userId} not found.");

    [HttpPut("{userId:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int userId)
    {
        var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(currentUserIdClaim, out var currentUserId) && currentUserId == userId)
        {
            return BadRequest(new { Message = "No puedes desactivar tu propio usuario." });
        }

        return await _userService.DeactivateAsync(userId) ? NoContent() : NotFound($"User with id {userId} not found.");
    }

    [HttpPut("{userId:int}/unlock")]
    public async Task<IActionResult> Unlock(int userId)
        => await _userService.UnlockAsync(userId) ? NoContent() : NotFound($"User with id {userId} not found.");
}