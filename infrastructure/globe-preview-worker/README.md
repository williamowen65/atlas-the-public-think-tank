# Globe preview worker

PTT-187 runs the globe GIF and static globe views in a one-shot container.
The worker runs Chromium in its own container.
Globe preview requests serve published images, cached previews, or a static
placeholder. The Atlas API captures other SEO images on demand.

The GIF capture hides latitude and longitude lines and uses the darker
ocean edge color across the globe. Static previews and the interactive
globe keep the grid and ocean gradient. Local Docker and Azure both encode
one 1200 × 630 GIF with a 70-second rotation at 15 FPS, a 128-color palette,
and Gifsicle's `--lossy=110` and `--no-conserve-memory` flags.
The worker publishes only GIFs strictly under 7 MB. The previous
generation remains current if encoding fails or the GIF reaches the budget.

The worker publishes the complete set under a new generation ID before changing
the current-generation pointer. The public globe metadata references that
generation in the image URL. A failed run leaves the current pointer intact.
Concurrent executions can still spend compute, but a conditional pointer update
rejects a competing publication. Check execution history before starting another
run. The first trial is manual.

## Test locally with Docker Desktop and PowerShell

Use Docker Desktop in Linux-container mode, your usual local Atlas database,
and two PowerShell terminals. Start both terminals at the repository root.

### 1. Build the frontend and start the API

In the first terminal, build the frontend into the API's web root and start
Atlas with the local preview storage directory.

```powershell
Push-Location src/hosts/Atlas.Web
npm ci
npm run build
Pop-Location

$env:Atlas__Seo__CaptureDirectory = Join-Path (Get-Location).Path 'globe-output'
dotnet run --project src/hosts/Atlas.Api --launch-profile https
```

Keep this terminal running. Open [the local globe](https://localhost:7240/?globe=open)
and confirm it renders. If another Atlas API is using port 7240, stop it before
starting this instance.

### 2. Build and run the worker

In the second terminal, build the image from the current branch. Wait for the
build to succeed before running the worker.

```powershell
docker build -f infrastructure/globe-preview-worker/Dockerfile -t atlas-globe-preview .
```

Run the following commands in order. Keep the entire `docker run` command on
one line, including all three `-e` options. If PowerShell shows the `>>`
continuation prompt after a partial paste, press **Ctrl+C** and paste the
complete command again.

```powershell
$output = (New-Item -ItemType Directory -Force globe-output).FullName
docker run --rm --mount "type=bind,source=$output,target=/output" -e "ASPNETCORE_ENVIRONMENT=Development" -e "Atlas__Seo__CaptureOrigin=https://host.docker.internal:7240" -e "Atlas__Seo__CaptureDirectory=/output" atlas-globe-preview
```

Docker starts the capture immediately. The worker logs the GIF size in MB. **Published globe preview generation** confirms the current generation was updated.
Development capture accepts the local development certificate. On Linux
Docker Engine, add `--add-host=host.docker.internal:host-gateway` and bind
Atlas to an interface reachable from the container.

### 3. View the published GIF

In the second terminal, get the published file and its size in decimal MB.

```powershell
$generation = (Get-Content .\globe-output\globe-published\current -Raw).Trim()
$gif = Get-Item ".\globe-output\globe-published\$generation\globe-motion.gif"
$gif.FullName
'{0:F2} MB' -f ($gif.Length / 1000000)
```

Open that file in a browser to judge the motion. To check what Atlas serves,
view the page source for `https://localhost:7240/?globe=open&motion=gif`
and open the `og:image` URL. It should reference the same generation.

For artwork changes, rebuild the frontend and restart the API. For worker
code or Dockerfile changes, rebuild the image before running it again. To
verify failure behavior, stop Atlas, run the worker again, then restart
Atlas and confirm the last published generation is still available.

## Enable on the redis03 demo rehearsal

1. Build and push the Docker image to a public GHCR package. Use **Build Atlas
   globe preview worker** once that workflow is available for manual dispatch,
   or build and push with Docker from the worker branch. Set the Actions variable
   `ATLAS_GLOBE_PREVIEW_WORKER_IMAGE` to its `ghcr.io/…@sha256:…` reference.
2. Run the Terraform plan and reviewed apply on this branch using
   `installation=rehearsal`, `rehearsal_name=redis03`,
   `deployment_shape=demo`. Plan and Apply provision the worker for `redis03`
   automatically. Both require the image digest configured in step 1. The job shares the rehearsal Log Analytics
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
