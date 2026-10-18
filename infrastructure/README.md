# Infrastructure

This folder will contain Atlas infrastructure as code (IaC): versioned configuration for provisioning and managing the environments and services needed to run the application.

Runtime SQL repositories, seed support and observability live in [Atlas.Application](../src/Atlas.Application/README.md). There is no runtime Atlas.Infrastructure assembly. Console lives under `src/hosts/Atlas.Console`; hosts select runtime adapters, while this folder remains dedicated to provisioning and environment configuration.
