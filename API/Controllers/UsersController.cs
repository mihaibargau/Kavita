﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Data.Repositories;
using API.DTOs;
using API.Entities.Enums;
using API.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UsersController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpDelete("delete-user")]
        public async Task<ActionResult> DeleteUser(string username)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            _unitOfWork.UserRepository.Delete(user);

            if (await _unitOfWork.CommitAsync()) return Ok();

            return BadRequest("Could not delete the user.");
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            return Ok(await _unitOfWork.UserRepository.GetEmailConfirmedMemberDtosAsync());
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetPendingUsers()
        {
            return Ok(await _unitOfWork.UserRepository.GetPendingMemberDtosAsync());
        }


        [HttpGet("has-reading-progress")]
        public async Task<ActionResult<bool>> HasReadingProgress(int libraryId)
        {
            var userId = await _unitOfWork.UserRepository.GetUserIdByUsernameAsync(User.GetUsername());
            var library = await _unitOfWork.LibraryRepository.GetLibraryForIdAsync(libraryId, LibraryIncludes.None);
            return Ok(await _unitOfWork.AppUserProgressRepository.UserHasProgress(library.Type, userId));
        }

        [HttpGet("has-library-access")]
        public async Task<ActionResult<bool>> HasLibraryAccess(int libraryId)
        {
            var libs = await _unitOfWork.LibraryRepository.GetLibraryDtosForUsernameAsync(User.GetUsername());
            return Ok(libs.Any(x => x.Id == libraryId));
        }

        [HttpPost("update-preferences")]
        public async Task<ActionResult<UserPreferencesDto>> UpdatePreferences(UserPreferencesDto preferencesDto)
        {
            var existingPreferences = await _unitOfWork.UserRepository.GetPreferencesAsync(User.GetUsername());

            existingPreferences.ReadingDirection = preferencesDto.ReadingDirection;
            existingPreferences.ScalingOption = preferencesDto.ScalingOption;
            existingPreferences.PageSplitOption = preferencesDto.PageSplitOption;
            existingPreferences.AutoCloseMenu = preferencesDto.AutoCloseMenu;
            existingPreferences.ShowScreenHints = preferencesDto.ShowScreenHints;
            existingPreferences.ReaderMode = preferencesDto.ReaderMode;
            existingPreferences.LayoutMode = preferencesDto.LayoutMode;
            existingPreferences.BackgroundColor = string.IsNullOrEmpty(preferencesDto.BackgroundColor) ? "#000000" : preferencesDto.BackgroundColor;
            existingPreferences.BookReaderMargin = preferencesDto.BookReaderMargin;
            existingPreferences.BookReaderLineSpacing = preferencesDto.BookReaderLineSpacing;
            existingPreferences.BookReaderFontFamily = preferencesDto.BookReaderFontFamily;
            existingPreferences.BookReaderDarkMode = preferencesDto.BookReaderDarkMode;
            existingPreferences.BookReaderFontSize = preferencesDto.BookReaderFontSize;
            existingPreferences.BookReaderTapToPaginate = preferencesDto.BookReaderTapToPaginate;
            existingPreferences.BookReaderReadingDirection = preferencesDto.BookReaderReadingDirection;
            preferencesDto.Theme ??= await _unitOfWork.SiteThemeRepository.GetDefaultTheme();
            existingPreferences.Theme = await _unitOfWork.SiteThemeRepository.GetThemeById(preferencesDto.Theme.Id);

            // TODO: Remove this code - this overrides layout mode to be single until the mode is released
            existingPreferences.LayoutMode = LayoutMode.Single;

            _unitOfWork.UserRepository.Update(existingPreferences);

            if (await _unitOfWork.CommitAsync())
            {
                return Ok(preferencesDto);
            }

            return BadRequest("There was an issue saving preferences.");
        }

        [HttpGet("get-preferences")]
        public async Task<ActionResult<UserPreferencesDto>> GetPreferences()
        {
            return _mapper.Map<UserPreferencesDto>(
                await _unitOfWork.UserRepository.GetPreferencesAsync(User.GetUsername()));

        }
    }
}
