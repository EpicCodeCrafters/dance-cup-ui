# ClickHouse Log Archiving to S3 (MinIO)

## Overview
This setup implements automatic log archiving from ClickHouse to S3 (MinIO) storage using tiered storage with TTL. Logs are stored locally for fast access during the first 5 minutes, then automatically moved to S3 for archival. After 15 minutes total, logs are deleted.

## Architecture

### Components
1. **Main Logs Table** (`logs`): 
   - Stores incoming logs from Kafka
   - Uses tiered storage policy (`logs_policy`)
   - Local storage (hot): 0-5 minutes
   - S3 storage (cold): 5-15 minutes
   - TTL: Deleted after 15 minutes total

### Storage Tiers
- **Hot Volume** (default disk): Logs from 0-5 minutes old
- **Cold Volume** (s3_disk): Logs from 5-15 minutes old

### Data Flow
```
Kafka (dance_cup_logs) 
  → kafka_messages (Kafka Engine) 
    → kafka_messages_to_logs (Materialized View) 
      → logs (MergeTree with tiered storage)
        ├─ [0-5 min]: Local disk (hot)
        ├─ [5-15 min]: S3 (cold) ← archived
        └─ [>15 min]: Deleted
```

## Configuration

### S3 Storage Configuration
File: `clickhouse/s3_config.xml`
- Endpoint: `http://object-storage:9000/logs-archive/`
- Access Key: `admin`
- Secret Key: `admin123`
- Storage Policy: `logs_policy` with two volumes:
  - `hot`: Local default disk
  - `cold`: S3 disk

### MinIO Bucket
Bucket `logs-archive` is automatically created by `object-storage-init` service in docker-compose.yml

## How It Works

1. Logs are received from Kafka topic `dance_cup_logs`
2. Materialized view `kafka_messages_to_logs` parses and inserts logs into the `logs` table
3. For the first 5 minutes, logs remain on local fast storage (hot volume)
4. After 5 minutes, ClickHouse automatically moves logs to S3 storage (cold volume) - **ARCHIVING**
5. After 15 minutes total, logs are automatically deleted by TTL
6. During the 5-15 minute window, archived logs remain accessible in S3

## Verification

To verify the setup is working:

1. Check that logs are being written:
```sql
SELECT count() FROM logs;
```

2. Check which volume logs are on:
```sql
SELECT 
    disk_name,
    count() as count,
    formatReadableSize(sum(bytes_on_disk)) as size
FROM system.parts
WHERE table = 'logs' AND active
GROUP BY disk_name;
```

3. Access MinIO console at http://localhost:9001 (admin/admin123) and verify the `logs-archive` bucket contains data after 5 minutes.

## Benefits

- **Performance**: Recent logs (0-5 minutes) are on fast local storage for quick queries
- **Cost Efficiency**: Older logs (5-15 minutes) are automatically moved to cheaper S3 storage
- **Data Retention**: Logs are retained for 15 minutes total with automatic archiving
- **Transparent**: Applications query the same `logs` table, ClickHouse handles the storage tiers automatically
- **Automatic**: No manual intervention required for archiving or cleanup
