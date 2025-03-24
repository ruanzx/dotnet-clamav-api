# Dotnet ClamAV File Scan API

This project provides a RESTful API for scanning files using ClamAV. It allows users to upload files and scan them for viruses and malware. The API is built using ASP.NET Core and targets .NET 8.

## Features

- Upload single or multiple files for scanning
- Scan files with additional metadata
- Integration with ClamAV for virus scanning

## Getting Started

### Prerequisites

- .NET 8 SDK
- ClamAV installed on the server

### Installation

1. Clone the repository:
   
```bash
git clone https://github.com/ruanzx/dotnet-clamav-api.git
cd dotnet-clamav-api
```

2. Build the project:
   
```
docker compose build
```

3. Run the project:

```
docker compose up -d
```

### Usage

#### Upload and Scan Files

To upload and scan files, send a POST request to `http://localhost:32772/clamavscan/scan-files` with the files and optional metadata in a multipart form.

Example using `curl`:

```bash
curl -X POST "http://localhost:32772/clamavscan/scan-files" \
  -F "Files=@path/to/your/file1.txt" \
  -F "Files=@path/to/your/file2.txt" \
  -F "RequestId=123" \
  -F "Description=Test file upload"
```

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any changes.

## License

This project is licensed under the MIT License.