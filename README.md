# VM-PrintService

A cross-platform print service that runs as a Windows Service or Linux systemd service.  
It exposes a local REST API on `http://127.0.0.1:1913`.

## Installation

### Windows

Download the `.msi` installer from the [latest GitHub Release](https://github.com/MusikzugSN/VM-PrintService/releases).  
The MSI automatically registers and starts the `VMPrintService` Windows service.

### Linux

Download the `.deb` package from the [latest GitHub Release](https://github.com/MusikzugSN/VM-PrintService/releases), then:

```bash
sudo dpkg -i vm-printservice_*.deb
sudo systemctl enable --now vm-printservice
```

## Usage

### Foreground mode (for testing)

```bash
VM-PrintService --foreground
# or
VM-PrintService -f
```

## API

All endpoints are served on `http://127.0.0.1:1913`.

### `GET /api/health`

Returns the service status and version.

**Response:**

```json
{
  "status": "ok",
  "version": "0.1.2"
}
```

### `GET /api/printers`

Returns a list of all locally available printers.

**Response:**

```json
[
  { "name": "Brother HL-L2350DW", "is_default": true },
  { "name": "Microsoft Print to PDF", "is_default": false }
]
```

### `POST /api/print`

Submits a print job. The service downloads each file from the provided URL and sends it to the specified printer.

**Request:**

```json
{
  "printer": "Brother HL-L2350DW",
  "files": [
    { "url": "https://example.com/document1.pdf" },
    { "url": "https://example.com/document2.pdf", "filename": "report.pdf" }
  ]
}
```

| Field              | Type     | Required | Description                                      |
|--------------------|----------|----------|--------------------------------------------------|
| `printer`          | string   | yes      | Exact printer name as returned by `/api/printers`|
| `files`            | array    | yes      | List of files to print                           |
| `files[].url`      | string   | yes      | URL to download the file from                    |
| `files[].filename` | string   | no       | Filename hint (used for temp file extension)     |

**Response:**

```json
{
  "total": 2,
  "successful": 2,
  "failed": 0,
  "errors": []
}
```
