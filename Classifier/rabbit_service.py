import json
import logging
import pika

logger = logging.getLogger("classifier.rabbit")


class RabbitService:
    def __init__(self):
        self.connection = pika.BlockingConnection(
            pika.ConnectionParameters("localhost")
        )
        self.channel = self.connection.channel()
        for command in ["NORTH", "CENTER", "SOUTH", "OVERSEAS"]:
            queue = f"{command.lower()}.alerts"
            self.channel.queue_declare(queue=queue, durable=True)

        logger.info("RabbitMQ connected and queues are ready")

    def send(self, alert, command):
        queue = f"{command.lower()}.alerts"

        message = dict(alert)
        message["command"] = command

        logger.info(
            "Sending alert %s to RabbitMQ queue %s",
            alert.get("alert_id"),
            queue
        )

        self.channel.basic_publish(
            exchange="",
            routing_key=queue,
            body=json.dumps(message, ensure_ascii=False).encode("utf-8"),
            properties=pika.BasicProperties(
                delivery_mode=2,
                content_type="application/json"
            )
        )

        logger.info("Alert sent to RabbitMQ queue %s", queue)

    def close(self):
        if self.connection and self.connection.is_open:
            self.connection.close()
