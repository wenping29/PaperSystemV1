-- Writing Platform Database Schema
-- Generated for MySQL 8.0+
-- Four services: FileService, SearchService, FriendshipService, NotificationService

-- ============================================
-- 1. File Service Database: writing_platform_file
-- ============================================
CREATE DATABASE IF NOT EXISTS `writing_platform_file`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE `writing_platform_file`;

-- FileMetadata table
CREATE TABLE IF NOT EXISTS `FileMetadata` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `FileId` VARCHAR(36) NOT NULL,
    `OriginalFileName` VARCHAR(255) NOT NULL,
    `StoragePath` VARCHAR(100) NOT NULL,
    `ContentType` VARCHAR(50) NOT NULL,
    `SizeInBytes` BIGINT NOT NULL,
    `FileType` VARCHAR(50) NOT NULL,
    `UploadedByUserId` BIGINT NULL,
    `Description` VARCHAR(500) NULL,
    `Status` VARCHAR(20) NOT NULL DEFAULT 'uploaded',
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL,
    `ExpiresAt` DATETIME NULL,
    `IsDeleted` BIT NOT NULL DEFAULT 0,
    `MetadataJson` VARCHAR(1000) NULL,
    `FileHash` VARCHAR(64) NULL,
    `AccessCount` INT NOT NULL DEFAULT 0,
    `LastAccessedAt` DATETIME NULL,
    `Tags` VARCHAR(500) NULL,

    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_FileMetadata_FileId` (`FileId`),
    INDEX `IX_FileMetadata_UploadedByUserId` (`UploadedByUserId`),
    INDEX `IX_FileMetadata_FileType` (`FileType`),
    INDEX `IX_FileMetadata_Status` (`Status`),
    INDEX `IX_FileMetadata_CreatedAt` (`CreatedAt`),
    INDEX `IX_FileMetadata_ExpiresAt` (`ExpiresAt`),
    INDEX `IX_FileMetadata_FileHash` (`FileHash`),
    INDEX `IX_FileMetadata_LastAccessedAt` (`LastAccessedAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- 2. Search Service Database: writing_platform_search
-- ============================================
CREATE DATABASE IF NOT EXISTS `writing_platform_search`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE `writing_platform_search`;

