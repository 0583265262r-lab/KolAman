from datetime import datetime, timezone
from elasticsearch import Elasticsearch
import logging

logger = logging.getLogger("classifier.elastic")
es = Elasticsearch("http://localhost:9200")


def log_important(event_type, alert_id=None, command=None, message=None):

    document = {
        "service": "Classifier",
        "eventType": event_type,
        "timestamp": datetime.now(timezone.utc).isoformat()
    }

    if alert_id is not None:
        document["alertId"] = alert_id

    if command is not None:
        document["command"] = command

    if message is not None:
        document["message"] = message

    try:
        es.index(index="kolaman-logs", document=document)
    except Exception as ex:
        logger.warning("Could not write important log to Elasticsearch: %s", ex)
