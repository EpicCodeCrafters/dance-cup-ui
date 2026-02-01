# ClickHouse Log Archiving to S3 (MinIO)

## Overview
This setup implements automatic log archiving from ClickHouse to S3 (MinIO) storage using the S3 table engine. Logs are written to both local storage (for fast queries) and S3 (for permanent archival) simultaneously. Local logs are deleted after 15 minutes.

## Architecture

### Components
1. **Main Logs Table** (`logs`): 
   - Stores incoming logs from Kafka
   - Storage: Local ClickHouse storage
   - TTL: 15 minutes (deleted after)
   
2. **Archive Table** (`logs_archive`):
   - Stores archived logs permanently in S3/MinIO
   - Uses S3 table engine
   - Storage: S3 (MinIO) bucket `logs-archive`
   - Retention: Permanent (no TTL)

3. **Materialized Views**:
   - `kafka_messages_to_logs`: Writes logs from Kafka to local `logs` table
   - `logs_to_archive`: Writes logs from Kafka to S3 `logs_archive` table

### Data Flow
```
Kafka (dance_cup_logs) 
  → kafka_messages (Kafka Engine) 
    ├─→ kafka_messages_to_logs (MV) → logs (MergeTree, local, TTL 15 min)
    └─→ logs_to_archive (MV) → logs_archive (S3 engine, permanent)
```

## Configuration

### S3 Table Configuration
The `logs_archive` table uses the S3 engine:
- Endpoint: `http://object-storage:9000/logs-archive/{_partition_id}.parquet`
- Format: Parquet (optimized for storage)
- Credentials: admin/admin123

### MinIO Bucket
Bucket `logs-archive` is automatically created by `object-storage-init` service in docker-compose.yml

## How It Works

1. Logs are received from Kafka topic `dance_cup_logs`
2. Two materialized views process logs in parallel:
   - `kafka_messages_to_logs`: Writes to local `logs` table (fast queries, 15 min retention)
   - `logs_to_archive`: Writes to S3 `logs_archive` table (permanent archival)
3. After 15 minutes, logs in the local `logs` table are automatically deleted by TTL
4. Archived logs remain permanently accessible in S3 via the `logs_archive` table

## Verification

To verify the setup is working:

1. Check that logs are being written to the local table:
```sql
SELECT count() FROM logs;
```

2. Check that logs are being archived to S3:
```sql
SELECT count() FROM logs_archive;
```

3. Access MinIO console at http://localhost:9001 (admin/admin123) and verify the `logs-archive` bucket contains Parquet files.

## Benefits

- **Immediate Archiving**: All logs are written to S3 immediately
- **Fast Queries**: Recent logs (last 15 minutes) available on local storage for fast queries
- **Permanent Retention**: All logs preserved indefinitely in S3
- **Cost-Effective**: S3 storage is cheaper than local storage for long-term retention
- **Transparent**: Query either table depending on your needs (recent vs historical logs)
- **Reliable**: Uses native S3 table engine instead of tiered storage
