# BadReview.Shared

This is a shared library project intented to provide a common interface for the Api and Client projects.\
It contains common DTOs, Fluent Validation validators, constants and utilities.

### Folder Structure
```bash
BadReview.Shared/
├── DTOs/                
│   ├── External/           # Used for external services (e.g. IGDB)
│   ├── Request/            # Client requests to the Api
│   │    ├── Validators.cs  # Fluent Validation general rules
│   │    └── ...
│   └── Response/           # Api responses to the Client
└── Utils/                  # Constants and various utilities
```



[Back to main README](../README.md)