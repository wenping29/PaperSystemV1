# Writing Platform Database Setup Guide

This document provides instructions for setting up the MySQL databases for the eight microservices: FileService, SearchService, FriendshipService, NotificationService, UserService, WritingService, AIService, and ChatService.

## Database Overview

| Service | Database Name | Tables | Purpose |
| --- | --- | --- | --- |
| FileService | `writing_platform_file` | `FileMetadata` | File uploads and metadata management |
| SearchService | `writing_platform_search` | `SearchHistories`, `SearchIndices`, `PopularSearchTerms` | Search functionality and analytics |
| FriendshipService | `writing_platform_friendship` | `Friendships`, `FriendRequests` | User relationships and friend requests |
| NotificationService | `writing_platform_notification` | `Notifications`, `NotificationTemplates`, `NotificationSettings` | User notifications and templates |
| UserService | `writing_platform_user` | `Users`, `UserProfiles`, `UserFollows`, `RefreshTokens`, `UserActivityLogs` | User management, authentication, and activity tracking |
| WritingService | `writing_platform_writing` | `Works`, `Categories`, `Templates`, `WorkVersions`, `WorkCollaborators` | Writing content management and collaboration |
| AIService | `writing_platform_ai` | `AIAuditLogs` | AI service audit and usage tracking |
| ChatService | `writing_platform_chat` | `ChatRooms`, `ChatRoomMembers`, `Messages`, `UserMessageReads` | Real-time chat and messaging |
| CommunityService | `writing_platform_community` | `Posts`, `Comments`, `Likes`, `Collections` | Community posts, comments, likes and collections |
| PaymentService | `writing_platform_payment` | `PaymentTransactions`, `RefundTransactions` | Payment and refund transaction management |

## Quick Setup

### 1. Install MySQL 8.0+
Ensure MySQL 8.0 or higher is installed and running.

### 2. Run the Full Schema Script
```bash
mysql -u root -p < database_schema_mysql.sql
```

This script will:
- Create all eight databases
- Create all tables with proper indexes
- Insert sample data for testing
- Create database users (optional)
- Create useful views for monitoring

### 3. Or Run the Basic Schema Script
For a clean setup without sample data:
```bash
mysql -u root -p < database_schema_basic_mysql.sql
```

## Database Connection Strings

Update the `appsettings.json` files in each service with the appropriate connection string:

### FileService (`src/Services/FileService/appsettings.json`)
```json
"ConnectionStrings": {
  "FileDatabase": "Server=localhost;Port=3306;Database=writing_platform_file;Uid=root;Pwd=your_password;Pooling=true;..."
}
```

### SearchService (`src/Services/SearchService/appsettings.json`)
```json
"ConnectionStrings": {
  "SearchDatabase": "Server=localhost;Port=3306;Database=writing_platform_search;Uid=root;Pwd=your_password;Pooling=true;..."
}
```

### FriendshipService (`src/Services/FriendshipService/appsettings.json`)
```json
"ConnectionStrings": {
  "FriendshipDatabase": "Server=localhost;Port=3306;Database=writing_platform_friendship;Uid=root;Pwd=your_password;Pooling=true;..."
}
```

### NotificationService (`src/Services/NotificationService/appsettings.json`)
```json
"ConnectionStrings": {
  "NotificationDatabase": "Server=localhost;Port=3306;Database=writing_platform_notification;Uid=root;Pwd=your_password;Pooling=true;..."
}
```

### UserService (`src/Services/UserService/appsettings.json`)
```json
"ConnectionStrings": {
  "UserDatabase": "Server=localhost;Port=3306;Database=writing_platform_user;Uid=root;Pwd=your_password;Pooling=true;..."
}
```

### WritingService (`src/Services/WritingService/appsettings.json`)
```json
"ConnectionStrings": {
  "WritingDatabase": "Server=localhost;Port=3306;Database=writing_platform_writing;Uid=root;Pwd=your_password;Pooling=true;..."
}
```

### AIService (`src/Services/AIService/appsettings.json`)
```json
"ConnectionStrings": {
  "AIDatabase": "Server=localhost;Port=3306;Database=writing_platform_ai;Uid=root;Pwd=your_password;Pooling=true;..."
}
```

