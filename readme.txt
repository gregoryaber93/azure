resource-group: rg-learning-dev
web-service-name: webapp-learning-1
functions-name: func-learning-grzegorz123

========================================================================================================

##WebApi Serbice:

dotnet publish -c Release -o ./publish
Compress-Archive -Path .\publish\* -DestinationPath .\publish\app.zip

az webapp deploy --name webapp-learning-1 --resource-group rg-learning-dev --src-path publish/app.zip

https://webapp-learning-1.azurewebsites.net/api/test/get
https://webapp-learning-1.azurewebsites.net/api/test/post

========================================================================================================

##Functions

func azure functionapp publish func-learning-grzegorz123




Ostatnie wykonania funkcji Test (czy w ogóle się odpala)
requests
| where timestamp > ago(24h)
| where name has "Test" or operation_Name has "Test"
| project timestamp, name, operation_Name, success, resultCode, duration, operation_Id
| order by timestamp desc

Błędy i wyjątki z funkcji
exceptions
| where timestamp > ago(24h)
| where operation_Name has "Test" or outerMessage has "Test" or innermostMessage has "Test"
| project timestamp, type, outerMessage, innermostMessage, operation_Name, operation_Id
| order by timestamp desc

Logi z queue triggera (Twoje nowe logi)
traces
| where timestamp > ago(24h)
| where message has "Queue trigger invoked." or message startswith "Payload:"
| project timestamp, severityLevel, message, operation_Id, operation_Name,
QueueMessageId = tostring(customDimensions["QueueMessageId"]),
DequeueCount = tostring(customDimensions["DequeueCount"]),
OperationIdFromMsg = tostring(customDimensions["OperationId"])

| order by timestamp desc

Wiadomości, które były retry (DequeueCount > 1)
traces
| where timestamp > ago(24h)
| extend DequeueCount = toint(customDimensions["DequeueCount"])
| where isnotempty(DequeueCount) and DequeueCount > 1
| project timestamp, message, DequeueCount, operation_Id,
QueueMessageId = tostring(customDimensions["QueueMessageId"]),
OperationIdFromMsg = tostring(customDimensions["OperationId"])
| order by DequeueCount desc, timestamp desc

Korelacja po OperationId z payloadu (gdy chcesz śledzić jedną wiadomość)
let opId = "TU_WKLEJ_OPERATION_ID";
traces
| where timestamp > ago(24h)
| where tostring(customDimensions["OperationId"]) == opId or operation_Id == opId
| project timestamp, itemType, severityLevel, message, operation_Id, operation_Name,
QueueMessageId = tostring(customDimensions["QueueMessageId"]),
DequeueCount = tostring(customDimensions["DequeueCount"])
| order by timestamp asc

Jedna oś czasu: requests + traces + exceptions razem
let lookback = 24h;
union isfuzzy=true
(
requests
| where timestamp > ago(lookback)
| project timestamp, itemType="request", operation_Id, operation_Name, message=name, success, resultCode
),
(
traces
| where timestamp > ago(lookback)
| project timestamp, itemType="trace", operation_Id, operation_Name, message, success=bool(null), resultCode=""
),
(
exceptions
| where timestamp > ago(lookback)
| project timestamp, itemType="exception", operation_Id, operation_Name, message=coalesce(innermostMessage, outerMessage), success=bool(null), resultCode=""
)
| where operation_Name has "Test" or message has "Queue trigger" or message has "Payload"
| order by timestamp asc