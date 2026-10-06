# KolAman - Simple test version with logging

## Workflow

alert-simulator -> NotificationGate (C#) -> Kafka -> Classifier (Python)
-> Validation + deduplication in Redis + geographic classification
-> RabbitMQ queues -> CommandWorker (C#) -> MySQL

## Execution

1. Start the infrastructure from the project root directory:

```powershell
docker compose up -d
```

2. CommandWorker:

```powershell
cd CommandWorker
dotnet restore
dotnet run
```

3. Classifier:

```powershell
cd Classifier
py -m venv .venv
.venv\Scripts\python.exe -m pip install -r requirements.txt
python main.py
```

4. NotificationGate:

```powershell
cd NotificationGate
dotnet restore
dotnet run
```

5. Simulator:

```powershell
cd alert-simulator
python simulator.py
```
## Why did I choose SQL?.
I chose SQL because the incoming data is not expected to change and always retains the same structure; furthermore, I implemented validations to ensure that consistency. My reasoning was also based on the fact that SQL is a relational database—there are relationships between the tables, and ultimately, they all represent the same underlying subject matter, even if they are segmented according to user needs. Additionally, SQL makes it easier to retrieve, update, and manage this data.

## MySQL - Separate table for each command
The `CommandWorker` now saves each alert to a different MySQL table based on the `command`:

- `NORTH` -> `north_alerts`
- `CENTER` -> `center_alerts`
- `SOUTH` -> `south_alerts`
- `OVERSEAS` -> `overseas_alerts`