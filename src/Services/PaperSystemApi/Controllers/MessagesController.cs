using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaperSystemApi.Chat.DTOs;
using PaperSystemApi.Chat.Interfaces;

namespace PaperSystemApi.Controllers
{
    [ApiController]
    [Route("api/v1/messages")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly ILogger<MessagesController> _logger;
        private readonly IChatService _chatService;
        private readonly IMapper _mapper;

        public MessagesController(
            ILogger<MessagesController> logger,
            IChatService chatService,
            IMapper mapper)
        {
            _logger = logger;
            _chatService = chatService;
            _mapper = mapper;
        }

        /// <summary>
        /// 获取消息列表
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMessages([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var messages = await _chatService.GetMessagesAsync(page, pageSize);
                var response = _mapper.Map<IEnumerable<MessageListResponse>>(messages);

                // 设置是否为自己的消息
                var currentUserId = GetCurrentUserId();
                foreach (var msg in response)
                {
                    msg.IsOwnMessage = msg.SenderId == currentUserId;
                }

                return Ok(new
                {
                    messages = response,
                    page,
                    pageSize,
                    total = await _chatService.GetUnreadMessageCountAsync(currentUserId)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages");
                return StatusCode(500, new { error = "An error occurred while getting messages" });
            }
        }

        /// <summary>
        /// 获取特定消息
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessage(long id)
        {
            try
            {
                var message = await _chatService.GetMessageByIdAsync(id);
                if (message == null)
                    return NotFound(new { error = "Message not found" });

                // 检查权限
                var currentUserId = GetCurrentUserId();
                if (message.SenderId != currentUserId &&
                    message.ReceiverId != currentUserId &&
                    !(message.ChatRoomId.HasValue && await _chatService.IsChatRoomMemberAsync(message.ChatRoomId.Value, currentUserId)))
                {
                    return Forbid();
                }

                var response = _mapper.Map<MessageResponse>(message);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting message {MessageId}", id);
                return StatusCode(500, new { error = "An error occurred while getting message" });
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var message = await _chatService.SendMessageAsync(
                    currentUserId,
                    request.ReceiverId,
                    request.ChatRoomId,
                    request.Content,
                    request.MessageType,
                    request.FileUrl,
                    request.ParentMessageId);

                var response = _mapper.Map<MessageResponse>(message);
                return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return StatusCode(500, new { error = "An error occurred while sending message" });
            }
        }

        /// <summary>
        /// 更新消息
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMessage(long id, [FromBody] UpdateMessageRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var message = await _chatService.GetMessageByIdAsync(id);
                if (message == null)
                    return NotFound(new { error = "Message not found" });

                // 只有发送者可以编辑消息
                if (message.SenderId != currentUserId)
                    return Forbid();

                var updatedMessage = await _chatService.UpdateMessageAsync(id, request.Content);
                var response = _mapper.Map<MessageResponse>(updatedMessage);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating message {MessageId}", id);
                return StatusCode(500, new { error = "An error occurred while updating message" });
            }
        }

        /// <summary>
        /// 删除消息
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(long id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _chatService.DeleteMessageAsync(id, currentUserId);
                if (!result)
                    return NotFound(new { error = "Message not found or no permission to delete" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting message" });
            }
        }

        /// <summary>
        /// 标记消息为已读
        /// </summary>
        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(long id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _chatService.MarkMessageAsReadAsync(id, currentUserId);
                if (!result)
                    return NotFound(new { error = "Message not found or no permission to mark as read" });

                return Ok(new { message = "Message marked as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message {MessageId} as read", id);
                return StatusCode(500, new { error = "An error occurred while marking message as read" });
            }
        }

        /// <summary>
        /// 获取聊天室消息
        /// </summary>
        [HttpGet("chatroom/{chatRoomId}")]
        public async Task<IActionResult> GetChatRoomMessages(long chatRoomId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!await _chatService.IsChatRoomMemberAsync(chatRoomId, currentUserId))
                    return Forbid();

                var messages = await _chatService.GetMessagesByChatRoomAsync(chatRoomId, page, pageSize);
                var response = _mapper.Map<IEnumerable<MessageListResponse>>(messages);

                // 设置是否为自己的消息
                foreach (var msg in response)
                {
                    msg.IsOwnMessage = msg.SenderId == currentUserId;
                }

                return Ok(new
                {
                    messages = response,
                    page,
                    pageSize,
                    total = await _chatService.GetUnreadMessageCountAsync(currentUserId, chatRoomId)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat room messages for chat room {ChatRoomId}", chatRoomId);
                return StatusCode(500, new { error = "An error occurred while getting chat room messages" });
            }
        }

        /// <summary>
        /// 获取与特定用户的对话
        /// </summary>
        [HttpGet("conversation/{userId}")]
        public async Task<IActionResult> GetConversation(long userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var messages = await _chatService.GetConversationAsync(currentUserId, userId, page, pageSize);
                var response = _mapper.Map<IEnumerable<MessageListResponse>>(messages);

                // 设置是否为自己的消息
                foreach (var msg in response)
                {
                    msg.IsOwnMessage = msg.SenderId == currentUserId;
                }

                return Ok(new
                {
                    messages = response,
                    page,
                    pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation with user {UserId}", userId);
                return StatusCode(500, new { error = "An error occurred while getting conversation" });
            }
        }

        /// <summary>
        /// 获取未读消息
        /// </summary>
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadMessages([FromQuery] long? chatRoomId = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var messages = await _chatService.GetUnreadMessagesAsync(currentUserId, chatRoomId);
                var response = _mapper.Map<IEnumerable<MessageListResponse>>(messages);

                // 设置是否为自己的消息
                foreach (var msg in response)
                {
                    msg.IsOwnMessage = msg.SenderId == currentUserId;
                }

                return Ok(new
                {
                    messages = response,
                    total = response.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread messages");
                return StatusCode(500, new { error = "An error occurred while getting unread messages" });
            }
        }

        /// <summary>
        /// 搜索消息
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchMessages([FromQuery] SearchMessagesRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                if (request.ChatRoomId.HasValue)
                {
                    if (!await _chatService.IsChatRoomMemberAsync(request.ChatRoomId.Value, currentUserId))
                        return Forbid();
                }

                var messages = await _chatService.SearchMessagesAsync(
                    request.ChatRoomId,
                    request.SearchTerm,
                    request.Page,
                    request.PageSize);

                var response = _mapper.Map<IEnumerable<MessageListResponse>>(messages);

                // 设置是否为自己的消息
                foreach (var msg in response)
                {
                    msg.IsOwnMessage = msg.SenderId == currentUserId;
                }

                return Ok(new
                {
                    messages = response,
                    page = request.Page,
                    pageSize = request.PageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching messages");
                return StatusCode(500, new { error = "An error occurred while searching messages" });
            }
        }

        /// <summary>
        /// 获取未读消息数量
        /// </summary>
        [HttpGet("unread/count")]
        public async Task<IActionResult> GetUnreadCount([FromQuery] long? chatRoomId = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var count = await _chatService.GetUnreadMessageCountAsync(currentUserId, chatRoomId);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread message count");
                return StatusCode(500, new { error = "An error occurred while getting unread message count" });
            }
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