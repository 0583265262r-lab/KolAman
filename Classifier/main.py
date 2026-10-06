import json
from pathlib import Path
from confluent_kafka import Consumer
import logging
import geopandas as gpd
from shapely.geometry import Point


from validation import validate
from geo_service import get_region_with_geopandas
from redis_service import is_duplicate
from elastic_service import log_important
from rabbit_service import RabbitService


logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s %(levelname)s %(name)s - %(message)s"
)
logger = logging.getLogger("classifier")


GEOJSON_PATH = "C:/Users/user1/OneDrive/שולחן העבודה/final test/alert-simulator/regions.geojson"

consumer = Consumer({
    "bootstrap.servers": "localhost:9092",
    "group.id": "classifier",
    "auto.offset.reset": "earliest"
})


consumer.subscribe(["alerts"])
rabbit = RabbitService()

logger.info("Classifier started and listening to Kafka topic alerts")
log_important("ServiceStarted", message="Classifier started")


try:
    while True:
        msg = consumer.poll(1.0)

        if msg is None:
            continue

        if msg.error():
            logger.error("Kafka consumer error: %s", msg.error())
            log_important("KafkaConsumerError", message=str(msg.error()))
            continue
        logger.info("Kafka message received")

        try:
            text = msg.value().decode("utf-8")
            alert = json.loads(text)
            logger.info("JSON deserialized successfully")
        except Exception as ex:
            logger.warning("Bad JSON received: %s", ex)
            log_important("AlertRejected", message=f"Bad JSON: {ex}")
            continue
        

        valid, reason = validate(alert)
        
        if not valid:
            logger.warning(
                "Alert %s rejected: %s",
                alert.get("alert_id"),
                reason
                )
            log_important(
                "AlertRejected",
                alert.get("alert_id"),
                message=reason
                )
            continue
        logger.info("Alert %s passed validation", alert["alert_id"])
        try:
            duplicate = is_duplicate(alert["alert_id"])
        except Exception as ex:
            logger.error("Redis failed for alert %s: %s", alert["alert_id"], ex)
            log_important(
                "RedisError",
                alert["alert_id"],
                message=str(ex)
                )
            continue
        
        if duplicate:
            logger.warning("Duplicate alert ignored: %s", alert["alert_id"])
            log_important("DuplicateAlert", alert["alert_id"])
            continue
        print(alert["lat"],alert["lon"])
        command = get_region_with_geopandas(GEOJSON_PATH,
                                        alert["lon"],
                                        alert["lat"]
                                           )
        logger.info("Alert %s classified as %s", alert["alert_id"], command)
        try:
            rabbit.send(alert, command)
            log_important(
                "AlertRouted",
                alert["alert_id"],
                command,
                "Alert was classified and sent to RabbitMQ"
                )
        except Exception as ex:
            logger.error("RabbitMQ failed for alert %s: %s", alert["alert_id"], ex)
            log_important(
                "RabbitError",
                alert["alert_id"],
                command,
                str(ex)
                    )      
finally:
    consumer.close()
    rabbit.close()    

        
                            
        
                
        