### ChatService (`src/Services/ChatService/appsettings.json`)
```json
"ConnectionStrings": {
  "ChatDatabase": "Server=localhost;Port=3306;Database=writing_platform_chat;Uid=root;Pwd=your_password;Pooling=true;..."
}
```

### CommunityService (`src/Services/CommunityService/appsettings.json`)
```json
"ConnectionStrings": {
  "CommunityDatabase": "Server=localhost;Port=3306;Database=writing_platform_community;Uid=root;Pwd=your_password;Pooling=true;..."
}
```

### PaymentService (`src/Services/PaymentService/appsettings.json`)
```json
"ConnectionStrings": {
  "PaymentDatabase": "Server=localhost;Port=3306;Database=writing_platform_payment;Uid=root;Pwd=your_password;Pooling=true;..."
}
```

## Table Details

### FileService Tables

#### `FileMetadata`
Stores metadata for uploaded files.
- `FileId`: Unique identifier (UUID)
- `StoragePath`: Path to the physical file
- `FileType`: image, document, video, audio, archive, other
- `Status`: uploaded, processing, ready, failed, deleted
- `FileHash`: SHA256 hash for deduplication

### SearchService Tables

#### `SearchHistories`
Tracks user search queries for analytics.
- `Query`: The search term
- `SearchType`: writing, user, tag, all, comment, community
- `ResponseTime`: How long the search took

#### `SearchIndices`
Indexed content for fast searching.
- `IndexType`: writing, user, tag, comment, community
- `EntityId`: ID from the source service
- `Status`: pending, indexed, updating, error, deleted
- `RelevanceScore`: For ranking search results

#### `PopularSearchTerms`
Popular search terms for suggestions.
- `Term`: The search term
- `SearchCount`: How many times searched
- `IsTrending`: Currently trending flag

### FriendshipService Tables

#### `Friendships`
User friendship relationships.
- `UserId`, `FriendId`: User pair (unique constraint)
- `Status`: active, inactive, blocked, restricted
- `InteractionScore`: For friend suggestions
- `IsFavorite`: Favorite friend flag

#### `FriendRequests`
Friend request management.
- `RequesterId`, `ReceiverId`: Request user pair
- `Status`: pending, accepted, rejected, cancelled, expired
- `ExpiresAt`: Automatic expiration (default 7 days)

### NotificationService Tables

#### `Notifications`
User notifications.
- `Type`: system, friend_request, message, comment, like, etc.
- `Status`: unread, read, archived, deleted
- `Priority`: 0=normal, 1=important, 2=urgent
- `RelatedEntityType/Id`: Links to other entities (writings, comments, etc.)

#### `NotificationTemplates`
Templates for dynamic notifications.
- `Name`: Unique template identifier
- `TitleTemplate`, `ContentTemplate`: With {{variable}} placeholders
- `VariablesJson`: Variable definitions

#### `NotificationSettings`
User notification preferences.
- `NotificationType`: Which types of notifications
- `Channel`: in_app, email, sms, push, all
- `IsEnabled`: Whether the user wants this type

### CommunityService Tables

#### `Posts`
Community posts and discussions.
- `Title`: Post title (max 200 characters)
- `Content`: Post content (text)
- `AuthorId`: User ID of the post author
- `Category`: Fiction, NonFiction, Poetry, Drama, Essay, Blog, Tutorial, Other
- `Status`: Draft, Pending, Published, Rejected, Archived
- `Visibility`: Public, Private, FriendsOnly, SubscribersOnly
- `HotScore`: Calculated score for trending posts
- `WorkId`: Optional link to writing service work

#### `Comments`
Comments on posts with nested replies support.
- `Content`: Comment content (text)
- `PostId`: Related post ID
- `AuthorId`: User ID of comment author
- `ParentId`: Parent comment ID for nested replies
- `Status`: Active, Hidden, Deleted, Reported
- `LikeCount`: Number of likes on the comment

