# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/) and this project adheres to [Semantic Versioning](http://semver.org/).

## [1.1.0] - 2026-03-25

Changes:
- Bump Mediator.Abstractions from 3.0.1 to 3.0.2
- Add IRequestAuthorizationResultAccessor api for accessing the authorization metadata after the pipeline has successfully run.
- Add support for registering request-builders and requirement-handlers via delegates.

## [1.0.0] - 2026-03-08

Initial release of:
- Jameak.RequestAuthorization.Core
- Jameak.RequestAuthorization.Adapter.AspNetCore
- Jameak.RequestAuthorization.Adapter.Mediator
- Jameak.RequestAuthorization.Adapter.MediatR
