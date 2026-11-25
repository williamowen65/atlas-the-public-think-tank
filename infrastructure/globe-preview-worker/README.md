# Globe preview worker

PTT-187 runs the globe GIF and static globe views in a one-shot container.
The worker uses the v14 GIF settings and runs Chromium in its own container.
Other SEO images continue using the existing Atlas API capture path.

The worker first optimizes a full v14 rotation with a shared 128-color
palette and changed-pixel encoding. If that exceeds 7 MB, it samples the
same captured rotation at progressively wider intervals while keeping the
display frame rate near 6 FPS. This speeds up the rotation and preserves
the 1200 × 630 canvas. Logs show each candidate's loop duration and size.
A GIF that exceeds the budget at every tested speed fails the execution
before it publishes a new generation.

The worker publishes the complete set under a new generation ID before changing
the current-generation pointer. The public globe metadata references that
generation in the image URL. A failed run leaves the current pointer intact.
Concurrent executions can still spend compute, but a conditional pointer update
rejects a competing publication. Check execution history before starting another
run. The first trial is manual. Pin-count triggers and the 12 FPS trial come
after baseline verification.

## Test with Docker

Start Atlas with its normal local database and frontend. Give both the API
and worker the same capture directory. In the API terminal, set the following
before starting Atlas. Use an absolute path.

```powershell
$env:Atlas__Seo__ExternalGlobeCapture = 'true'
$env:Atlas__Seo__CaptureDirectory = Join-Path (Get-Location) 'globe-output'
```

In another terminal at the repo root:

```powershell
docker build -f infrastructure/globe-preview-worker/Dockerfile -t atlas-globe-preview .
$output = (New-Item -ItemType Directory -Force globe-output).FullName
docker run --rm --mount "type=bind,source=$output,target=/output" -e ASPNETCORE_ENVIRONMENT=Development -e Atlas__Seo__CaptureOrigin=https://host.docker.internal:7240 -e Atlas__Seo__CaptureDirectory=/output atlas-globe-preview
```

Use a host address and port reachable from Docker. On Linux Docker Engine,
add `--add-host=host.docker.internal:host-gateway` and bind the local Atlas
listener to an interface reachable from the container. Development capture
accepts the local development certificate. Inspect the globe page HTML and
open its `og:image` URL. Run again to check the URL changes. Stop Atlas before
another run to force a capture failure, restart it, and verify the old URL
and file are still served.

## Enable on the redis03 demo rehearsal

1. Build and push the Docker image to a public GHCR package. Use **Build Atlas
   globe preview worker** once that workflow is available for manual dispatch,
   or build and push with Docker from the worker branch. Set the Actions variable
   `ATLAS_GLOBE_PREVIEW_WORKER_IMAGE` to its `ghcr.io/…@sha256:…` reference.
2. Run the Terraform plan and reviewed apply on this branch using
   `installation=rehearsal`, `rehearsal_name=redis03`,
   `deployment_shape=demo`, and `enable_globe_preview_worker=true`. Use the same
   input on subsequent applies for this rehearsal to retain the job. The optional job shares the rehearsal Log Analytics
   workspace and uses its own managed identity. The regular demo uses a
   separate Terraform installation. Review its inputs before using the variable there.
3. Allow **Atlas infra apply** to assign **Storage Blob Data Contributor** to
   the worker managed identity on the private `share-images` container. The
   existing `redis03` delegation may allow only the web identity. An Azure
   administrator must extend that condition. If the worker identity does not
   exist yet, the first apply can create it before failing on its grant. Use its
   principal ID to extend the condition, then run a fresh plan and apply.
4. Deploy the worker branch to the rehearsal demo App Service so it serves
   published globe images. In Azure Portal, open the **Container Apps Job** in
   `atlas-rehearsal-redis03-rg` and choose **Start**. Use **Execution history**
   for status and **Monitoring → Logs** in the Container Apps environment for
   progress. The job logs each capture, GIF encoding and upload, and successful
   generation publication. App Service logs still cover web requests.

For execution status from the Azure CLI:

```powershell
az containerapp job execution list --resource-group atlas-rehearsal-redis03-rg --name <job-name> --output table
```

Record elapsed time, peak memory, GIF size, and Atlas/SQL activity while the
job runs. Check that the globe metadata has a new generation and that both the
PNG and GIF URLs return the expected media types. Verify ordinary node share
images still use their existing capture path. Published worker generations are
outside the seven-day cache rule so the current generation remains available.
Review older and incomplete generations before enabling recurring runs.