#### `Likes`
User likes for posts and comments.
- `UserId`: User who liked
- `TargetType`: Post or Comment
- `TargetId`: ID of the liked post or comment
- `CreatedAt`: When the like was created

#### `Collections`
User collections of posts.
- `UserId`: User who collected the post
- `PostId`: Collected post ID
- `Note`: Optional note about why collected
- `IsDeleted`: Soft delete flag

### PaymentService Tables

#### `PaymentTransactions`
Payment transaction records.
- `TransactionNo`: System-generated transaction number (unique)
- `GatewayTransactionNo`: Payment gateway transaction number
- `UserId`: User making the payment
- `PaymentGateway`: Alipay, WeChatPay, PayPal, Stripe, Manual
- `PaymentType`: Donation, Reward, Subscription, Purchase, Refund
- `Amount`: Transaction amount
- `Status`: Pending, Processing, Success, Failed, Cancelled, Refunded, PartiallyRefunded
- `TargetUserId`: For donations - target user ID
- `GatewayCallbackData`: JSON data from payment gateway callback

#### `RefundTransactions`
Refund transaction records.
- `RefundNo`: System-generated refund number (unique)
- `PaymentTransactionId`: Related payment transaction ID
- `Amount`: Refund amount
- `Status`: Pending, Processing, Success, Failed, Cancelled
- `Source`: UserRequest, AdminManual, SystemAuto, Dispute
- `Reason`: Reason for refund
- `GatewayRefundNo`: Payment gateway refund number

## Indexes

Each table has been optimized with appropriate indexes:
- Primary keys on all `Id` columns
- Unique constraints where needed (FileId, user pairs)
- Composite indexes for common query patterns
- Foreign key indexes for relationships

## Sample Data

The full schema script includes sample data for testing:

1. **FileService**: Sample file records
2. **SearchService**: Popular search terms
3. **NotificationService**: Notification templates (welcome, friend request, etc.)
4. **CommunityService**: Sample community post, comment, like, and collection
5. **PaymentService**: Sample payment and refund transactions

## Security Considerations

### Production Deployment

1. **Change Default Passwords**: Update all connection string passwords
2. **Create Dedicated Users**: Use the provided SQL to create service-specific users
3. **Restrict Permissions**: Grant only necessary permissions to each user
4. **Enable SSL/TLS**: Require encrypted connections
5. **Network Security**: Restrict database access to application servers only
6. **Regular Backups**: Implement a backup strategy

### Database User Creation (Optional)

The full schema script creates these users (adjust passwords):
- `writing_file_user` - FileService
- `writing_search_user` - SearchService  
- `writing_friendship_user` - FriendshipService
- `writing_notification_user` - NotificationService
- `writing_user_user` - UserService
- `writing_writing_user` - WritingService
- `writing_ai_user` - AIService
- `writing_chat_user` - ChatService
- `writing_community_user` - CommunityService
- `writing_payment_user` - PaymentService

## Monitoring Views

The full schema includes these useful views:

1. `vw_FileStats` - File type statistics
2. `vw_NotificationStats` - User notification counts
3. `vw_FriendshipStats` - User friendship statistics  
4. `vw_SearchStats` - Daily search statistics

## Troubleshooting

### Common Issues

1. **Character Set Issues**: All databases use `utf8mb4_unicode_ci` for full Unicode support
2. **Time Zone**: All timestamps are in UTC
3. **Connection Pooling**: Configured in connection strings
4. **Large Files**: Adjust `max_allowed_packet` in MySQL if uploading large files

### Migration Notes

When updating the schema:
1. Backup existing databases
2. Test changes in a development environment
3. Use migrations for incremental changes
4. Update connection strings if database names change

## Next Steps

After database setup:

1. **Run Database Migrations**: Use Entity Framework Core migrations
2. **Test Connections**: Verify each service can connect to its database
3. **Load Test Data**: Populate with realistic data for testing
4. **Performance Tuning**: Adjust MySQL configuration based on usage patterns
5. **Monitoring Setup**: Configure database monitoring and alerts

## Support

For database-related issues:
1. Check MySQL error logs
2. Verify connection strings
3. Ensure proper permissions
4. Check disk space and memory limits
