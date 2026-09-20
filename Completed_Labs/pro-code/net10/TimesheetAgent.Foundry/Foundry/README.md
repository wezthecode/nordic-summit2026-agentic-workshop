# Foundry deployment notes

`agent.yaml` is the deployment manifest consumed by `az foundry agent deploy`. The schema is sketched against the current preview surface and will be refined during rehearsal week — see the TODO blocks in the YAML.

## Deploy

```bash
dotnet publish -c Release
cd Foundry
az foundry agent deploy --file agent.yaml
```

## Invoke

```bash
ENDPOINT=$(az foundry agent show --name timesheet-orchestrator --query endpoint -o tsv)
TOKEN=$(az account get-access-token --resource <foundry-audience> --query accessToken -o tsv)

curl -X POST "$ENDPOINT/invoke" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"message":"Log my time for last Tuesday."}'
```

## Tear down

```bash
az foundry agent delete --name timesheet-orchestrator
```

Cheap to redeploy. Don't sit on idle deployments between rehearsal sessions.