-- SearchHistory table
CREATE TABLE IF NOT EXISTS `SearchHistories` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `Query` VARCHAR(500) NOT NULL,
    `SearchType` VARCHAR(50) NULL,
    `UserId` BIGINT NULL,
    `IpAddress` VARCHAR(45) NULL,
    `UserAgent` VARCHAR(500) NULL,
    `DeviceType` VARCHAR(100) NULL,
    `ResultCount` INT NOT NULL,
    `IsSuccessful` BIT NOT NULL DEFAULT 1,
    `ErrorMessage` VARCHAR(500) NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `ResponseTime` TIME NOT NULL,
    `MetadataJson` VARCHAR(1000) NULL,

    PRIMARY KEY (`Id`),
    INDEX `IX_SearchHistories_Query` (`Query`),
    INDEX `IX_SearchHistories_UserId` (`UserId`),
    INDEX `IX_SearchHistories_SearchType` (`SearchType`),
    INDEX `IX_SearchHistories_CreatedAt` (`CreatedAt`),
    INDEX `IX_SearchHistories_UserId_CreatedAt` (`UserId`, `CreatedAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- SearchIndex table
CREATE TABLE IF NOT EXISTS `SearchIndices` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `IndexType` VARCHAR(50) NOT NULL,
    `EntityId` BIGINT NOT NULL,
    `Status` VARCHAR(20) NOT NULL DEFAULT 'indexed',
    `ErrorMessage` VARCHAR(500) NULL,
    `Version` INT NOT NULL DEFAULT 1,
    `IndexedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `LastUpdatedAt` DATETIME NULL,
    `LastAccessedAt` DATETIME NULL,
    `AccessCount` INT NOT NULL DEFAULT 0,
    `MetadataJson` VARCHAR(1000) NULL,
    `Tags` VARCHAR(500) NULL,
    `RelevanceScore` DOUBLE NOT NULL DEFAULT 1.0,

    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_SearchIndices_IndexType_EntityId` (`IndexType`, `EntityId`),
    INDEX `IX_SearchIndices_IndexType` (`IndexType`),
    INDEX `IX_SearchIndices_EntityId` (`EntityId`),
    INDEX `IX_SearchIndices_Status` (`Status`),
    INDEX `IX_SearchIndices_IndexedAt` (`IndexedAt`),
    INDEX `IX_SearchIndices_IndexType_Status` (`IndexType`, `Status`),
    INDEX `IX_SearchIndices_LastUpdatedAt` (`LastUpdatedAt`),
    INDEX `IX_SearchIndices_LastAccessedAt` (`LastAccessedAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- PopularSearchTerm table
CREATE TABLE IF NOT EXISTS `PopularSearchTerms` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `Term` VARCHAR(200) NOT NULL,
    `SearchCount` INT NOT NULL DEFAULT 0,
    `FirstSearchedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `LastSearchedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `IsTrending` BIT NOT NULL DEFAULT 0,
    `Category` VARCHAR(50) NULL,
    `Rank` INT NOT NULL DEFAULT 0,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (`Id`),
    INDEX `IX_PopularSearchTerms_Term` (`Term`),
    INDEX `IX_PopularSearchTerms_SearchCount` (`SearchCount`),
    INDEX `IX_PopularSearchTerms_LastSearchedAt` (`LastSearchedAt`),
    INDEX `IX_PopularSearchTerms_IsTrending` (`IsTrending`),
    INDEX `IX_PopularSearchTerms_Rank` (`Rank`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- 3. Friendship Service Database: writing_platform_friendship
-- ============================================
CREATE DATABASE IF NOT EXISTS `writing_platform_friendship`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE `writing_platform_friendship`;

-- Friendship table
CREATE TABLE IF NOT EXISTS `Friendships` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `UserId` BIGINT NOT NULL,
    `FriendId` BIGINT NOT NULL,
    `Status` VARCHAR(20) NOT NULL DEFAULT 'active',
    `Alias` VARCHAR(100) NULL,
    `Note` VARCHAR(500) NULL,
    `IsFavorite` BIT NOT NULL DEFAULT 0,
    `InteractionScore` INT NOT NULL DEFAULT 0,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `LastInteractedAt` DATETIME NULL,
    `IsDeleted` BIT NOT NULL DEFAULT 0,
    `MetadataJson` VARCHAR(1000) NULL,
    `Tags` VARCHAR(500) NULL,
    `PrivacySettings` VARCHAR(500) NULL,

    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_Friendships_UserId_FriendId` (`UserId`, `FriendId`),
    INDEX `IX_Friendships_UserId` (`UserId`),
    INDEX `IX_Friendships_FriendId` (`FriendId`),
    INDEX `IX_Friendships_Status` (`Status`),
    INDEX `IX_Friendships_CreatedAt` (`CreatedAt`),
    INDEX `IX_Friendships_LastInteractedAt` (`LastInteractedAt`),
    INDEX `IX_Friendships_UserId_IsFavorite` (`UserId`, `IsFavorite`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- FriendRequest table
CREATE TABLE IF NOT EXISTS `FriendRequests` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `RequesterId` BIGINT NOT NULL,
    `ReceiverId` BIGINT NOT NULL,
    `Status` VARCHAR(20) NOT NULL DEFAULT 'pending',
    `Message` VARCHAR(500) NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL,
    `RespondedAt` DATETIME NULL,
    `ResponseMessage` VARCHAR(1000) NULL,
    `ExpiresAt` DATETIME NULL,
    `IsDeleted` BIT NOT NULL DEFAULT 0,

    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_FriendRequests_RequesterId_ReceiverId_Status` (`RequesterId`, `ReceiverId`, `Status`),
    INDEX `IX_FriendRequests_RequesterId` (`RequesterId`),
    INDEX `IX_FriendRequests_ReceiverId` (`ReceiverId`),
    INDEX `IX_FriendRequests_Status` (`Status`),
    INDEX `IX_FriendRequests_CreatedAt` (`CreatedAt`),
    INDEX `IX_FriendRequests_ExpiresAt` (`ExpiresAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- 4. Notification Service Database: writing_platform_notification
-- ============================================
CREATE DATABASE IF NOT EXISTS `writing_platform_notification`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE `writing_platform_notification`;

-- Notification table
CREATE TABLE IF NOT EXISTS `Notifications` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `UserId` BIGINT NOT NULL,
    `Type` VARCHAR(50) NOT NULL DEFAULT 'system',
    `Title` VARCHAR(200) NOT NULL,
    `Content` VARCHAR(2000) NOT NULL,
    `Status` VARCHAR(20) NOT NULL DEFAULT 'unread',
    `IsImportant` BIT NOT NULL DEFAULT 0,
    `MetadataJson` VARCHAR(1000) NULL,
    `SourceUserId` BIGINT NULL,
    `RelatedEntityType` VARCHAR(50) NULL,
    `RelatedEntityId` BIGINT NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `ReadAt` DATETIME NULL,
    `ExpiresAt` DATETIME NULL,
    `UpdatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `IsDeleted` BIT NOT NULL DEFAULT 0,
    `Priority` INT NOT NULL DEFAULT 0,
    `ActionUrl` VARCHAR(500) NULL,
    `Icon` VARCHAR(100) NULL,
    `Tags` VARCHAR(500) NULL,

    PRIMARY KEY (`Id`),
    INDEX `IX_Notifications_UserId` (`UserId`),
    INDEX `IX_Notifications_UserId_Status` (`UserId`, `Status`),
    INDEX `IX_Notifications_UserId_Type` (`UserId`, `Type`),
    INDEX `IX_Notifications_Type` (`Type`),
    INDEX `IX_Notifications_Status` (`Status`),
    INDEX `IX_Notifications_CreatedAt` (`CreatedAt`),
    INDEX `IX_Notifications_ExpiresAt` (`ExpiresAt`),
    INDEX `IX_Notifications_UserId_IsImportant` (`UserId`, `IsImportant`),
    INDEX `IX_Notifications_UserId_Priority` (`UserId`, `Priority`),
    INDEX `IX_Notifications_RelatedEntityType_RelatedEntityId` (`RelatedEntityType`, `RelatedEntityId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- NotificationTemplate table
CREATE TABLE IF NOT EXISTS `NotificationTemplates` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `Name` VARCHAR(100) NOT NULL,
    `Type` VARCHAR(50) NOT NULL,
    `TitleTemplate` VARCHAR(200) NOT NULL,
    `ContentTemplate` VARCHAR(2000) NOT NULL,
    `VariablesJson` VARCHAR(1000) NULL,
    `IsActive` BIT NOT NULL DEFAULT 1,
    `Description` VARCHAR(500) NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `IsDeleted` BIT NOT NULL DEFAULT 0,

    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_NotificationTemplates_Name` (`Name`),
    INDEX `IX_NotificationTemplates_Type` (`Type`),
    INDEX `IX_NotificationTemplates_IsActive` (`IsActive`),
    INDEX `IX_NotificationTemplates_CreatedAt` (`CreatedAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- NotificationSettings table
CREATE TABLE IF NOT EXISTS `NotificationSettings` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `UserId` BIGINT NOT NULL,
    `NotificationType` VARCHAR(50) NOT NULL,
    `Channel` VARCHAR(20) NOT NULL DEFAULT 'in_app',
    `IsEnabled` BIT NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_NotificationSettings_UserId_NotificationType_Channel` (`UserId`, `NotificationType`, `Channel`),
    INDEX `IX_NotificationSettings_UserId` (`UserId`),
    INDEX `IX_NotificationSettings_NotificationType` (`NotificationType`),
    INDEX `IX_NotificationSettings_Channel` (`Channel`),
    INDEX `IX_NotificationSettings_IsEnabled` (`IsEnabled`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- Sample Data for Testing
-- ============================================

USE `writing_platform_notification`;

-- Insert sample notification types
INSERT INTO `NotificationTemplates` (`Name`, `Type`, `TitleTemplate`, `ContentTemplate`, `VariablesJson`, `Description`, `IsActive`) VALUES
('welcome_message', 'system', '欢迎来到写作平台！', '亲爱的{{UserName}}，欢迎加入写作平台！我们很高兴您的到来。', '{"UserName": "用户姓名"}', '新用户欢迎消息模板', 1),
('friend_request', 'friend_request', '新的好友请求', '{{RequesterName}} 想添加您为好友', '{"RequesterName": "请求者姓名"}', '好友请求通知模板', 1),
('writing_published', 'writing_published', '您的作品已发布', '您的作品《{{WritingTitle}}》已成功发布！', '{"WritingTitle": "作品标题"}', '作品发布成功通知模板', 1);

USE `writing_platform_friendship`;

-- Insert sample friend request status
-- (No sample data needed for basic schema)

USE `writing_platform_search`;

-- Insert sample popular search terms
INSERT INTO `PopularSearchTerms` (`Term`, `SearchCount`, `IsTrending`, `Category`, `Rank`) VALUES
('科幻小说', 150, 1, 'writing', 1),
('诗歌创作', 120, 1, 'writing', 2),
('散文随笔', 95, 0, 'writing', 3),
('写作技巧', 200, 1, 'tips', 1),
('文学评论', 75, 0, 'community', 2);

USE `writing_platform_file`;

-- Insert sample file metadata
INSERT INTO `FileMetadata` (`FileId`, `OriginalFileName`, `StoragePath`, `ContentType`, `SizeInBytes`, `FileType`, `Status`) VALUES
(UUID(), 'avatar.jpg', '/uploads/avatars/avatar1.jpg', 'image/jpeg', 204800, 'image', 'ready'),
(UUID(), 'document.pdf', '/uploads/documents/doc1.pdf', 'application/pdf', 512000, 'document', 'ready'),
(UUID(), 'cover_image.png', '/uploads/covers/cover1.png', 'image/png', 153600, 'image', 'ready');

-- ============================================
-- Database Users and Permissions
-- ============================================

-- Create database users (adjust passwords in production)
CREATE USER IF NOT EXISTS 'writing_file_user'@'%' IDENTIFIED BY 'FileServicePassword123!';
CREATE USER IF NOT EXISTS 'writing_search_user'@'%' IDENTIFIED BY 'SearchServicePassword123!';
CREATE USER IF NOT EXISTS 'writing_friendship_user'@'%' IDENTIFIED BY 'FriendshipServicePassword123!';
CREATE USER IF NOT EXISTS 'writing_notification_user'@'%' IDENTIFIED BY 'NotificationServicePassword123!';
CREATE USER IF NOT EXISTS 'writing_user_user'@'%' IDENTIFIED BY 'UserServicePassword123!';
CREATE USER IF NOT EXISTS 'writing_writing_user'@'%' IDENTIFIED BY 'WritingServicePassword123!';
CREATE USER IF NOT EXISTS 'writing_ai_user'@'%' IDENTIFIED BY 'AIServicePassword123!';
CREATE USER IF NOT EXISTS 'writing_chat_user'@'%' IDENTIFIED BY 'ChatServicePassword123!';
CREATE USER IF NOT EXISTS 'writing_community_user'@'%' IDENTIFIED BY 'CommunityServicePassword123!';
CREATE USER IF NOT EXISTS 'writing_payment_user'@'%' IDENTIFIED BY 'PaymentServicePassword123!';

-- Grant permissions
GRANT ALL PRIVILEGES ON `writing_platform_file`.* TO 'writing_file_user'@'%';
GRANT ALL PRIVILEGES ON `writing_platform_search`.* TO 'writing_search_user'@'%';
GRANT ALL PRIVILEGES ON `writing_platform_friendship`.* TO 'writing_friendship_user'@'%';
GRANT ALL PRIVILEGES ON `writing_platform_notification`.* TO 'writing_notification_user'@'%';
GRANT ALL PRIVILEGES ON `writing_platform_user`.* TO 'writing_user_user'@'%';
GRANT ALL PRIVILEGES ON `writing_platform_writing`.* TO 'writing_writing_user'@'%';
GRANT ALL PRIVILEGES ON `writing_platform_ai`.* TO 'writing_ai_user'@'%';
GRANT ALL PRIVILEGES ON `writing_platform_chat`.* TO 'writing_chat_user'@'%';
GRANT ALL PRIVILEGES ON `writing_platform_community`.* TO 'writing_community_user'@'%';
GRANT ALL PRIVILEGES ON `writing_platform_payment`.* TO 'writing_payment_user'@'%';

FLUSH PRIVILEGES;

-- ============================================
-- Database Maintenance and Monitoring Views
-- ============================================

USE `writing_platform_file`;

-- View for file statistics
CREATE OR REPLACE VIEW `vw_FileStats` AS
SELECT
    FileType,
    COUNT(*) as TotalFiles,
    SUM(SizeInBytes) as TotalSizeBytes,
    AVG(SizeInBytes) as AvgFileSize,
    MAX(CreatedAt) as LatestUpload,
    SUM(AccessCount) as TotalAccesses
FROM FileMetadata
WHERE IsDeleted = 0
GROUP BY FileType;

USE `writing_platform_notification`;

-- View for notification statistics
CREATE OR REPLACE VIEW `vw_NotificationStats` AS
SELECT
    UserId,
    COUNT(*) as TotalNotifications,
    SUM(CASE WHEN Status = 'unread' THEN 1 ELSE 0 END) as UnreadNotifications,
    SUM(CASE WHEN IsImportant = 1 THEN 1 ELSE 0 END) as ImportantNotifications,
    MAX(CreatedAt) as LastNotificationAt
FROM Notifications
WHERE IsDeleted = 0
GROUP BY UserId;

USE `writing_platform_friendship`;

-- View for friendship statistics
CREATE OR REPLACE VIEW `vw_FriendshipStats` AS
SELECT
    UserId,
    COUNT(*) as TotalFriends,
    SUM(CASE WHEN IsFavorite = 1 THEN 1 ELSE 0 END) as FavoriteFriends,
    SUM(InteractionScore) as TotalInteractionScore,
    MAX(LastInteractedAt) as LastInteractionAt
FROM Friendships
WHERE Status = 'active' AND IsDeleted = 0
GROUP BY UserId;

USE `writing_platform_search`;

-- View for search statistics
CREATE OR REPLACE VIEW `vw_SearchStats` AS
SELECT
    DATE(CreatedAt) as SearchDate,
    COUNT(*) as TotalSearches,
    AVG(ResultCount) as AvgResults,
    AVG(TIME_TO_SEC(ResponseTime)) as AvgResponseTimeSeconds,
    SUM(CASE WHEN IsSuccessful = 1 THEN 1 ELSE 0 END) as SuccessfulSearches
FROM SearchHistories
GROUP BY DATE(CreatedAt);

-- ============================================
-- 5. User Service Database: writing_platform_user
-- ============================================
CREATE DATABASE IF NOT EXISTS `writing_platform_user`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE `writing_platform_user`;

-- Users table
CREATE TABLE IF NOT EXISTS `Users` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `Username` VARCHAR(50) NOT NULL,
    `Email` VARCHAR(100) NOT NULL,
    `PasswordHash` VARCHAR(100) NOT NULL,
    `Phone` VARCHAR(50) NULL,
    `Bio` VARCHAR(500) NULL,
    `AvatarUrl` VARCHAR(200) NULL,
    `Location` VARCHAR(100) NULL,
    `Website` VARCHAR(200) NULL,
    `Role` VARCHAR(20) NOT NULL DEFAULT 'User',
    `Status` VARCHAR(20) NOT NULL DEFAULT 'Active',
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `LastLoginAt` DATETIME NULL,
    `IsDeleted` BIT NOT NULL DEFAULT 0,

    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_Users_Username` (`Username`),
    UNIQUE INDEX `IX_Users_Email` (`Email`),
    INDEX `IX_Users_Role` (`Role`),
    INDEX `IX_Users_Status` (`Status`),
    INDEX `IX_Users_CreatedAt` (`CreatedAt`),
    INDEX `IX_Users_UpdatedAt` (`UpdatedAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- UserProfiles table
CREATE TABLE IF NOT EXISTS `UserProfiles` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `UserId` BIGINT NOT NULL,
    `Biography` VARCHAR(1000) NULL,
    `WritingCount` INT NOT NULL DEFAULT 0,
    `LikeCount` INT NOT NULL DEFAULT 0,
    `FollowersCount` INT NOT NULL DEFAULT 0,
    `FollowingCount` INT NOT NULL DEFAULT 0,
    `TotalWords` INT NOT NULL DEFAULT 0,
    `BirthDate` DATETIME NULL,
    `Gender` VARCHAR(50) NULL,
    `TwitterUrl` VARCHAR(100) NULL,
    `GitHubUrl` VARCHAR(100) NULL,
    `LinkedInUrl` VARCHAR(100) NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_UserProfiles_UserId` (`UserId`),
    INDEX `IX_UserProfiles_CreatedAt` (`CreatedAt`),
    INDEX `IX_UserProfiles_UpdatedAt` (`UpdatedAt`),
    CONSTRAINT `FK_UserProfiles_Users_UserId` FOREIGN KEY (`UserId`)
        REFERENCES `Users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- UserFollows table (user following relationships)
CREATE TABLE IF NOT EXISTS `UserFollows` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `FollowerId` BIGINT NOT NULL,
    `FollowingId` BIGINT NOT NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_UserFollows_FollowerId_FollowingId` (`FollowerId`, `FollowingId`),
    INDEX `IX_UserFollows_FollowerId` (`FollowerId`),
    INDEX `IX_UserFollows_FollowingId` (`FollowingId`),
    INDEX `IX_UserFollows_CreatedAt` (`CreatedAt`),
    CONSTRAINT `FK_UserFollows_Users_FollowerId` FOREIGN KEY (`FollowerId`)
        REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_UserFollows_Users_FollowingId` FOREIGN KEY (`FollowingId`)
        REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- RefreshTokens table
CREATE TABLE IF NOT EXISTS `RefreshTokens` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `UserId` BIGINT NOT NULL,
    `Token` VARCHAR(500) NOT NULL,
    `ExpiresAt` DATETIME NOT NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `IsRevoked` BIT NOT NULL DEFAULT 0,
    `RevokedAt` DATETIME NULL,
    `RevokedByIp` VARCHAR(50) NULL,
    `ReplacedByToken` VARCHAR(200) NULL,

    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_RefreshTokens_Token` (`Token`),
    INDEX `IX_RefreshTokens_UserId` (`UserId`),
    INDEX `IX_RefreshTokens_ExpiresAt` (`ExpiresAt`),
    INDEX `IX_RefreshTokens_CreatedAt` (`CreatedAt`),
    CONSTRAINT `FK_RefreshTokens_Users_UserId` FOREIGN KEY (`UserId`)
        REFERENCES `Users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- UserActivityLogs table
CREATE TABLE IF NOT EXISTS `UserActivityLogs` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `UserId` BIGINT NOT NULL,
    `ActivityType` VARCHAR(50) NOT NULL,
    `Description` VARCHAR(200) NOT NULL,
    `Details` VARCHAR(500) NULL,
    `IpAddress` VARCHAR(45) NULL,
    `UserAgent` VARCHAR(500) NULL,
    `ResourceType` VARCHAR(50) NULL,
    `ResourceId` BIGINT NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (`Id`),
    INDEX `IX_UserActivityLogs_UserId` (`UserId`),
    INDEX `IX_UserActivityLogs_ActivityType` (`ActivityType`),
    INDEX `IX_UserActivityLogs_CreatedAt` (`CreatedAt`),
    INDEX `IX_UserActivityLogs_ResourceType_ResourceId` (`ResourceType`, `ResourceId`),
    CONSTRAINT `FK_UserActivityLogs_Users_UserId` FOREIGN KEY (`UserId`)
        REFERENCES `Users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert sample admin user (password: Admin@123)
INSERT INTO `Users` (`Id`, `Username`, `Email`, `PasswordHash`, `Role`, `Status`, `CreatedAt`, `UpdatedAt`) VALUES
(1, 'admin', 'admin@writingplatform.com', 'AQAAAAIAAYagAAAAENpBpO6J7ROv7LzmGQ7wKHfT4Nl1V8hGjH0yXpYpMk0=', 'Admin', 'Active', NOW(), NOW());

-- Insert sample user profile for admin
INSERT INTO `UserProfiles` (`UserId`, `Biography`, `CreatedAt`, `UpdatedAt`) VALUES
(1, '系统管理员账户', NOW(), NOW());

-- ============================================
-- 6. Writing Service Database: writing_platform_writing
-- ============================================
CREATE DATABASE IF NOT EXISTS `writing_platform_writing`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE `writing_platform_writing`;

-- Categories table
CREATE TABLE IF NOT EXISTS `Categories` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `Name` VARCHAR(100) NOT NULL,
    `Description` VARCHAR(500) NULL,
    `ParentCategoryId` BIGINT NULL,
    `Slug` VARCHAR(200) NULL,
    `IsActive` BIT NOT NULL DEFAULT 1,
    `SortOrder` INT NOT NULL DEFAULT 0,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (`Id`),
    INDEX `IX_Categories_ParentCategoryId` (`ParentCategoryId`),
    UNIQUE INDEX `IX_Categories_Slug` (`Slug`),
    INDEX `IX_Categories_IsActive` (`IsActive`),
    INDEX `IX_Categories_SortOrder` (`SortOrder`),
    CONSTRAINT `FK_Categories_Categories_ParentCategoryId` FOREIGN KEY (`ParentCategoryId`)
        REFERENCES `Categories` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Works table
CREATE TABLE IF NOT EXISTS `Works` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `Title` VARCHAR(200) NOT NULL,
    `Content` TEXT NOT NULL,
    `Excerpt` VARCHAR(500) NULL,
    `AuthorId` BIGINT NOT NULL,
    `CategoryId` BIGINT NULL,
    `TagsJson` VARCHAR(1000) NULL,
    `WordCount` INT NOT NULL DEFAULT 0,
    `Status` VARCHAR(20) NOT NULL DEFAULT 'Draft',
    `IsPublished` BIT NOT NULL DEFAULT 0,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `PublishedAt` DATETIME NULL,
    `Views` INT NOT NULL DEFAULT 0,
    `Likes` INT NOT NULL DEFAULT 0,
    `Slug` VARCHAR(200) NULL,
    `IsDeleted` BIT NOT NULL DEFAULT 0,

    PRIMARY KEY (`Id`),
    INDEX `IX_Works_AuthorId` (`AuthorId`),
    INDEX `IX_Works_CategoryId` (`CategoryId`),
    INDEX `IX_Works_Status` (`Status`),
    INDEX `IX_Works_IsPublished` (`IsPublished`),
    UNIQUE INDEX `IX_Works_Slug` (`Slug`),
    INDEX `IX_Works_CreatedAt` (`CreatedAt`),
    INDEX `IX_Works_PublishedAt` (`PublishedAt`),
    CONSTRAINT `FK_Works_Categories_CategoryId` FOREIGN KEY (`CategoryId`)
        REFERENCES `Categories` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;



-- Templates table
CREATE TABLE IF NOT EXISTS `Templates` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `Title` VARCHAR(200) NOT NULL,
    `Content` TEXT NOT NULL,
    `Description` VARCHAR(500) NULL,
    `AuthorId` BIGINT NOT NULL,
    `CategoryId` BIGINT NULL,
    `TagsJson` VARCHAR(1000) NULL,
    `Type` VARCHAR(20) NOT NULL DEFAULT 'Custom',
    `IsPublic` BIT NOT NULL DEFAULT 0,
    `UsageCount` INT NOT NULL DEFAULT 0,
    `Likes` INT NOT NULL DEFAULT 0,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (`Id`),
    INDEX `IX_Templates_AuthorId` (`AuthorId`),
    INDEX `IX_Templates_CategoryId` (`CategoryId`),
    INDEX `IX_Templates_Type` (`Type`),
    INDEX `IX_Templates_IsPublic` (`IsPublic`),
    INDEX `IX_Templates_UsageCount` (`UsageCount`),
    CONSTRAINT `FK_Templates_Categories_CategoryId` FOREIGN KEY (`CategoryId`)
        REFERENCES `Categories` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- WorkVersions table
CREATE TABLE IF NOT EXISTS `WorkVersions` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `WorkId` BIGINT NOT NULL,
    `VersionNumber` INT NOT NULL,
    `Content` TEXT NOT NULL,
    `Title` VARCHAR(200) NULL,
    `Excerpt` VARCHAR(500) NULL,
    `ChangeDescription` VARCHAR(1000) NULL,
    `WordCount` INT NOT NULL DEFAULT 0,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `CreatedBy` BIGINT NOT NULL,

    PRIMARY KEY (`Id`),
    INDEX `IX_WorkVersions_WorkId` (`WorkId`),
    INDEX `IX_WorkVersions_VersionNumber` (`VersionNumber`),
    INDEX `IX_WorkVersions_CreatedAt` (`CreatedAt`),
    UNIQUE INDEX `IX_WorkVersions_WorkId_VersionNumber` (`WorkId`, `VersionNumber`),
    CONSTRAINT `FK_WorkVersions_Works_WorkId` FOREIGN KEY (`WorkId`)
        REFERENCES `Works` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- WorkCollaborators table
CREATE TABLE IF NOT EXISTS `WorkCollaborators` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `WorkId` BIGINT NOT NULL,
    `UserId` BIGINT NOT NULL,
    `Role` VARCHAR(50) NOT NULL DEFAULT 'Collaborator',
    `InvitedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `JoinedAt` DATETIME NULL,
    `Status` VARCHAR(20) NOT NULL DEFAULT 'Pending',

    PRIMARY KEY (`Id`),
    INDEX `IX_WorkCollaborators_WorkId` (`WorkId`),
    INDEX `IX_WorkCollaborators_UserId` (`UserId`),
    INDEX `IX_WorkCollaborators_Status` (`Status`),
    UNIQUE INDEX `IX_WorkCollaborators_WorkId_UserId` (`WorkId`, `UserId`),
    CONSTRAINT `FK_WorkCollaborators_Works_WorkId` FOREIGN KEY (`WorkId`)
        REFERENCES `Works` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert sample categories
INSERT INTO `Categories` (`Name`, `Description`, `Slug`, `SortOrder`) VALUES
('小说', '各类小说作品', 'novel', 1),
('诗歌', '诗歌创作', 'poetry', 2),
('散文', '散文随笔', 'essay', 3),
('教程', '写作教程和技巧', 'tutorial', 4);

-- Insert sample work
INSERT INTO `Works` (`Title`, `Content`, `AuthorId`, `CategoryId`, `Status`, `IsPublished`, `WordCount`, `Slug`) VALUES
('我的第一篇作品', '这是作品内容...', 1, 1, 'Published', 1, 1500, 'my-first-work');

-- Insert sample template
INSERT INTO `Templates` (`Title`, `Content`, `AuthorId`, `Type`, `IsPublic`, `Description`) VALUES
('小说开头模板', '# 标题\n\n## 第一章\n\n开始你的故事...', 1, 'System', 1, '用于小说开头的模板');

-- ============================================
-- 7. AI Service Database: writing_platform_ai
-- ============================================
CREATE DATABASE IF NOT EXISTS `writing_platform_ai`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE `writing_platform_ai`;

-- AIAuditLogs table
CREATE TABLE IF NOT EXISTS `AIAuditLogs` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `UserId` CHAR(36) NOT NULL,
    `ServiceType` VARCHAR(50) NOT NULL,
    `RequestType` VARCHAR(100) NOT NULL,
    `RequestData` JSON NULL,
    `ResponseData` JSON NULL,
    `StatusCode` INT NOT NULL,
    `ErrorMessage` VARCHAR(500) NULL,
    `ProcessingTimeMs` BIGINT NOT NULL,
    `TokenCount` INT NOT NULL DEFAULT 0,
    `Cost` DECIMAL(10, 4) NOT NULL DEFAULT 0.0000,
    `ModelUsed` VARCHAR(50) NULL,
    `Provider` VARCHAR(100) NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `RequestDate` DATE NULL,
    `ClientIp` VARCHAR(100) NULL,
    `UserAgent` VARCHAR(500) NULL,
    `CorrelationId` VARCHAR(100) NULL,
    `SessionId` VARCHAR(100) NULL,

    PRIMARY KEY (`Id`),
    INDEX `IX_AIAuditLogs_UserId` (`UserId`),
    INDEX `IX_AIAuditLogs_ServiceType` (`ServiceType`),
    INDEX `IX_AIAuditLogs_RequestType` (`RequestType`),
    INDEX `IX_AIAuditLogs_CreatedAt` (`CreatedAt`),
    INDEX `IX_AIAuditLogs_UserId_CreatedAt` (`UserId`, `CreatedAt`),
    INDEX `IX_AIAuditLogs_ServiceType_RequestType_CreatedAt` (`ServiceType`, `RequestType`, `CreatedAt`),
    INDEX `IX_AIAuditLogs_RequestDate` (`RequestDate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert sample AI audit log
INSERT INTO `AIAuditLogs` (`UserId`, `ServiceType`, `RequestType`, `StatusCode`, `ProcessingTimeMs`, `CreatedAt`) VALUES
(UUID(), 'Assistant', 'GetWritingSuggestion', 200, 1505, NOW());

-- ============================================
-- 8. Chat Service Database: writing_platform_chat
-- ============================================
CREATE DATABASE IF NOT EXISTS `writing_platform_chat`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE `writing_platform_chat`;

-- ChatRooms table
CREATE TABLE IF NOT EXISTS `ChatRooms` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `Name` VARCHAR(100) NOT NULL,
    `Description` VARCHAR(500) NULL,
    `RoomType` VARCHAR(20) NOT NULL DEFAULT 'Private',
    `CreatorId` BIGINT NOT NULL,
    `AvatarUrl` VARCHAR(500) NULL,
    `MaxMembers` INT NOT NULL DEFAULT 0,
    `IsEncrypted` BIT NOT NULL DEFAULT 0,
    `IsPublic` BIT NOT NULL DEFAULT 0,
    `InviteEnabled` BIT NOT NULL DEFAULT 1,
    `InviteCode` VARCHAR(100) NULL,
    `InviteExpiresAt` DATETIME NULL,
    `LastActivityAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `Metadata` JSON NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `IsDeleted` BIT NOT NULL DEFAULT 0,

    PRIMARY KEY (`Id`),
    INDEX `IX_ChatRooms_CreatorId` (`CreatorId`),
    INDEX `IX_ChatRooms_RoomType` (`RoomType`),
    INDEX `IX_ChatRooms_IsPublic` (`IsPublic`),
    INDEX `IX_ChatRooms_InviteCode` (`InviteCode`),
    INDEX `IX_ChatRooms_LastActivityAt` (`LastActivityAt`),
    INDEX `IX_ChatRooms_CreatedAt` (`CreatedAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Messages table
CREATE TABLE IF NOT EXISTS `Messages` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `Content` TEXT NOT NULL,
    `MessageType` VARCHAR(20) NOT NULL DEFAULT 'Text',
    `SenderId` BIGINT NOT NULL,
    `ReceiverId` BIGINT NULL,
    `ChatRoomId` BIGINT NULL,
    `ParentMessageId` BIGINT NULL,
    `Status` VARCHAR(20) NOT NULL DEFAULT 'Sent',
    `SentAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `DeliveredAt` DATETIME NULL,
    `ReadAt` DATETIME NULL,
    `FileUrl` VARCHAR(500) NULL,
    `FileSize` BIGINT NULL,
    `FileType` VARCHAR(100) NULL,
    `Metadata` JSON NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `IsDeleted` BIT NOT NULL DEFAULT 0,

    PRIMARY KEY (`Id`),
    INDEX `IX_Messages_SenderId` (`SenderId`),
    INDEX `IX_Messages_ReceiverId` (`ReceiverId`),
    INDEX `IX_Messages_ChatRoomId` (`ChatRoomId`),
    INDEX `IX_Messages_ParentMessageId` (`ParentMessageId`),
    INDEX `IX_Messages_Status` (`Status`),
    INDEX `IX_Messages_SentAt` (`SentAt`),
    INDEX `IX_Messages_CreatedAt` (`CreatedAt`),
    CONSTRAINT `FK_Messages_Users_SenderId` FOREIGN KEY (`SenderId`)
        REFERENCES `writing_platform_user`.`Users` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Messages_Users_ReceiverId` FOREIGN KEY (`ReceiverId`)
        REFERENCES `writing_platform_user`.`Users` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_Messages_ChatRooms_ChatRoomId` FOREIGN KEY (`ChatRoomId`)
        REFERENCES `ChatRooms` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Messages_Messages_ParentMessageId` FOREIGN KEY (`ParentMessageId`)
        REFERENCES `Messages` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ChatRoomMembers table
CREATE TABLE IF NOT EXISTS `ChatRoomMembers` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `UserId` BIGINT NOT NULL,
    `ChatRoomId` BIGINT NOT NULL,
    `Role` VARCHAR(20) NOT NULL DEFAULT 'Member',
    `Nickname` VARCHAR(50) NULL,
    `JoinedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `LastReadMessageId` BIGINT NULL,
    `LastReadAt` DATETIME NULL,
    `IsMuted` BIT NOT NULL DEFAULT 0,
    `MutedUntil` DATETIME NULL,
    `IsBlocked` BIT NOT NULL DEFAULT 0,
    `Metadata` JSON NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_ChatRoomMembers_UserId_ChatRoomId` (`UserId`, `ChatRoomId`),
    INDEX `IX_ChatRoomMembers_UserId` (`UserId`),
    INDEX `IX_ChatRoomMembers_ChatRoomId` (`ChatRoomId`),
    INDEX `IX_ChatRoomMembers_Role` (`Role`),
    INDEX `IX_ChatRoomMembers_JoinedAt` (`JoinedAt`),
    INDEX `IX_ChatRoomMembers_LastReadAt` (`LastReadAt`),
    CONSTRAINT `FK_ChatRoomMembers_Users_UserId` FOREIGN KEY (`UserId`)
        REFERENCES `writing_platform_user`.`Users` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_ChatRoomMembers_ChatRooms_ChatRoomId` FOREIGN KEY (`ChatRoomId`)
        REFERENCES `ChatRooms` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_ChatRoomMembers_Messages_LastReadMessageId` FOREIGN KEY (`LastReadMessageId`)
        REFERENCES `Messages` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;



-- UserMessageReads table (tracking read status per user per message)
CREATE TABLE IF NOT EXISTS `UserMessageReads` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `UserId` BIGINT NOT NULL,
    `MessageId` BIGINT NOT NULL,
    `ReadAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `DeviceId` VARCHAR(100) NULL,
    `IpAddress` VARCHAR(50) NULL,
    `UserAgent` VARCHAR(500) NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_UserMessageReads_UserId_MessageId` (`UserId`, `MessageId`),
    INDEX `IX_UserMessageReads_UserId` (`UserId`),
    INDEX `IX_UserMessageReads_MessageId` (`MessageId`),
    INDEX `IX_UserMessageReads_ReadAt` (`ReadAt`),
    CONSTRAINT `FK_UserMessageReads_Users_UserId` FOREIGN KEY (`UserId`)
        REFERENCES `writing_platform_user`.`Users` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_UserMessageReads_Messages_MessageId` FOREIGN KEY (`MessageId`)
        REFERENCES `Messages` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert sample chat room
INSERT INTO `ChatRooms` (`Name`, `Description`, `RoomType`, `CreatorId`, `IsPublic`) VALUES
('写作交流群', '写作爱好者的交流群', 'Group', 1, 1);

-- Insert sample chat room member
INSERT INTO `ChatRoomMembers` (`UserId`, `ChatRoomId`, `Role`, `JoinedAt`) VALUES
(1, 1, 'Owner', NOW());

-- Insert sample message
INSERT INTO `Messages` (`Content`, `MessageType`, `SenderId`, `ChatRoomId`, `Status`, `SentAt`) VALUES
('大家好，欢迎加入写作交流群！', 'Text', 1, 1, 'Sent', NOW());

-- ============================================
-- 9. Community Service Database: writing_platform_community
-- ============================================
CREATE DATABASE IF NOT EXISTS `writing_platform_community`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE `writing_platform_community`;

-- Posts table
CREATE TABLE IF NOT EXISTS `Posts` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `Title` VARCHAR(200) NOT NULL,
    `Content` TEXT NOT NULL,
    `Summary` TEXT NULL,
    `AuthorId` BIGINT NOT NULL,
    `WorkId` BIGINT NULL,
    `CoverImageUrl` VARCHAR(500) NULL,
    `Tags` VARCHAR(1000) NULL,
    `Category` VARCHAR(50) NOT NULL DEFAULT 'Other',
    `Status` VARCHAR(50) NOT NULL DEFAULT 'Pending',
    `Visibility` VARCHAR(50) NOT NULL DEFAULT 'Public',
    `LikeCount` INT NOT NULL DEFAULT 0,
    `CommentCount` INT NOT NULL DEFAULT 0,
    `CollectionCount` INT NOT NULL DEFAULT 0,
    `ViewCount` INT NOT NULL DEFAULT 0,
    `HotScore` DECIMAL(18,6) NOT NULL DEFAULT 0.000000,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `PublishedAt` DATETIME NULL,
    `IsDeleted` BIT NOT NULL DEFAULT 0,

    PRIMARY KEY (`Id`),
    INDEX `IX_Posts_AuthorId` (`AuthorId`),
    INDEX `IX_Posts_WorkId` (`WorkId`),
    INDEX `IX_Posts_Status` (`Status`),
    INDEX `IX_Posts_Visibility` (`Visibility`),
    INDEX `IX_Posts_Category` (`Category`),
    INDEX `IX_Posts_CreatedAt` (`CreatedAt`),
    INDEX `IX_Posts_HotScore` (`HotScore` DESC),
    CONSTRAINT `FK_Posts_Users_AuthorId` FOREIGN KEY (`AuthorId`)
        REFERENCES `writing_platform_user`.`Users` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Posts_Works_WorkId` FOREIGN KEY (`WorkId`)
        REFERENCES `writing_platform_writing`.`Works` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Comments table
CREATE TABLE IF NOT EXISTS `Comments` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `Content` TEXT NOT NULL,
    `PostId` BIGINT NOT NULL,
    `AuthorId` BIGINT NOT NULL,
    `ParentId` BIGINT NULL,
    `LikeCount` INT NOT NULL DEFAULT 0,
    `Status` VARCHAR(50) NOT NULL DEFAULT 'Active',
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `IsDeleted` BIT NOT NULL DEFAULT 0,

    PRIMARY KEY (`Id`),
    INDEX `IX_Comments_PostId` (`PostId`),
    INDEX `IX_Comments_AuthorId` (`AuthorId`),
    INDEX `IX_Comments_ParentId` (`ParentId`),
    INDEX `IX_Comments_Status` (`Status`),
    INDEX `IX_Comments_CreatedAt` (`CreatedAt`),
    CONSTRAINT `FK_Comments_Posts_PostId` FOREIGN KEY (`PostId`)
        REFERENCES `Posts` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Comments_Users_AuthorId` FOREIGN KEY (`AuthorId`)
        REFERENCES `writing_platform_user`.`Users` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Comments_Comments_ParentId` FOREIGN KEY (`ParentId`)
        REFERENCES `Comments` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Likes table
CREATE TABLE IF NOT EXISTS `Likes` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `UserId` BIGINT NOT NULL,
    `TargetType` VARCHAR(50) NOT NULL,
    `TargetId` BIGINT NOT NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_Likes_UserId_TargetType_TargetId` (`UserId`, `TargetType`, `TargetId`),
    INDEX `IX_Likes_TargetType_TargetId` (`TargetType`, `TargetId`),
    INDEX `IX_Likes_CreatedAt` (`CreatedAt`),
    CONSTRAINT `FK_Likes_Users_UserId` FOREIGN KEY (`UserId`)
        REFERENCES `writing_platform_user`.`Users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Collections table
CREATE TABLE IF NOT EXISTS `Collections` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `UserId` BIGINT NOT NULL,
    `PostId` BIGINT NOT NULL,
    `Note` VARCHAR(200) NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `IsDeleted` BIT NOT NULL DEFAULT 0,

    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_Collections_UserId_PostId` (`UserId`, `PostId`),
    INDEX `IX_Collections_UserId` (`UserId`),
    INDEX `IX_Collections_PostId` (`PostId`),
    INDEX `IX_Collections_CreatedAt` (`CreatedAt`),
    CONSTRAINT `FK_Collections_Users_UserId` FOREIGN KEY (`UserId`)
        REFERENCES `writing_platform_user`.`Users` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Collections_Posts_PostId` FOREIGN KEY (`PostId`)
        REFERENCES `Posts` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert sample data for CommunityService
INSERT INTO `Posts` (`Title`, `Content`, `Summary`, `AuthorId`, `Tags`, `Category`, `Status`, `Visibility`, `LikeCount`, `CommentCount`, `CollectionCount`, `ViewCount`, `HotScore`, `PublishedAt`) VALUES
('欢迎来到写作平台社区', '这是第一篇社区帖子，欢迎大家分享自己的作品和想法！', '社区欢迎帖', 1, '欢迎,社区,公告', 'Other', 'Published', 'Public', 10, 5, 3, 100, 50.5, NOW());

INSERT INTO `Comments` (`Content`, `PostId`, `AuthorId`, `LikeCount`, `Status`) VALUES
('很好的社区！', 1, 1, 2, 'Active');

INSERT INTO `Likes` (`UserId`, `TargetType`, `TargetId`, `CreatedAt`) VALUES
(1, 'Post', 1, NOW());

INSERT INTO `Collections` (`UserId`, `PostId`, `Note`, `CreatedAt`) VALUES
(1, 1, '收藏这篇欢迎帖', NOW());

-- ============================================
-- 10. Payment Service Database: writing_platform_payment
-- ============================================
CREATE DATABASE IF NOT EXISTS `writing_platform_payment`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE `writing_platform_payment`;

-- PaymentTransactions table
CREATE TABLE IF NOT EXISTS `PaymentTransactions` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `TransactionNo` VARCHAR(50) NOT NULL,
    `GatewayTransactionNo` VARCHAR(100) NULL,
    `UserId` CHAR(36) NOT NULL,
    `UserEmail` VARCHAR(100) NULL,
    `PaymentGateway` VARCHAR(50) NOT NULL,
    `PaymentType` VARCHAR(50) NOT NULL,
    `Currency` VARCHAR(3) NOT NULL DEFAULT 'CNY',
    `Amount` DECIMAL(18,2) NOT NULL,
    `Fee` DECIMAL(18,2) NULL,
    `RefundedAmount` DECIMAL(18,2) NULL,
    `Status` VARCHAR(50) NOT NULL DEFAULT 'Pending',
    `Description` VARCHAR(500) NULL,
    `ProductId` VARCHAR(100) NULL,
    `ProductName` VARCHAR(200) NULL,
    `ProductDescription` VARCHAR(500) NULL,
    `TargetUserId` CHAR(36) NULL,
    `TargetUserName` VARCHAR(200) NULL,
    `Message` VARCHAR(500) NULL,
    `Anonymous` VARCHAR(50) NULL DEFAULT 'false',
    `GatewayRequestData` JSON NULL,
    `GatewayResponseData` JSON NULL,
    `GatewayCallbackData` JSON NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL,
    `PaidAt` DATETIME NULL,
    `ExpiredAt` DATETIME NULL,
    `ClientIp` VARCHAR(50) NULL,
    `UserAgent` VARCHAR(500) NULL,
    `DeviceInfo` VARCHAR(100) NULL,
    `Channel` VARCHAR(100) NULL,
    `ReturnUrl` VARCHAR(100) NULL,
    `NotifyUrl` VARCHAR(100) NULL,
    `Attach` VARCHAR(100) NULL,
    `CreatedBy` VARCHAR(100) NULL,
    `UpdatedBy` VARCHAR(100) NULL,
    `ErrorMessage` VARCHAR(500) NULL,
    `ErrorCode` VARCHAR(50) NULL,

    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_PaymentTransactions_TransactionNo` (`TransactionNo`),
    INDEX `IX_PaymentTransactions_GatewayTransactionNo` (`GatewayTransactionNo`),
    INDEX `IX_PaymentTransactions_UserId` (`UserId`),
    INDEX `IX_PaymentTransactions_Status` (`Status`),
    INDEX `IX_PaymentTransactions_CreatedAt` (`CreatedAt`),
    INDEX `IX_PaymentTransactions_UserId_CreatedAt` (`UserId`, `CreatedAt`),
    INDEX `IX_PaymentTransactions_PaymentGateway_Status_CreatedAt` (`PaymentGateway`, `Status`, `CreatedAt`),
    INDEX `IX_PaymentTransactions_PaymentType_Status_CreatedAt` (`PaymentType`, `Status`, `CreatedAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- RefundTransactions table
CREATE TABLE IF NOT EXISTS `RefundTransactions` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `RefundNo` VARCHAR(50) NOT NULL,
    `GatewayRefundNo` VARCHAR(100) NULL,
    `PaymentTransactionId` BIGINT NOT NULL,
    `PaymentGateway` VARCHAR(50) NOT NULL,
    `Currency` VARCHAR(3) NOT NULL DEFAULT 'CNY',
    `Amount` DECIMAL(18,2) NOT NULL,
    `Fee` DECIMAL(18,2) NULL,
    `Status` VARCHAR(50) NOT NULL DEFAULT 'Pending',
    `Source` VARCHAR(50) NOT NULL DEFAULT 'UserRequest',
    `UserId` CHAR(36) NOT NULL,
    `UserEmail` VARCHAR(100) NULL,
    `Reason` VARCHAR(500) NULL,
    `Description` VARCHAR(1000) NULL,
    `ReviewerId` VARCHAR(100) NULL,
    `ReviewerName` VARCHAR(100) NULL,
    `ReviewedAt` DATETIME NULL,
    `ReviewComment` VARCHAR(500) NULL,
    `GatewayRequestData` JSON NULL,
    `GatewayResponseData` JSON NULL,
    `GatewayCallbackData` JSON NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL,
    `RefundedAt` DATETIME NULL,
    `CreatedBy` VARCHAR(100) NULL,
    `UpdatedBy` VARCHAR(100) NULL,
    `ErrorMessage` VARCHAR(500) NULL,
    `ErrorCode` VARCHAR(50) NULL,

    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_RefundTransactions_RefundNo` (`RefundNo`),
    INDEX `IX_RefundTransactions_GatewayRefundNo` (`GatewayRefundNo`),
    INDEX `IX_RefundTransactions_PaymentTransactionId` (`PaymentTransactionId`),
    INDEX `IX_RefundTransactions_UserId` (`UserId`),
    INDEX `IX_RefundTransactions_Status` (`Status`),
    INDEX `IX_RefundTransactions_CreatedAt` (`CreatedAt`),
    INDEX `IX_RefundTransactions_PaymentGateway_Status_CreatedAt` (`PaymentGateway`, `Status`, `CreatedAt`),
    CONSTRAINT `FK_RefundTransactions_PaymentTransactions_PaymentTransactionId` FOREIGN KEY (`PaymentTransactionId`)
        REFERENCES `PaymentTransactions` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert sample data for PaymentService
INSERT INTO `PaymentTransactions` (`TransactionNo`, `UserId`, `PaymentGateway`, `PaymentType`, `Amount`, `Status`, `Description`, `CreatedAt`) VALUES
('PAY202501010001', UUID(), 'Alipay', 'Donation', 50.00, 'Success', '测试打赏交易', NOW());

INSERT INTO `RefundTransactions` (`RefundNo`, `PaymentTransactionId`, `PaymentGateway`, `Amount`, `Status`, `Source`, `UserId`, `Reason`, `CreatedAt`) VALUES
('REF202501010001', 1, 'Alipay', 50.00, 'Success', 'UserRequest', UUID(), '测试退款', NOW());

-- ============================================
-- Migration Notes
-- ============================================
--
-- 1. To apply this schema:
--    mysql -u root -p < database_schema_mysql.sql
--
-- 2. Update connection strings in appsettings.json:
--    FileService: Server=localhost;Port=3306;Database=writing_platform_file;Uid=writing_file_user;Pwd=FileServicePassword123!
--    SearchService: Server=localhost;Port=3306;Database=writing_platform_search;Uid=writing_search_user;Pwd=SearchServicePassword123!
--    FriendshipService: Server=localhost;Port=3306;Database=writing_platform_friendship;Uid=writing_friendship_user;Pwd=FriendshipServicePassword123!
--    NotificationService: Server=localhost;Port=3306;Database=writing_platform_notification;Uid=writing_notification_user;Pwd=NotificationServicePassword123!
--    UserService: Server=localhost;Port=3306;Database=writing_platform_user;Uid=writing_user_user;Pwd=UserServicePassword123!
--    WritingService: Server=localhost;Port=3306;Database=writing_platform_writing;Uid=writing_writing_user;Pwd=WritingServicePassword123!
--    AIService: Server=localhost;Port=3306;Database=writing_platform_ai;Uid=writing_ai_user;Pwd=AIServicePassword123!
--    ChatService: Server=localhost;Port=3306;Database=writing_platform_chat;Uid=writing_chat_user;Pwd=ChatServicePassword123!
--    CommunityService: Server=localhost;Port=3306;Database=writing_platform_community;Uid=writing_community_user;Pwd=CommunityServicePassword123!
--    PaymentService: Server=localhost;Port=3306;Database=writing_platform_payment;Uid=writing_payment_user;Pwd=PaymentServicePassword123!
--
-- 3. For production:
--    - Change all passwords
--    - Restrict database user permissions
--    - Enable SSL/TLS for connections
--    - Set up proper backup strategy
--    - Configure connection pooling in applications
--