# Local Deployment via Cloudflare Tunnel

This guide explains how to expose your local `Analytics_BE` server to the internet securely using Cloudflare Tunnels (Argo Tunnel).

## Prerequisites

1.  **Cloudflare Account**: A free account with a registered domain.
2.  **cloudflared CLI**: Download and install from [Cloudflare's website](https://developers.cloudflare.com/cloudflare-one/connections/connect-apps/install-and-setup/installation/).

## Setup Instructions

### 1. Authenticate Cloudflared
Open your terminal and run:
```bash
cloudflared tunnel login
```
This will open a browser to authenticate your account and select your domain.

### 2. Create the Tunnel
Create a tunnel named `analytics-be`:
```bash
cloudflared tunnel create analytics-be
```
**Note the Tunnel ID** provided in the output (a long string like `ad7...`).

### 3. Configure the Tunnel
Copy `Deployment/Cloudflare/config.yml.example` to `config.yml` (keep it in a secure location or the Deployment folder) and update:
- `tunnel`: Your Tunnel ID.
- `credentials-file`: The path to the JSON file created in step 2 (usually in `%USERPROFILE%\.cloudflared\<ID>.json`).
- `hostname`: The domain/subdomain you want to use (e.g., `api.example.com`).

### 4. Route DNS
Route your domain to the tunnel:
```bash
cloudflared tunnel route dns analytics-be api.example.com
```

### 5. Start the Tunnel
To start the tunnel and expose your local API:
```bash
cloudflared tunnel run analytics-be
```

## Running the Application
Ensure your .NET application is running locally on port **5126**:
```bash
cd WebAPI
dotnet run --launch-profile http
```

## Security Note
Since this exposes your local machine to the internet:
1.  Keep your `credentials.json` private.
2.  Ensure your `RequireAuthMiddleware` is active on all non-public endpoints (we have already configured this).
3.  The tunnel uses HTTPS automatically on the Cloudflare side, even if your local app uses HTTP.
