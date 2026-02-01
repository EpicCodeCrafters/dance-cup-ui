CREATE TABLE IF NOT EXISTS logs
(
    Timestamp DateTime64(6) DEFAULT toDateTime64('1970-01-01 00:00:00', 6),
    Level String,
    MessageTemplate String,
    RenderedMessage String,
    TraceId String,
    SpanId String,

    ElapsedMilliseconds Nullable(Float64),
    StatusCode Nullable(Int32),
    ContentType Nullable(String),
    ContentLength Nullable(Int32),
    Protocol Nullable(String),
    Method Nullable(String),
    Scheme Nullable(String),
    Host Nullable(String),
    PathBase Nullable(String),
    Path Nullable(String),
    QueryString Nullable(String),
    EventId_Id Nullable(Int32),
    SourceContext Nullable(String),
    RequestId Nullable(String),
    RequestPath Nullable(String),
    ConnectionId Nullable(String),
    Service Nullable(String),
    Environment Nullable(String),

    _kafka_topic Nullable(String),
    _kafka_partition Nullable(Int32),
    _kafka_offset Nullable(UInt64),
    _kafka_timestamp Nullable(DateTime64(3)),

    id UUID DEFAULT generateUUIDv4()    
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(Timestamp)
ORDER BY (Timestamp, TraceId, id)
TTL toDateTime(Timestamp) + INTERVAL 15 MINUTE DELETE;

CREATE TABLE IF NOT EXISTS kafka_messages
(
    value String
)
ENGINE = Kafka
SETTINGS
    kafka_broker_list = 'kafka1:29092,kafka2:29093,kafka3:29094',
    kafka_topic_list = 'dance_cup_logs',
    kafka_group_name = 'clickhouse',
    kafka_format = 'JSONAsString',
    kafka_num_consumers = 1;


CREATE MATERIALIZED VIEW IF NOT EXISTS kafka_messages_to_logs TO logs
AS SELECT
    parseDateTime64BestEffort(
            JSONExtractString(value, 'Timestamp'),
            6
    ) AS Timestamp,

    JSONExtractString(value, 'Level') AS Level,
    JSONExtractString(value, 'MessageTemplate') AS MessageTemplate,
    JSONExtractString(value, 'RenderedMessage') AS RenderedMessage,
    JSONExtractString(value, 'TraceId') AS TraceId,
    JSONExtractString(value, 'SpanId') AS SpanId,

    JSONExtractRaw(value, 'Properties') AS props,

    JSONExtractFloat(props,   'ElapsedMilliseconds')             AS ElapsedMilliseconds,
    JSONExtractInt(props,     'StatusCode')                      AS StatusCode,
    JSONExtractString(props,  'ContentType')                     AS ContentType,
    CASE
        WHEN JSONHas(props, 'ContentLength') = 0 OR JSONExtractString(props, 'ContentLength') = 'null'
            THEN CAST(NULL AS Nullable(Int32))
        ELSE CAST(JSONExtractInt(props, 'ContentLength') AS Nullable(Int32))
    END AS ContentLength,
    JSONExtractString(props,  'Protocol')                        AS Protocol,
    JSONExtractString(props,  'Method')                          AS Method,
    JSONExtractString(props,  'Scheme')                          AS Scheme,
    JSONExtractString(props,  'Host')                            AS Host,
    JSONExtractString(props,  'PathBase')                        AS PathBase,
    JSONExtractString(props,  'Path')                            AS Path,
    JSONExtractString(props,  'QueryString')                     AS QueryString,

    JSONExtractInt(JSONExtractRaw(props, 'EventId'), 'Id')      AS EventId_Id,

    JSONExtractString(props,  'SourceContext')                  AS SourceContext,
    JSONExtractString(props,  'RequestId')                      AS RequestId,
    JSONExtractString(props,  'RequestPath')                    AS RequestPath,
    JSONExtractString(props,  'ConnectionId')                   AS ConnectionId,
    JSONExtractString(props,  'Service')                        AS Service,
    JSONExtractString(props,  'Environment')                    AS Environment,

    _topic   AS _kafka_topic,
    _partition AS _kafka_partition,
    _offset  AS _kafka_offset,
    toDateTime64(_timestamp, 3) AS _kafka_timestamp
   
FROM kafka_messages
WHERE length(value) > 0;

CREATE TABLE IF NOT EXISTS logs_archive
(
    Timestamp DateTime64(6),
    Level String,
    MessageTemplate String,
    RenderedMessage String,
    TraceId String,
    SpanId String,

    ElapsedMilliseconds Nullable(Float64),
    StatusCode Nullable(Int32),
    ContentType Nullable(String),
    ContentLength Nullable(Int32),
    Protocol Nullable(String),
    Method Nullable(String),
    Scheme Nullable(String),
    Host Nullable(String),
    PathBase Nullable(String),
    Path Nullable(String),
    QueryString Nullable(String),
    EventId_Id Nullable(Int32),
    SourceContext Nullable(String),
    RequestId Nullable(String),
    RequestPath Nullable(String),
    ConnectionId Nullable(String),
    Service Nullable(String),
    Environment Nullable(String),

    _kafka_topic Nullable(String),
    _kafka_partition Nullable(Int32),
    _kafka_offset Nullable(UInt64),
    _kafka_timestamp Nullable(DateTime64(3)),

    id UUID
)
ENGINE = S3('http://object-storage:9000/logs-archive/{_partition_id}.parquet', 'admin', 'admin123', 'Parquet')
PARTITION BY toYYYYMM(Timestamp)
ORDER BY (Timestamp, TraceId, id);

CREATE MATERIALIZED VIEW IF NOT EXISTS logs_to_archive TO logs_archive
AS SELECT
    parseDateTime64BestEffort(
            JSONExtractString(value, 'Timestamp'),
            6
    ) AS Timestamp,

    JSONExtractString(value, 'Level') AS Level,
    JSONExtractString(value, 'MessageTemplate') AS MessageTemplate,
    JSONExtractString(value, 'RenderedMessage') AS RenderedMessage,
    JSONExtractString(value, 'TraceId') AS TraceId,
    JSONExtractString(value, 'SpanId') AS SpanId,

    JSONExtractRaw(value, 'Properties') AS props,

    JSONExtractFloat(props,   'ElapsedMilliseconds')             AS ElapsedMilliseconds,
    JSONExtractInt(props,     'StatusCode')                      AS StatusCode,
    JSONExtractString(props,  'ContentType')                     AS ContentType,
    CASE
        WHEN JSONHas(props, 'ContentLength') = 0 OR JSONExtractString(props, 'ContentLength') = 'null'
            THEN CAST(NULL AS Nullable(Int32))
        ELSE CAST(JSONExtractInt(props, 'ContentLength') AS Nullable(Int32))
    END AS ContentLength,
    JSONExtractString(props,  'Protocol')                        AS Protocol,
    JSONExtractString(props,  'Method')                          AS Method,
    JSONExtractString(props,  'Scheme')                          AS Scheme,
    JSONExtractString(props,  'Host')                            AS Host,
    JSONExtractString(props,  'PathBase')                        AS PathBase,
    JSONExtractString(props,  'Path')                            AS Path,
    JSONExtractString(props,  'QueryString')                     AS QueryString,

    JSONExtractInt(JSONExtractRaw(props, 'EventId'), 'Id')      AS EventId_Id,

    JSONExtractString(props,  'SourceContext')                  AS SourceContext,
    JSONExtractString(props,  'RequestId')                      AS RequestId,
    JSONExtractString(props,  'RequestPath')                    AS RequestPath,
    JSONExtractString(props,  'ConnectionId')                   AS ConnectionId,
    JSONExtractString(props,  'Service')                        AS Service,
    JSONExtractString(props,  'Environment')                    AS Environment,

    _topic   AS _kafka_topic,
    _partition AS _kafka_partition,
    _offset  AS _kafka_offset,
    toDateTime64(_timestamp, 3) AS _kafka_timestamp,
    
    generateUUIDv4() AS id
   
FROM kafka_messages
WHERE length(value) > 0;
