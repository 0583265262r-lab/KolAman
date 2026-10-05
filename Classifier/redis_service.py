import redis

client = redis.Redis(
    host="localhost",
    port=6379,
    decode_responses=True
)

TTL_SECONDS = 300


def is_duplicate(alert_id):
    key = f"alert:{alert_id}"
    created = client.set(
        key,
        "1",
        ex=TTL_SECONDS,
        nx=True
    )

    return created is None
