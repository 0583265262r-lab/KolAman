import json
from pathlib import Path
from confluent_kafka import Consumer

from validation import validate
from geo_service import get_region_with_geopandas
from redis_service import is_duplicate

GEOJSON_PATH = "alert-simulator/regions.geojson"
consumer = Consumer({
    "bootstrap.servers": "localhost:9092",
    "group.id": "classifier",
    "auto.offset.reset": "earliest"
})
region = get_region_with_geopandas()
consumer.subscribe(["alerts"])

try:
    while True:
        msg = consumer.poll(1.0)

        if msg is None:
            continue

        if msg.error():
            print(msg.error())
            continue

        try:
            text = msg.value().decode("utf-8")
            alert = json.loads(text)

        except Exception as ex:
            print(f"Bad JSON: {ex}")
            continue
        valid, reason = validate(alert)
        
        if not valid:
            print(f"Invalid alert: {reason}")
            continue
        if is_duplicate(alert["alert_id"]):
            print(f'Duplicate: {alert["alert_id"]}')
            continue
        region = get_region_with_geopandas(GEOJSON_PATH,
                                           alert["lat"],
                                           alert["lon"])
           
finally:
    consumer.close()     

        
                            
        
                
        