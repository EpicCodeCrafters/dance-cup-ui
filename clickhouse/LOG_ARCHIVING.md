# ClickHouse Log Archiving to S3 (MinIO)

## Overview
This setup implements automatic log archiving from ClickHouse to S3 (MinIO) storage with a 5-minute TTL for the main logs table.

## Architecture

### Components
1. **Main Logs Table** (`logs`): 
   - Stores incoming logs from Kafka
   - TTL: 5 minutes
   - Storage: Local ClickHouse storage
   
2. **Archive Table** (`logs_archive`):
   - Stores archived logs permanently
   - Storage: S3 (MinIO) via storage policy `s3_main`
   - Bucket: `logs-archive`

3. **Materialized View** (`logs_to_archive`):
   - Automatically archives all logs from Kafka to `logs_archive`
   - Runs in real-time as logs arrive from Kafka
   - Parallel to the main logs ingestion pipeline

### Data Flow
```
Kafka (dance_cup_logs) 
  → kafka_messages (Kafka Engine) 
    → kafka_messages_to_logs (Materialized View) → logs (MergeTree, TTL 5 min)
    → logs_to_archive (Materialized View) → logs_archive (MergeTree on S3)
```

## Configuration

### S3 Storage Configuration
File: `clickhouse/s3_config.xml`
- Endpoint: `http://object-storage:9000/logs-archive/`
- Access Key: `admin`
- Secret Key: `admin123`
- Storage Policy: `s3_main`

### MinIO Bucket
Bucket `logs-archive` is automatically created by `object-storage-init` service in docker-compose.yml

## How It Works

1. Logs are received from Kafka topic `dance_cup_logs`
2. Two materialized views process the Kafka messages in parallel:
   - `kafka_messages_to_logs`: Parses and inserts logs into the `logs` table (local storage)
   - `logs_to_archive`: Parses and inserts logs into the `logs_archive` table (S3 storage)
3. After 5 minutes, logs in the `logs` table are automatically deleted by TTL
4. Archived logs remain permanently in S3 storage (MinIO)

## Verification

To verify the setup is working:

1. Check that logs are being written to the main table:
```sql
SELECT count() FROM logs;
```

2. Check that logs are being archived to S3:
```sql
SELECT count() FROM logs_archive;
```

3. Access MinIO console at http://localhost:9001 (admin/admin123) and verify the `logs-archive` bucket contains data.

## Benefits

- **Cost Efficiency**: Only recent logs (5 minutes) are stored in fast local storage
- **Data Retention**: All logs are preserved permanently in S3
- **Automatic**: No manual intervention required for archiving
- **Scalable**: S3 storage can grow indefinitely without impacting ClickHouse performance
