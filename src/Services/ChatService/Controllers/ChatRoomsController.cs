using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChatService.DTOs;
using ChatService.Interfaces;

namespace ChatService.Controllers
{
    [ApiController]
    [Route("api/v1/chatrooms")]
    [Authorize]
    public class ChatRoomsController : ControllerBase
    {
        private readonly ILogger<ChatRoomsController> _logger;
        private readonly IChatService _chatService;
        private readonly IMapper _mapper;

        public ChatRoomsController(
            ILogger<ChatRoomsController> logger,
            IChatService chatService,
            IMapper mapper)
        {
            _logger = logger;
            _chatService = chatService;
            _mapper = mapper;
        }

        /// <summary>
        /// 获取聊天室列表
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetChatRooms([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var chatRooms = await _chatService.GetChatRoomsByUserAsync(currentUserId, page, pageSize);
                var response = _mapper.Map<IEnumerable<ChatRoomListResponse>>(chatRooms);

                // 设置当前用户成员信息
                foreach (var chatRoom in response)
                {
                    if (chatRoom.Id > 0)
                    {
                        var member = await _chatService.GetChatRoomMemberAsync(chatRoom.Id, currentUserId);
                        if (member != null)
                        {
                            chatRoom.CurrentUserMember = _mapper.Map<ChatRoomMemberInfo>(member);
                        }
                    }
                }

                return Ok(new
                {
                    chatRooms = response,
                    page,
                    pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat rooms");
                return StatusCode(500, new { error = "An error occurred while getting chat rooms" });
            }
        }

        /// <summary>
        /// 获取公开聊天室列表
        /// </summary>
        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicChatRooms([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var chatRooms = await _chatService.GetPublicChatRoomsAsync(page, pageSize);
                var response = _mapper.Map<IEnumerable<ChatRoomListResponse>>(chatRooms);
                return Ok(new
                {
                    chatRooms = response,
                    page,
                    pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public chat rooms");
                return StatusCode(500, new { error = "An error occurred while getting public chat rooms" });
            }
        }

        /// <summary>
        /// 获取特定聊天室
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChatRoom(long id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var chatRoom = await _chatService.GetChatRoomByIdAsync(id);
                if (chatRoom == null)
                    return NotFound(new { error = "Chat room not found" });

                // 检查权限：公开聊天室或成员
                if (!chatRoom.IsPublic && !await _chatService.IsChatRoomMemberAsync(id, currentUserId))
                    return Forbid();

                var response = _mapper.Map<ChatRoomResponse>(chatRoom);

                // 设置当前用户成员信息
                var member = await _chatService.GetChatRoomMemberAsync(id, currentUserId);
                if (member != null)
                {
                    response.CurrentUserMember = _mapper.Map<ChatRoomMemberInfo>(member);
                }

                // 设置成员列表（仅限成员查看）
                if (await _chatService.IsChatRoomMemberAsync(id, currentUserId))
                {
                    var members = await _chatService.GetChatRoomMembersAsync(id, 1, 50);
                    response.Members = _mapper.Map<IEnumerable<ChatRoomMemberInfo>>(members);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat room {ChatRoomId}", id);
                return StatusCode(500, new { error = "An error occurred while getting chat room" });
            }
        }

        /// <summary>
        /// 创建聊天室
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateChatRoom([FromBody] CreateChatRoomRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var chatRoom = await _chatService.CreateChatRoomAsync(
                    currentUserId,
                    request.Name,
                    request.RoomType,
                    request.Description,
                    request.IsPublic,
                    request.MaxMembers);

                var response = _mapper.Map<ChatRoomResponse>(chatRoom);
                return CreatedAtAction(nameof(GetChatRoom), new { id = chatRoom.Id }, response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chat room");
                return StatusCode(500, new { error = "An error occurred while creating chat room" });
            }
        }

        /// <summary>
        /// 更新聊天室
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChatRoom(long id, [FromBody] UpdateChatRoomRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var chatRoom = await _chatService.UpdateChatRoomAsync(
                    id,
                    currentUserId,
                    request.Name,
                    request.Description,
                    request.IsPublic);

                if (chatRoom == null)
                    return NotFound(new { error = "Chat room not found or no permission to update" });

                var response = _mapper.Map<ChatRoomResponse>(chatRoom);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating chat room {ChatRoomId}", id);
                return StatusCode(500, new { error = "An error occurred while updating chat room" });
            }
        }

        /// <summary>
        /// 删除聊天室
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChatRoom(long id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _chatService.DeleteChatRoomAsync(id, currentUserId);
                if (!result)
                    return NotFound(new { error = "Chat room not found or no permission to delete" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting chat room {ChatRoomId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting chat room" });
            }
        }

        /// <summary>
        /// 加入聊天室
        /// </summary>
        [HttpPost("{id}/join")]
        public async Task<IActionResult> JoinChatRoom(long id, [FromBody] JoinChatRoomRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _chatService.JoinChatRoomAsync(id, currentUserId, request.InviteCode);
                if (!result)
                    return BadRequest(new { error = "Cannot join chat room. Invalid invite code or room is full." });

                return Ok(new { message = "Successfully joined chat room" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining chat room {ChatRoomId}", id);
                return StatusCode(500, new { error = "An error occurred while joining chat room" });
            }
        }

        /// <summary>
        /// 离开聊天室
        /// </summary>
        [HttpPost("{id}/leave")]
        public async Task<IActionResult> LeaveChatRoom(long id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _chatService.LeaveChatRoomAsync(id, currentUserId);
                if (!result)
                    return BadRequest(new { error = "Cannot leave chat room. You may be the owner." });

                return Ok(new { message = "Successfully left chat room" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving chat room {ChatRoomId}", id);
                return StatusCode(500, new { error = "An error occurred while leaving chat room" });
            }
        }

        /// <summary>
        /// 获取聊天室成员
        /// </summary>
        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetChatRoomMembers(long id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!await _chatService.IsChatRoomMemberAsync(id, currentUserId))
                    return Forbid();

                var members = await _chatService.GetChatRoomMembersAsync(id, page, pageSize);
                var response = _mapper.Map<IEnumerable<ChatRoomMemberInfo>>(members);
                return Ok(new
                {
                    members = response,
                    page,
                    pageSize,
                    total = await _chatService.GetChatRoomMemberCountAsync(id)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat room members for chat room {ChatRoomId}", id);
                return StatusCode(500, new { error = "An error occurred while getting chat room members" });
            }
        }

        /// <summary>
        /// 添加成员
        /// </summary>
        [HttpPost("{id}/members")]
        public async Task<IActionResult> AddMember(long id, [FromBody] AddMemberRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _chatService.AddMemberAsync(id, currentUserId, request.UserId, request.Role);
                if (!result)
                    return BadRequest(new { error = "Cannot add member. Check permissions or user may already be a member." });

                return Ok(new { message = "Member added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding member to chat room {ChatRoomId}", id);
                return StatusCode(500, new { error = "An error occurred while adding member" });
            }
        }

        /// <summary>
        /// 移除成员
        /// </summary>
        [HttpDelete("{id}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(long id, long userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _chatService.RemoveMemberAsync(id, currentUserId, userId);
                if (!result)
                    return BadRequest(new { error = "Cannot remove member. Check permissions or user may not be a member." });

                return Ok(new { message = "Member removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing member from chat room {ChatRoomId}", id);
                return StatusCode(500, new { error = "An error occurred while removing member" });
            }
        }

        /// <summary>
        /// 更新成员角色
        /// </summary>
        [HttpPut("{id}/members/{userId}/role")]
        public async Task<IActionResult> UpdateMemberRole(long id, long userId, [FromBody] UpdateMemberRoleRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _chatService.UpdateMemberRoleAsync(id, currentUserId, userId, request.Role);
                if (!result)
                    return BadRequest(new { error = "Cannot update member role. Check permissions." });

                return Ok(new { message = "Member role updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating member role in chat room {ChatRoomId}", id);
                return StatusCode(500, new { error = "An error occurred while updating member role" });
            }
        }

        /// <summary>
        /// 生成邀请码
        /// </summary>
        [HttpPost("{id}/invite")]
        public async Task<IActionResult> GenerateInviteCode(long id, [FromBody] GenerateInviteCodeRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var inviteCode = await _chatService.GenerateInviteCodeAsync(id, currentUserId, request.ExpiresAt);
                if (inviteCode == null)
                    return BadRequest(new { error = "Cannot generate invite code. Check permissions." });

                var response = new InviteCodeResponse
                {
                    InviteCode = inviteCode,
                    ExpiresAt = request.ExpiresAt,
                    InviteUrl = $"/chat/join?code={inviteCode}" // 示例URL
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invite code for chat room {ChatRoomId}", id);
                return StatusCode(500, new { error = "An error occurred while generating invite code" });
            }
        }

        /// <summary>
        /// 撤销邀请码
        /// </summary>
        [HttpDelete("{id}/invite")]
        public async Task<IActionResult> RevokeInviteCode(long id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _chatService.RevokeInviteCodeAsync(id, currentUserId);
                if (!result)
                    return BadRequest(new { error = "Cannot revoke invite code. Check permissions." });

                return Ok(new { message = "Invite code revoked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking invite code for chat room {ChatRoomId}", id);
                return StatusCode(500, new { error = "An error occurred while revoking invite code" });
            }
        }

        /// <summary>
        /// 通过邀请码获取聊天室
        /// </summary>
        [HttpGet("invite/{inviteCode}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetChatRoomByInviteCode(string inviteCode)
        {
            try
            {
                var chatRoom = await _chatService.GetChatRoomByInviteCodeAsync(inviteCode);
                if (chatRoom == null)
                    return NotFound(new { error = "Invalid or expired invite code" });

                var response = _mapper.Map<ChatRoomResponse>(chatRoom);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat room by invite code");
                return StatusCode(500, new { error = "An error occurred while getting chat room by invite code" });
            }
        }

        /// <summary>
        /// 搜索聊天室
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchChatRooms([FromQuery] SearchChatRoomsRequest request)
        {
            try
            {
                var chatRooms = await _chatService.SearchChatRoomsAsync(request.SearchTerm, request.Page, request.PageSize);
                var response = _mapper.Map<IEnumerable<ChatRoomListResponse>>(chatRooms);
                return Ok(new
                {
                    chatRooms = response,
                    page = request.Page,
                    pageSize = request.PageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching chat rooms");
                return StatusCode(500, new { error = "An error occurred while searching chat rooms" });
            }
        }

        /// <summary>
        /// 获取在线用户
        /// </summary>
        [HttpGet("{id}/online-users")]
        public async Task<IActionResult> GetOnlineUsers(long id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!await _chatService.IsChatRoomMemberAsync(id, currentUserId))
                    return Forbid();

                var onlineUsers = await _chatService.GetOnlineUsersAsync(id);
                return Ok(new { onlineUsers });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting online users for chat room {ChatRoomId}", id);
                return StatusCode(500, new { error = "An error occurred while getting online users" });
            }
        }

        /// <summary>
        /// 添加成员请求DTO
        /// </summary>
        public class AddMemberRequest
        {
            public long UserId { get; set; }
            public string Role { get; set; } = "Member";
        }

        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }
            return userId;
        }
    }
}