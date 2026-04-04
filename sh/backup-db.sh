#!/bin/bash
BASE="/www/wwwroot/PaperServer"
BACKUP_DIR="$BASE/backups"
DATE=$(date +%Y-%m-%d_%H-%M-%S)

# 创建备份目录
mkdir -p $BACKUP_DIR

# 数据库信息（改成你自己的）
DB_USER="root"
DB_PASS="你的数据库密码"
DB_NAME="PaperSystem"

echo "============================================="
echo "         数据库自动备份
echo "============================================="

# 备份
mysqldump -u$DB_USER -p$DB_PASS $DB_NAME > "$BACKUP_DIR/db_$DATE.sql"
gzip "$BACKUP_DIR/db_$DATE.sql"

echo "✅ 备份完成：db_$DATE.sql.gz"

# 删除7天前的备份
find $BACKUP_DIR -name "db_*.sql.gz" -mtime +7 -delete
echo "✅ 已清理7天前的旧备份"
echo ""