from datetime import datetime

VALID_PRIORITIES = {"CRITICAL", "HIGH", "MEDIUM", "LOW"}
VALID_CLASSIFICATIONS = {
    "UNCLASSIFIED",
    "RESTRICTED",
    "SECRET",
    "TOP_SECRET"
}
VALID_SOURCES = {"aman", "mossad", "pikud-haoref", "shabak"}

REQUIRED_FIELDS = {
    "alert_id",
    "source",
    "title",
    "content",
    "priority",
    "classification",
    "lat",
    "lon",
    "timestamp",
    "status"
}


def validate(alert):
    if not isinstance(alert, dict):
        return False, "alert is not an object"

    missing = REQUIRED_FIELDS - alert.keys()
    if missing:
        return False, f"missing fields: {sorted(missing)}"

    if not isinstance(alert["alert_id"], str) or not alert["alert_id"].strip():
        return False, "invalid alert_id"

    if alert["source"] not in VALID_SOURCES:
        return False, "invalid source"

    if not isinstance(alert["title"], str) or not alert["title"].strip():
        return False, "invalid title"

    if not isinstance(alert["content"], str) or not alert["content"].strip():
        return False, "invalid content"

    if isinstance(alert["lat"], bool) or not isinstance(alert["lat"], (int, float)):
        return False, "lat must be a number"

    if isinstance(alert["lon"], bool) or not isinstance(alert["lon"], (int, float)):
        return False, "lon must be a number"

    if not -90 <= alert["lat"] <= 90:
        return False, "lat out of range"

    if not -180 <= alert["lon"] <= 180:
        return False, "lon out of range"

    if alert["priority"] not in VALID_PRIORITIES:
        return False, "invalid priority"

    if alert["classification"] not in VALID_CLASSIFICATIONS:
        return False, "invalid classification"

    if alert["status"] != "WAITING":
        return False, "invalid status"

    try:
        datetime.fromisoformat(alert["timestamp"].replace("Z", "+00:00"))
    except Exception:
        return False, "invalid timestamp"

    return True, ""
